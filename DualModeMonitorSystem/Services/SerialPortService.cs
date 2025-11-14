using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Services
{
    /// <summary>
    /// 串口服务实现类，基于System.IO.Ports.SerialPort封装，提供响应式接口
    /// </summary>
    public class SerialPortService : ISerialPortService
    {
        private SerialPort _serialPort; // 底层串口对象
        private readonly Subject<byte[]> _dataReceivedSubject; // 数据接收事件主题
        private readonly Subject<string> _statusChangedSubject; // 状态变更事件主题
        private readonly CancellationTokenSource _cancellationTokenSource; // 用于取消异步任务的令牌源
        private bool _disposed = false; // 释放标志，防止重复释放

        /// <summary>
        /// 数据接收事件流（对外暴露为只读可观察对象）
        /// </summary>
        public IObservable<byte[]> DataReceived => _dataReceivedSubject.AsObservable();

        /// <summary>
        /// 状态变更事件流（对外暴露为只读可观察对象）
        /// </summary>
        public IObservable<string> StatusChanged => _statusChangedSubject.AsObservable();

        /// <summary>
        /// 串口是否打开
        /// </summary>
        public bool IsOpen => _serialPort?.IsOpen == true;

        /// <summary>
        /// 当前串口名称（如未打开则为null）
        /// </summary>
        public string PortName => _serialPort?.PortName;

        /// <summary>
        /// 构造函数，初始化事件主题和取消令牌
        /// </summary>
        public SerialPortService()
        {
            _dataReceivedSubject = new Subject<byte[]>();
            _statusChangedSubject = new Subject<string>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 异步打开串口
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>打开成功返回true</returns>
        public async Task<bool> OpenAsync(string portName, int baudRate = 9600, Parity parity = Parity.None,
                                        int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                // 如果已打开则先关闭
                if (IsOpen)
                    await CloseAsync();

                // 初始化串口参数
                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    ReadTimeout = 1000, // 读取超时时间（毫秒）
                    WriteTimeout = 1000, // 写入超时时间（毫秒）
                    Encoding = Encoding.UTF8 // 默认编码
                };

                // 订阅串口自带的数据接收和错误事件
                _serialPort.DataReceived += OnDataReceived;
                _serialPort.ErrorReceived += OnErrorReceived;

                // 打开串口
                _serialPort.Open();
                _statusChangedSubject.OnNext($"串口 {portName} 打开成功");

                // 启动后台数据读取任务（使用独立线程避免阻塞）
                _ = Task.Run(async () => await ReadDataAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                _statusChangedSubject.OnNext($"打开串口失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 异步关闭串口
        /// </summary>
        public async Task CloseAsync()
        {
            try
            {
                // 如果串口已打开，则关闭
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    _statusChangedSubject.OnNext($"串口 {_serialPort.PortName} 已关闭");
                }
            }
            catch (Exception ex)
            {
                _statusChangedSubject.OnNext($"关闭串口失败: {ex.Message}");
            }
            finally
            {
                // 清理资源：取消事件订阅、释放串口对象
                if (_serialPort != null)
                {
                    _serialPort.DataReceived -= OnDataReceived;
                    _serialPort.ErrorReceived -= OnErrorReceived;
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }

            // 异步方法必须返回Task，此处返回已完成的任务
            await Task.CompletedTask;
        }

        /// <summary>
        /// 异步发送字节数组
        /// </summary>
        /// <param name="data">要发送的字节数组</param>
        /// <returns>发送成功返回true</returns>
        public async Task<bool> WriteAsync(byte[] data)
        {
            // 校验参数：串口未打开、数据为空或长度为0则直接返回失败
            if (!IsOpen || data == null || data.Length == 0)
                return false;

            try
            {
                // 在后台线程执行写入操作（避免UI线程阻塞）
                await Task.Run(() => _serialPort.Write(data, 0, data.Length));
                return true;
            }
            catch (Exception ex)
            {
                _statusChangedSubject.OnNext($"发送数据失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 异步发送字符串
        /// </summary>
        /// <param name="text">要发送的字符串</param>
        /// <param name="encoding">编码格式（默认UTF8）</param>
        /// <returns>发送成功返回true</returns>
        public async Task<bool> WriteAsync(string text, Encoding encoding = null)
        {
            // 转换字符串为字节数组（默认使用UTF8编码）
            var enc = encoding ?? Encoding.UTF8;
            var data = enc.GetBytes(text);
            return await WriteAsync(data); // 调用字节数组发送方法
        }

        /// <summary>
        /// 串口数据接收事件处理函数
        /// </summary>
        /// <param name="sender">事件发送者（SerialPort对象）</param>
        /// <param name="e">事件参数（包含接收数据类型）</param>
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // 校验串口状态
                if (_serialPort?.IsOpen != true) return;

                // 读取缓冲区中的所有字节
                var bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    var buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);
                    // 通过主题推送接收到的数据
                    _dataReceivedSubject.OnNext(buffer);
                }
            }
            catch (Exception ex)
            {
                _statusChangedSubject.OnNext($"接收数据错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 串口错误事件处理函数
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数（包含错误类型）</param>
        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _statusChangedSubject.OnNext($"串口错误: {e.EventType}");
        }

        /// <summary>
        /// 后台数据读取任务（辅助监控数据接收）
        /// </summary>
        /// <param name="cancellationToken">取消令牌（用于停止任务）</param>
        private async Task ReadDataAsync(CancellationToken cancellationToken)
        {
            // 循环监控，直到任务被取消或串口关闭
            while (!cancellationToken.IsCancellationRequested && IsOpen)
            {
                try
                {
                    // 延迟100ms，降低CPU占用（实际数据接收依赖DataReceived事件）
                    await Task.Delay(100, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // 任务被取消时退出循环
                    break;
                }
                catch (Exception ex)
                {
                    _statusChangedSubject.OnNext($"数据读取任务错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // 取消所有异步任务
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                // 关闭串口（最多等待1秒）
                CloseAsync().Wait(1000);

                // 完成并释放事件主题
                _dataReceivedSubject?.OnCompleted();
                _dataReceivedSubject?.Dispose();

                _statusChangedSubject?.OnCompleted();
                _statusChangedSubject?.Dispose();

                _disposed = true;
            }
        }
    }
}
