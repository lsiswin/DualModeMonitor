using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Description("01 - 读线圈")]
        ReadCoils = 01,                       
        [Description("02 - 读离散输入")]
        ReadDiscreteInputs = 02,              
        [Description("03 - 读保持寄存器")]
        ReadHoldingRegisters = 03,            
        [Description("04 - 读输入寄存器")]
        ReadInputRegisters = 04,              
        [Description("05 - 写单个线圈")]
        WriteSingleCoil = 05,                 
        [Description("06 - 写单个寄存器")]
        WriteSingleRegister = 06,             
        [Description("15 - 写多个线圈")]
        WriteMultipleCoils = 15,              
        [Description("16 - 写多个寄存器")]
        WriteMultipleRegisters = 16           
    }
}
