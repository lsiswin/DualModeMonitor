using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorLibrary.Models;
using MonitorLibrary.Reactive;
using MonitorRabbitMQService.Models;
using Newtonsoft.Json;
using OpcUaMonitorServer.Configuration;
using OpcUaMonitorServer.Model;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// 数据采集服务 - 通过Modbus/SerialPort读取传感器数据
    /// </summary>
    public interface IDataCollectionService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
        OpcDataMessage? GetLatestData(int deviceId, int dataPointId);
    }

    public class DataCollectionService : IDataCollectionService
    {
        private readonly IDeviceManagementService _deviceManager;
        private readonly ISensorDataPublisher _dataPublisher;
        private readonly IOpcUaServerService? _opcServer;
        private readonly ReactiveLogger _logger;
        private readonly DataCollectionConfiguration _config;
        private readonly IModbusServiceFactory _modbusFactory; // 新增工厂依赖
        private readonly ConcurrentDictionary<int, IModbusService> _modbusConnections = new();
        private readonly ConcurrentDictionary<string, OpcDataMessage> _latestData = new();
        private CancellationTokenSource? _cts;
        private Task? _collectionTask;

        public DataCollectionService(
            IDeviceManagementService deviceManager,
            ISensorDataPublisher dataPublisher,
            IOptions<DataCollectionConfiguration> config,
            ReactiveLogger logger,
            IModbusServiceFactory modbusFactory, // 注入工厂
            IOpcUaServerService? opcServer = null
        )
        {
            _deviceManager = deviceManager;
            _dataPublisher = dataPublisher;

            _opcServer = opcServer;
            _modbusFactory = modbusFactory;
            _config = config.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("启动数据采集服务...");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _collectionTask = Task.Run(() => CollectionLoopAsync(_cts.Token), _cts.Token);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 轮询采集所有设备数据
        /// </summary>
        /// <param name="token"></param>
        private async void CollectionLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await CollectAllDeviceDataAsync();
                    await Task.Delay(_config.ScanIntervalMs, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("数据采集循环出错", ex);
                    await Task.Delay(1000, token);
                }
            }
        }

        /// <summary>
        /// 采集设备数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task CollectAllDeviceDataAsync()
        {
            var devices = await _deviceManager.GetDevicesAsync();

            foreach (var device in devices.Where(d => d.IsEnabled))
            {
                try
                {
                    await CollectDeviceDataAsync(device);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"采集设备数据失败: {device.Name}", ex);
                }
            }
        }

        /// <summary>
        /// 采集单个设备数据
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task CollectDeviceDataAsync(DeviceInfo device)
        {
            // 获取或创建Modbus连接
            var modbusService = await GetOrCreateModbusConnectionAsync(device);
            if (modbusService == null || !modbusService.IsConnected)
            {
                _logger.LogWarning($"设备未连接: {device.Name}");
                return;
            }
            // 获取设备的所有数据点
            var dataPoints = await _deviceManager.GetDataPointsAsync(device.Id);

            foreach (var dataPoint in dataPoints.Where(dp => dp.IsEnable))
            {
                try
                {
                    var value = await ReadDataPointAsync(modbusService, dataPoint);

                    var sensorData = new OpcDataMessage
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Name = device.Name,
                        DataPointId = dataPoint.Id,
                        DataPointCode = dataPoint.Code,
                        DataType = dataPoint.DataType,
                        Value = value * dataPoint.Scale + dataPoint.Offset,
                        Timestamp = DateTime.Now,
                        Quality = "Good",
                        CommandType = OpcCommandType.Read,
                    };

                    // 保存最新数据
                    var key = $"{device.Id}_{dataPoint.Id}";
                    _latestData[key] = sensorData;

                    // 更新OPC UA节点值
                    _opcServer?.UpdateDataPointValue(
                        device.Id,
                        dataPoint.Id,
                        sensorData.Value,
                        sensorData.Timestamp
                    );

                    // 发布到RabbitMQ
                    await _dataPublisher.PublishAsync(sensorData);

                    _logger.LogDebug(
                        $"采集数据: {sensorData.Name} = {sensorData.Value} {sensorData.DataType}"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError($"读取数据点失败: {device.Name}.{dataPoint.Name}", ex);
                }
            }
        }

        /// <summary>
        /// 获取或创建Modbus连接
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<IModbusService?> GetOrCreateModbusConnectionAsync(DeviceInfo device)
        {
            if (_modbusConnections.TryGetValue(device.Id, out var existingConnection))
            {
                if (existingConnection.IsConnected)
                    return existingConnection;

                // 连接已断开，尝试重新连接
                await existingConnection.DisconnectAsync();
                existingConnection.Dispose();
                _modbusConnections.TryRemove(device.Id, out _);
            }
            var config = device.PortConfig;
            if (config == null)
            {
                _logger.LogError($"无法解析设备连接配置: {device.Name}");
                return null;
            }
            try
            {
                // 使用工厂创建并连接
                var (modbusService, isConnected) = await _modbusFactory.CreateAndConnectAsync(
                    config
                );

                if (isConnected && modbusService != null) // 检查 null 是好习惯
                {
                    _modbusConnections[device.Id] = modbusService; // 存储新创建的服务实例
                    _logger.LogInformation($"成功连接到设备: {device.Name}");
                    return modbusService;
                }
                else
                {
                    _logger.LogWarning($"连接设备失败: {device.Name}");
                    modbusService.Dispose();
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"创建Modbus连接失败: {device.Name}", ex);
                return null;
            }
        }

        /// <summary>
        /// 关闭数据采集服务
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            _logger.LogInformation("停止数据采集服务...");

            _cts?.Cancel();

            if (_collectionTask != null)
            {
                await _collectionTask;
            }

            // 关闭所有Modbus连接
            foreach (var connection in _modbusConnections.Values)
            {
                await connection.DisconnectAsync();
                connection.Dispose();
            }
            _modbusConnections.Clear();
        }

        /// <summary>
        /// 获取最新的数据点值
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="dataPointId"></param>
        /// <returns></returns>
        public OpcDataMessage? GetLatestData(int deviceId, int dataPointId)
        {
            var key = $"{deviceId}_{dataPointId}";
            _latestData.TryGetValue(key, out var data);
            return data;
        }

        /// <summary>
        /// 读取单个数据点的值
        /// </summary>
        /// <param name="modbusService"></param>
        /// <param name="dataPoint"></param>
        /// <returns></returns>
        private async Task<double> ReadDataPointAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            try
            {
                // 根据数据类型选择合适的读取方式
                return dataPoint.DataType.ToLower() switch
                {
                    "int16" or "short" => await ReadInt16Async(modbusService, dataPoint),
                    "uint16" or "ushort" => await ReadUInt16Async(modbusService, dataPoint),
                    "int32" or "int" => await ReadInt32Async(modbusService, dataPoint),
                    "uint32" or "uint" => await ReadUInt32Async(modbusService, dataPoint),
                    "float" or "single" => await ReadFloatAsync(modbusService, dataPoint),
                    "double" => await ReadDoubleAsync(modbusService, dataPoint),
                    "bool" or "boolean" => await ReadBoolAsync(modbusService, dataPoint),
                    "coil" => await ReadCoilAsync(modbusService, dataPoint),
                    "input" => await ReadInputAsync(modbusService, dataPoint),
                    "inputregister" => await ReadInputRegisterAsync(modbusService, dataPoint),
                    _ => await ReadUInt16Async(modbusService, dataPoint), // 默认读取UInt16
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"读取数据点出错: Address={dataPoint.Address}, DataType={dataPoint.DataType}",
                    ex
                );
                throw;
            }
        }

        /// <summary>
        /// 读取Int16（有符号16位整数）
        /// </summary>
        private async Task<double> ReadInt16Async(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                1
            );
            if (registers == null || registers.Length == 0)
                throw new InvalidOperationException("未能读取到寄存器数据");

            // 将ushort转换为short（有符号）
            return (short)registers[0];
        }

        /// <summary>
        /// 读取UInt16（无符号16位整数）
        /// </summary>
        private async Task<double> ReadUInt16Async(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                1
            );
            if (registers == null || registers.Length == 0)
                throw new InvalidOperationException("未能读取到寄存器数据");

            return registers[0];
        }

        /// <summary>
        /// 读取Int32（有符号32位整数，占用2个寄存器）
        /// </summary>
        private async Task<double> ReadInt32Async(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                2
            );
            if (registers == null || registers.Length < 2)
                throw new InvalidOperationException("未能读取到足够的寄存器数据");

            // 高位在前，低位在后（大端字节序）
            int value = (registers[0] << 16) | registers[1];
            return value;
        }

        /// <summary>
        /// 读取UInt32（无符号32位整数，占用2个寄存器）
        /// </summary>
        private async Task<double> ReadUInt32Async(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                2
            );
            if (registers == null || registers.Length < 2)
                throw new InvalidOperationException("未能读取到足够的寄存器数据");

            // 高位在前，低位在后（大端字节序）
            uint value = ((uint)registers[0] << 16) | registers[1];
            return value;
        }

        /// <summary>
        /// 读取Float（32位浮点数，占用2个寄存器）
        /// </summary>
        private async Task<double> ReadFloatAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                2
            );
            if (registers == null || registers.Length < 2)
                throw new InvalidOperationException("未能读取到足够的寄存器数据");

            // 将2个寄存器转换为float
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(registers[1] & 0xFF);
            bytes[1] = (byte)((registers[1] >> 8) & 0xFF);
            bytes[2] = (byte)(registers[0] & 0xFF);
            bytes[3] = (byte)((registers[0] >> 8) & 0xFF);

            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// 读取Double（64位浮点数，占用4个寄存器）
        /// </summary>
        private async Task<double> ReadDoubleAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                4
            );
            if (registers == null || registers.Length < 4)
                throw new InvalidOperationException("未能读取到足够的寄存器数据");

            // 将4个寄存器转换为double
            byte[] bytes = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                bytes[i * 2] = (byte)(registers[3 - i] & 0xFF);
                bytes[i * 2 + 1] = (byte)((registers[3 - i] >> 8) & 0xFF);
            }

            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// 读取Bool（从保持寄存器读取，0=false, 非0=true）
        /// </summary>
        private async Task<double> ReadBoolAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadHoldingRegistersAsync(
                (ushort)dataPoint.Address,
                1
            );
            if (registers == null || registers.Length == 0)
                throw new InvalidOperationException("未能读取到寄存器数据");

            return registers[0] != 0 ? 1.0 : 0.0;
        }

        /// <summary>
        /// 读取Coil（线圈状态）
        /// </summary>
        private async Task<double> ReadCoilAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var coils = await modbusService.ReadCoilsAsync((ushort)dataPoint.Address, 1);
            if (coils == null || coils.Length == 0)
                throw new InvalidOperationException("未能读取到线圈数据");

            return coils[0] ? 1.0 : 0.0;
        }

        /// <summary>
        /// 读取Input（离散输入状态）
        /// </summary>
        private async Task<double> ReadInputAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var inputs = await modbusService.ReadInputsAsync((ushort)dataPoint.Address, 1);
            if (inputs == null || inputs.Length == 0)
                throw new InvalidOperationException("未能读取到输入数据");

            return inputs[0] ? 1.0 : 0.0;
        }

        /// <summary>
        /// 读取InputRegister（输入寄存器）
        /// </summary>
        private async Task<double> ReadInputRegisterAsync(
            IModbusService modbusService,
            DataPointInfo dataPoint
        )
        {
            var registers = await modbusService.ReadInputRegistersAsync(
                (ushort)dataPoint.Address,
                1
            );
            if (registers == null || registers.Length == 0)
                throw new InvalidOperationException("未能读取到输入寄存器数据");

            return registers[0];
        }
    }
}
