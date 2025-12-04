using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// Modbus服务接口，定义Modbus RTU协议的基本操作
    /// </summary>
    public interface IModbusService : IDisposable
    {
        /// <summary>
        /// 连接状态变更事件流（连接/断开时触发）
        /// </summary>
        IObservable<bool> ConnectionStatusChanged { get; }

        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 异步连接到Modbus设备
        /// </summary>
        /// <param name="portName">串口名称（如"COM1"）</param>
        /// <param name="baudRate">波特率（默认9600）</param>
        /// <param name="slaveId">从站地址（默认1）</param>
        /// <returns>连接成功返回true</returns>
        Task<bool> ConnectAsync(SerialPortConfig config);

        /// <summary>
        /// 异步断开与Modbus设备的连接
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 读取线圈状态（功能码0x01）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>线圈状态数组（true=导通，false=断开）</returns>
        Task<bool[]> ReadCoilsAsync(ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 读取输入状态（功能码0x02）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>输入状态数组</returns>
        Task<bool[]> ReadInputsAsync(ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 读取保持寄存器（功能码0x03）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>寄存器值数组</returns>
        Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 读取输入寄存器（功能码0x04）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns>寄存器值数组</returns>
        Task<ushort[]> ReadInputRegistersAsync(ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 写入单个线圈（功能码0x05）
        /// </summary>
        /// <param name="address">线圈地址</param>
        /// <param name="value">线圈状态（true=导通，false=断开）</param>
        /// <returns>写入成功返回true</returns>
        Task<bool> WriteSingleCoilAsync(ushort address, bool value);

        /// <summary>
        /// 写入单个保持寄存器（功能码0x06）
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入成功返回true</returns>
        Task<bool> WriteSingleRegisterAsync(ushort address, ushort value);

        /// <summary>
        /// 写入多个保持寄存器（功能码0x10）
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="data">要写入的数据数组</param>
        /// <returns>写入成功返回true</returns>
        Task<bool> WriteMultipleRegistersAsync(ushort startAddress, ushort[] data);
    }
}
