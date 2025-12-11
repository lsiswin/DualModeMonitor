using System.Collections.Concurrent;
using System.IO.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using MonitorLibrary.Models.Enums;
using MonitorLibrary.Reactive;
using MonitorRabbitMQService.Models;
using NModbus;
using NModbus.Serial;
using NModbus.Utility; // 必须引用：用于数据转换
using OpcUaMonitorServer.Configuration;
using OpcUaMonitorServer.Model;

namespace OpcUaMonitorServer.Services
{
    public interface IDataCollectionService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
        OpcDataMessage? GetLatestData(int deviceId, int dataPointId);
    }

    public class DataCollectionService : IDataCollectionService, IDisposable
    {
        private readonly IDeviceManagementService _deviceManager;
        private readonly ISensorDataPublisher _dataPublisher;
        private readonly IOpcUaServerService? _opcServer;
        private readonly ReactiveLogger _logger;
        private readonly DataCollectionConfiguration _config;

        // Modbus 工厂
        private readonly ModbusFactory _modbusFactory;

        // 核心改变：按串口名(如 "COM1")缓存资源，而不是按设备ID
        // Key: PortName, Value: (SerialPort对象, ModbusMaster对象, 线程锁)
        private readonly ConcurrentDictionary<string, SerialPortContext> _serialContexts = new();

        private readonly ConcurrentDictionary<string, OpcDataMessage> _latestData = new();
        private CancellationTokenSource? _cts;
        private Task? _collectionTask;

        // 内部类：用于管理串口上下文
        private class SerialPortContext : IDisposable
        {
            public SerialPort Port { get; }
            public IModbusSerialMaster Master { get; }
            public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1); // 保证同一串口串行访问

            public SerialPortContext(SerialPort port, IModbusSerialMaster master)
            {
                Port = port;
                Master = master;
            }

            public void Dispose()
            {
                Master?.Dispose(); // Master通常会Dispose底层的Stream，但最好显式处理
                if (Port.IsOpen)
                    Port.Close();
                Port.Dispose();
                Lock.Dispose();
            }
        }

        public DataCollectionService(
            IDeviceManagementService deviceManager,
            ISensorDataPublisher dataPublisher,
            IOptions<DataCollectionConfiguration> config,
            ReactiveLogger logger,
            IOpcUaServerService? opcServer = null
        )
        {
            _deviceManager = deviceManager;
            _dataPublisher = dataPublisher;
            _modbusFactory = new ModbusFactory();
            _opcServer = opcServer;
            _config = config.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("启动数据采集服务...");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // 使用 Task.Run 并确保存储 Task 引用
            _collectionTask = Task.Run(() => CollectionLoopAsync(_cts.Token), _cts.Token);
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("停止数据采集服务...");
            _cts?.Cancel();

            if (_collectionTask != null)
            {
                try
                {
                    await _collectionTask;
                }
                catch (OperationCanceledException) { }
            }

            // 清理所有串口资源
            foreach (var context in _serialContexts.Values)
            {
                context.Dispose();
            }
            _serialContexts.Clear();
        }

        private async Task CollectionLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await CollectAllDeviceDataAsync(token);
                    await Task.Delay(_config.ScanIntervalMs, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("数据采集循环发生异常", ex);
                    await Task.Delay(1000, token); // 出错后稍作暂停
                }
            }
        }

        private async Task CollectAllDeviceDataAsync(CancellationToken token)
        {
            var devices = await _deviceManager.GetDevicesAsync();
            // 并发处理不同设备，但在底层我们会通过锁来控制同一串口的并发
            var tasks = devices
                .Where(d => d.IsEnabled)
                .Select(device => ProcessDeviceAsync(device, token));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessDeviceAsync(DeviceInfo device, CancellationToken token)
        {
            try
            {
                // 1. 获取该设备对应的串口上下文
                var context = GetOrCreateSerialContext(device);
                if (context == null)
                    return;

                // 2. 关键：进入锁 (同一COM口的设备必须排队)
                await context.Lock.WaitAsync(token);
                try
                {
                    // 获取点位
                    var dataPoints = await _deviceManager.GetDataPointsAsync(device.Id);
                    byte slaveId = (byte)device.PortConfig.DeviceAddress;

                    foreach (var dataPoint in dataPoints.Where(dp => dp.IsEnable))
                    {
                        if (token.IsCancellationRequested)
                            break;

                        try
                        {
                            // 3. 读取数据
                            double rawValue = await ReadDataPointAsync(
                                context.Master,
                                slaveId,
                                dataPoint
                            );

                            // 4. 处理和发布数据
                            await PublishDataAsync(device, dataPoint, rawValue);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                $"读取点位失败 [{device.Name} - {dataPoint.Name}]: {ex.Message}"
                            );
                        }
                    }
                }
                finally
                {
                    // 5. 释放锁
                    context.Lock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"处理设备失败 [{device.Name}]: {ex.Message}");
            }
        }

        // 管理串口连接的核心方法
        private SerialPortContext? GetOrCreateSerialContext(DeviceInfo device)
        {
            var config = device.PortConfig;
            if (config == null)
                return null;

            string portName = config.PortName.ToUpper();

            // 如果已存在，直接返回
            if (_serialContexts.TryGetValue(portName, out var context))
            {
                if (context.Port.IsOpen)
                    return context;
                context.Dispose(); // 如果端口意外关闭，清理并重建
                _serialContexts.TryRemove(portName, out _);
            }

            try
            {
                // 创建原生 SerialPort
                var port = new SerialPort(
                    portName,
                    (int)config.BaudRate,
                    config.Parity,
                    (int)config.DataBits,
                    config.StopBits
                );

                port.Open();

                var master = _modbusFactory.CreateRtuMaster(port);

                // 设置超时
                master.Transport.ReadTimeout = 1000;
                master.Transport.WriteTimeout = 1000;

                var newContext = new SerialPortContext(port, master);
                _serialContexts.TryAdd(portName, newContext);

                _logger.LogInformation($"串口 {portName} 已打开并初始化 Modbus Master");
                return newContext;
            }
            catch (Exception ex)
            {
                _logger.LogError($"无法打开串口 {portName}: {ex.Message}");
                return null;
            }
        }

        private async Task PublishDataAsync(
            DeviceInfo device,
            DataPointInfo dataPoint,
            double value
        )
        {
            double finalValue = value * dataPoint.Scale + dataPoint.Offset;

            var msg = new OpcDataMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Name = device.Name,
                DataPointId = dataPoint.Id,
                Value = finalValue,
                Timestamp = DateTime.Now,
                Quality = "Good",
                DataPointCode = dataPoint.Code,
                DataType = dataPoint.DataType.ToString(),
                CommandType = OpcCommandType.Read,
            };

            // 缓存
            _latestData[$"{device.Id}_{dataPoint.Id}"] = msg;

            // OPC 更新
            _opcServer?.UpdateDataPointValue(device.Id, dataPoint.Id, finalValue, msg.Timestamp);

            // MQ 推送
            await _dataPublisher.PublishAsync(msg);
        }

        public OpcDataMessage? GetLatestData(int deviceId, int dataPointId)
        {
            _latestData.TryGetValue($"{deviceId}_{dataPointId}", out var data);
            return data;
        }

        // === 数据读取逻辑重构 ===
        private async Task<double> ReadDataPointAsync(
            IModbusSerialMaster master,
            byte slaveId,
            DataPointInfo dp
        )
        {
            ushort addr = (ushort)dp.Address;
            var type = dp.DataType;

            // 1. 线圈和离散输入 (位操作)
            if (type == DataType.Coil)
            {
                var coils = await master.ReadCoilsAsync(slaveId, addr, 1);
                return coils.Length > 0 && coils[0] ? 1.0 : 0.0;
            }
            if (type == DataType.DiscreteInput)
            {
                var inputs = await master.ReadInputsAsync(slaveId, addr, 1);
                return inputs.Length > 0 && inputs[0] ? 1.0 : 0.0;
            }

            // 2. 寄存器操作 (字操作)
            ushort points = type.GetRegisterCount();

            // 判断是读 Holding 还是 Input Register
            ushort[] rawRegisters;
            if (type == DataType.InputRegister)
                rawRegisters = await master.ReadInputRegistersAsync(slaveId, addr, points);
            else
                rawRegisters = await master.ReadHoldingRegistersAsync(slaveId, addr, points);

            if (rawRegisters.Length < points)
                throw new Exception("读取寄存器长度不足");

            // 3. 使用 ModbusUtility 进行转换 (处理字节序)
            return type switch
            {
                DataType.Int16 => (short)rawRegisters[0],
                DataType.UInt16 => rawRegisters[0],
                DataType.InputRegister => rawRegisters[0],

                // 32位处理
                DataType.UInt32 => ModbusUtility.GetUInt32(rawRegisters[0], rawRegisters[1]),
                DataType.Float => ModbusUtility.GetSingle(rawRegisters[0], rawRegisters[1]),

                // 64位处理 (假设自定义转换)
                DataType.Double => ParseDouble(rawRegisters),

                // 默认
                _ => rawRegisters[0],
            };
        }

        private double ParseDouble(ushort[] registers)
        {
            // 将 4 个 ushort 转为 byte[]，再转 double
            byte[] bytes = new byte[8];
            // 注意：这里假设了字节序，实际情况可能需要调整
            Buffer.BlockCopy(registers, 0, bytes, 0, 8);
            // 如果设备是 BigEndian (Modbus标准)，C#是 LittleEndian，可能需要反转
            // 这里仅仅是一个示例，实际开发中建议写一个通用的 ByteSwap 工具方法
            return BitConverter.ToDouble(bytes, 0);
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _cts?.Dispose();
        }
    }
}
