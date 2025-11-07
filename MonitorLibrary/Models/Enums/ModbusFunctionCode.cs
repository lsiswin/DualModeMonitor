using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    /// <summary>
    /// Modbus功能码
    /// </summary>
    public enum ModbusFunctionCode
    {
        ReadCoils = 0x01,                       // 01 - 读线圈
        ReadDiscreteInputs = 0x02,              // 02 - 读离散输入
        ReadHoldingRegisters = 0x03,            // 03 - 读保持寄存器
        ReadInputRegisters = 0x04,              // 04 - 读输入寄存器
        WriteSingleCoil = 0x05,                 // 05 - 写单个线圈
        WriteSingleRegister = 0x06,             // 06 - 写单个寄存器
        WriteMultipleCoils = 0x0F,              // 15 - 写多个线圈
        WriteMultipleRegisters = 0x10           // 16 - 写多个寄存器
    }
}
