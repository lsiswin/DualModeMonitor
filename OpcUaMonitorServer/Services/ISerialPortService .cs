using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// 串口服务接口，定义串口操作的基本功能
    /// </summary>
    public interface ISerialPortService : IDisposable
    {
        /// <summary>
        /// 数据接收事件流，当串口收到数据时触发
        /// </summary>
        IObservable<byte[]> DataReceived { get; }

        /// <summary>
        /// 状态变更事件流，当串口状态变化时触发（如打开、关闭、错误等）
        /// </summary>
        IObservable<bool> StatusChanged { get; }

        /// <summary>
        /// 串口是否处于打开状态
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 当前打开的串口名称（如"COM1"）
        /// </summary>
        string PortName { get; }

        /// <summary>
        /// 异步打开串口
        /// </summary>
        /// <param name="portName">串口名称（如"COM1"）</param>
        /// <param name="baudRate">波特率（默认9600）</param>
        /// <param name="parity">校验位（默认无校验）</param>
        /// <param name="dataBits">数据位（默认8位）</param>
        /// <param name="stopBits">停止位（默认1位）</param>
        /// <returns>打开成功返回true，否则返回false</returns>
        Task<bool> OpenAsync(
            string portName,
            int baudRate = 9600,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One
        );

        /// <summary>
        /// 异步关闭串口
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// 异步发送字节数组数据
        /// </summary>
        /// <param name="data">要发送的字节数组</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        Task<bool> WriteAsync(byte[] data);

        /// <summary>
        /// 异步发送字符串数据
        /// </summary>
        /// <param name="text">要发送的字符串</param>
        /// <param name="encoding">编码格式（默认UTF8）</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        Task<bool> WriteAsync(string text, Encoding encoding = null);
    }
}
