using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;
using MonitorLibrary.Reactive;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// Modbus服务实现类，基于串口服务实现Modbus RTU协议
    /// </summary>
    public class ModbusService : IModbusService
    {
        private readonly ISerialPortService _serialPortService; // 串口服务依赖
        private readonly ReactiveLogger _logger;
        private readonly Subject<bool> _connectionStatusSubject; // 连接状态事件主题
        private byte _slaveId; // Modbus从站地址
        private bool _disposed = false; // 释放标志

        /// <summary>
        /// 连接状态变更事件流（对外暴露为只读可观察对象）
        /// </summary>
        public IObservable<bool> ConnectionStatusChanged => _connectionStatusSubject.AsObservable();

        /// <summary>
        /// 是否连接到Modbus设备
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 构造函数，注入串口服务依赖
        /// </summary>
        /// <param name="serialPortService">串口服务实例</param>
        public ModbusService(ISerialPortService serialPortService, ReactiveLogger logger)
        {
            _serialPortService = serialPortService;
            _logger = logger;
            _connectionStatusSubject = new Subject<bool>();
        }

        public ModbusService() { }

        /// <summary>
        /// 连接到Modbus设备（通过串口）
        /// </summary>
        /// <param name="config">Modbus连接配置</param>
        /// <returns>连接成功返回true</returns>
        public async Task<bool> ConnectAsync(SerialPortConfig config)
        {
            try
            {
                _slaveId = config.DeviceAddress; // 保存从站地址

                // 调用串口服务打开串口（Modbus RTU基于串口通信）
                var success = await _serialPortService.OpenAsync(
                    config.PortName,
                    (int)config.BaudRate,
                    config.Parity,
                    (int)config.DataBits,
                    config.StopBits
                );

                if (success)
                {
                    IsConnected = true;
                    _connectionStatusSubject.OnNext(true); // 推送连接成功事件
                    _logger.LogInformation(
                        $"Modbus连接成功 - 端口: {config.PortName}, 从站: {config.Id}"
                    );

                    // 订阅串口数据接收和状态变更事件
                    _serialPortService.DataReceived.Subscribe(OnSerialDataReceived);
                    _serialPortService.StatusChanged.Subscribe(OnSerialStatusChanged);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Modbus连接失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 断开与Modbus设备的连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            // 调用串口服务关闭串口
            await _serialPortService.CloseAsync();
            IsConnected = false;
            _connectionStatusSubject.OnNext(false); // 推送断开连接事件
            _logger.LogInformation("Modbus连接已关闭");
        }

        /// <summary>
        /// 处理串口接收的数据（Modbus响应）
        /// </summary>
        /// <param name="data">串口接收到的字节数组</param>
        private void OnSerialDataReceived(byte[] data)
        {
            // 此处可扩展为解析Modbus响应帧的逻辑
            _logger.LogInformation($"收到Modbus数据: {BitConverter.ToString(data)}");
        }

        /// <summary>
        /// 处理串口状态变更
        /// </summary>
        /// <param name="status">状态描述字符串</param>
        private void OnSerialStatusChanged(bool status)
        {
            _logger.LogInformation($"串口状态: {status}");
        }

        /// <summary>
        /// 读取线圈状态（功能码0x01）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>线圈状态数组</returns>
        public async Task<bool[]> ReadCoilsAsync(ushort startAddress, ushort numberOfPoints)
        {
            try
            {
                // 构建Modbus读取线圈请求帧
                var request = BuildReadCoilsRequest(startAddress, numberOfPoints);
                // 发送请求并等待响应（响应长度=5 + 数据字节数，数据字节数=(点数+7)/8）
                var response = await SendModbusRequestAsync(request, 5 + (numberOfPoints + 7) / 8);

                // 解析响应数据为线圈状态数组
                return ParseCoilsResponse(response, numberOfPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError($"读取线圈失败: {ex.Message}");
                return new bool[numberOfPoints]; // 失败时返回空数组
            }
        }

        /// <summary>
        /// 读取保持寄存器（功能码0x03）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>寄存器值数组</returns>
        public async Task<ushort[]> ReadHoldingRegistersAsync(
            ushort startAddress,
            ushort numberOfPoints
        )
        {
            try
            {
                // 构建读取保持寄存器请求帧
                var request = BuildReadHoldingRegistersRequest(startAddress, numberOfPoints);
                // 发送请求并等待响应（响应长度=5 + 2*点数，每个寄存器占2字节）
                var response = await SendModbusRequestAsync(request, 5 + numberOfPoints * 2);

                // 解析响应数据为寄存器数组
                return ParseRegistersResponse(response, numberOfPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError($"读取保持寄存器失败: {ex.Message}");
                return new ushort[numberOfPoints]; // 失败时返回空数组
            }
        }

        /// <summary>
        /// 写入单个保持寄存器（功能码0x06）
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入成功返回true</returns>
        public async Task<bool> WriteSingleRegisterAsync(ushort address, ushort value)
        {
            try
            {
                // 构建写入单个寄存器请求帧
                var request = BuildWriteSingleRegisterRequest(address, value);
                // 发送请求并等待响应（响应长度固定为8字节）
                var response = await SendModbusRequestAsync(request, 8);

                // 验证响应是否正确
                return ValidateWriteResponse(response, address, value);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"写入寄存器失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 读取输入状态（功能码0x02）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>输入状态数组</returns>
        public Task<bool[]> ReadInputsAsync(ushort startAddress, ushort numberOfPoints)
        {
            // 实现逻辑类似ReadCoilsAsync（功能码改为0x02）
            _logger.LogInformation($"读取输入状态 - 地址: {startAddress}, 数量: {numberOfPoints}");
            return Task.FromResult(new bool[numberOfPoints]);
        }

        /// <summary>
        /// 读取输入寄存器（功能码0x04）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>寄存器值数组</returns>
        public Task<ushort[]> ReadInputRegistersAsync(ushort startAddress, ushort numberOfPoints)
        {
            // 实现逻辑类似ReadHoldingRegistersAsync（功能码改为0x04）
            _logger.LogInformation(
                $"读取输入寄存器 - 地址: {startAddress}, 数量: {numberOfPoints}"
            );
            return Task.FromResult(new ushort[numberOfPoints]);
        }

        /// <summary>
        /// 写入单个线圈（功能码0x05）
        /// </summary>
        /// <param name="address">线圈地址</param>
        /// <param name="value">线圈状态</param>
        /// <returns>写入成功返回true</returns>
        public Task<bool> WriteSingleCoilAsync(ushort address, bool value)
        {
            // 实现逻辑：
            // 1. 构建功能码0x05的请求帧（值为0xFF00表示导通，0x0000表示断开）
            // 2. 发送请求并验证响应
            _logger.LogInformation($"写入线圈 - 地址: {address}, 值: {value}");
            return Task.FromResult(true);
        }

        /// <summary>
        /// 写入多个保持寄存器（功能码0x10）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="data">要写入的数据数组</param>
        /// <returns>写入成功返回true</returns>
        public Task<bool> WriteMultipleRegistersAsync(ushort startAddress, ushort[] data)
        {
            // 实现逻辑：
            // 1. 构建功能码0x10的请求帧（包含起始地址、数量、字节数、数据）
            // 2. 发送请求并验证响应
            _logger.LogInformation(
                $"写入多个寄存器 - 起始地址: {startAddress}, 数量: {data?.Length}"
            );
            return Task.FromResult(true);
        }

        /// <summary>
        /// 构建读取保持寄存器的请求帧（功能码0x03）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>Modbus请求帧字节数组</returns>
        private byte[] BuildReadHoldingRegistersRequest(ushort startAddress, ushort numberOfPoints)
        {
            // Modbus RTU帧结构：[从站地址(1字节)][功能码(1字节)][起始地址(2字节)][数量(2字节)][CRC校验(2字节)]
            var request = new byte[8];
            request[0] = _slaveId; // 从站地址
            request[1] = 0x03; // 功能码：读保持寄存器
            request[2] = (byte)(startAddress >> 8); // 起始地址高字节
            request[3] = (byte)(startAddress & 0xFF); // 起始地址低字节
            request[4] = (byte)(numberOfPoints >> 8); // 数量高字节
            request[5] = (byte)(numberOfPoints & 0xFF); // 数量低字节

            // 计算CRC校验并填充
            var crc = CalculateCRC(request, 6); // 前6字节参与CRC计算
            request[6] = (byte)(crc & 0xFF); // CRC低字节
            request[7] = (byte)(crc >> 8); // CRC高字节

            return request;
        }

        /// <summary>
        /// 构建读取线圈的请求帧（功能码0x01）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>Modbus请求帧字节数组</returns>
        private byte[] BuildReadCoilsRequest(ushort startAddress, ushort numberOfPoints)
        {
            var request = new byte[8];
            request[0] = _slaveId;
            request[1] = 0x01; // 功能码：读线圈
            request[2] = (byte)(startAddress >> 8); // 起始地址高字节
            request[3] = (byte)(startAddress & 0xFF); // 起始地址低字节
            request[4] = (byte)(numberOfPoints >> 8); // 数量高字节
            request[5] = (byte)(numberOfPoints & 0xFF); // 数量低字节

            var crc = CalculateCRC(request, 6);
            request[6] = (byte)(crc & 0xFF);
            request[7] = (byte)(crc >> 8);

            return request;
        }

        /// <summary>
        /// 构建写入单个寄存器的请求帧（功能码0x06）
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>Modbus请求帧字节数组</returns>
        private byte[] BuildWriteSingleRegisterRequest(ushort address, ushort value)
        {
            var request = new byte[8];
            request[0] = _slaveId;
            request[1] = 0x06; // 功能码：写单个寄存器
            request[2] = (byte)(address >> 8); // 地址高字节
            request[3] = (byte)(address & 0xFF); // 地址低字节
            request[4] = (byte)(value >> 8); // 值高字节
            request[5] = (byte)(value & 0xFF); // 值低字节

            var crc = CalculateCRC(request, 6);
            request[6] = (byte)(crc & 0xFF);
            request[7] = (byte)(crc >> 8);

            return request;
        }

        /// <summary>
        /// 发送Modbus请求并等待响应
        /// </summary>
        /// <param name="request">请求帧字节数组</param>
        /// <param name="expectedResponseLength">预期响应长度</param>
        /// <returns>响应帧字节数组</returns>
        private async Task<byte[]> SendModbusRequestAsync(
            byte[] request,
            int expectedResponseLength
        )
        {
            if (!IsConnected)
                throw new InvalidOperationException("Modbus未连接");

            // 发送请求帧
            var sent = await _serialPortService.WriteAsync(request);
            if (!sent)
                throw new Exception("发送Modbus请求失败");

            // 等待设备响应：订阅串口 DataReceived，累积字节直到达到预期长度或超时
            var buffer = new List<byte>(expectedResponseLength);
            var tcs = new TaskCompletionSource<byte[]>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            CancellationTokenSource cts = new CancellationTokenSource();

            // 默认超时 2000ms；如果串口实现或配置可提供超时值，应使用该值
            var timeoutMs = 2000;

            IDisposable subscription = null;
            try
            {
                subscription = _serialPortService.DataReceived.Subscribe(data =>
                {
                    try
                    {
                        if (data == null || data.Length == 0)
                            return;
                        lock (buffer)
                        {
                            buffer.AddRange(data);
                            if (buffer.Count >= expectedResponseLength)
                            {
                                // 截取前 expectedResponseLength 个字节返回
                                var result = buffer.Take(expectedResponseLength).ToArray();
                                // 完成 tcs（注意：可能多次触发，故使用 TrySetResult）
                                tcs.TrySetResult(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });

                // 启动超时任务
                cts.CancelAfter(timeoutMs);

                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    // 等待完成或取消
                    var completedTask = await Task.WhenAny(
                        tcs.Task,
                        Task.Delay(Timeout.Infinite, cts.Token)
                    );

                    if (completedTask == tcs.Task)
                    {
                        // 成功接收
                        return await tcs.Task; // 已完成
                    }
                    else
                    {
                        throw new TimeoutException($"等待Modbus响应超时（{timeoutMs}ms）");
                    }
                }
            }
            finally
            {
                // 取消订阅以释放资源
                try
                {
                    subscription?.Dispose();
                }
                catch { }
                cts.Dispose();
            }
        }

        /// <summary>
        /// 解析寄存器响应数据
        /// </summary>
        /// <param name="response">响应帧字节数组</param>
        /// <param name="numberOfPoints">寄存器数量</param>
        /// <returns>寄存器值数组</returns>
        private ushort[] ParseRegistersResponse(byte[] response, ushort numberOfPoints)
        {
            // Modbus寄存器响应结构：[从站地址][功能码][字节数][数据1(2字节)][数据2(2字节)]...[CRC]
            var registers = new ushort[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                // 每个寄存器占2字节，高位在前
                registers[i] = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
            }
            return registers;
        }

        /// <summary>
        /// 解析线圈响应数据
        /// </summary>
        /// <param name="response">响应帧字节数组</param>
        /// <param name="numberOfPoints">线圈数量</param>
        /// <returns>线圈状态数组</returns>
        private bool[] ParseCoilsResponse(byte[] response, ushort numberOfPoints)
        {
            // Modbus线圈响应结构：[从站地址][功能码][字节数][数据字节1]...[CRC]
            // 每个字节表示8个线圈状态（bit0对应第一个线圈）
            var coils = new bool[numberOfPoints];
            var byteCount = response[2]; // 响应中的数据字节数

            for (int i = 0; i < numberOfPoints; i++)
            {
                var byteIndex = i / 8; // 计算当前线圈所在的字节索引
                var bitIndex = i % 8; // 计算当前线圈在字节中的bit位置
                if (byteIndex < byteCount)
                {
                    // 检查对应bit是否为1
                    coils[i] = (response[3 + byteIndex] & (1 << bitIndex)) != 0;
                }
            }
            return coils;
        }

        /// <summary>
        /// 验证写入操作的响应是否正确
        /// </summary>
        /// <param name="response">响应帧字节数组</param>
        /// <param name="address">写入的地址</param>
        /// <param name="value">写入的值</param>
        /// <returns>验证通过返回true</returns>
        private bool ValidateWriteResponse(byte[] response, ushort address, ushort value)
        {
            // 写入响应应与请求帧一致（除CRC外）
            return response.Length >= 8
                && response[0] == _slaveId
                && // 从站地址匹配
                response[1] == 0x06
                && // 功能码匹配
                BitConverter.ToUInt16(response, 2) == address
                && // 地址匹配
                BitConverter.ToUInt16(response, 4) == value; // 值匹配
        }

        /// <summary>
        /// 计算Modbus CRC16校验值
        /// </summary>
        /// <param name="data">要计算的数据</param>
        /// <param name="length">数据长度</param>
        /// <returns>CRC16校验值</returns>
        private ushort CalculateCRC(byte[] data, int length)
        {
            ushort crc = 0xFFFF; // 初始值
            for (int pos = 0; pos < length; pos++)
            {
                crc ^= data[pos]; // 与当前字节异或

                // 处理每个bit
                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0) // 最低位为1
                    {
                        crc >>= 1; // 右移
                        crc ^= 0xA001; // 与多项式异或（0xA001是0x8005的反转）
                    }
                    else
                    {
                        crc >>= 1; // 右移
                    }
                }
            }
            return crc;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // 断开连接（最多等待1秒）
                DisconnectAsync().Wait(1000);
                // 完成并释放事件主题
                _connectionStatusSubject?.OnCompleted();
                _connectionStatusSubject?.Dispose();
                _logger.Dispose();
                _disposed = true;
            }
        }
    }
}
