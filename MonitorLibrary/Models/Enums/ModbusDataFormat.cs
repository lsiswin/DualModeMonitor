using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    /// <summary>
    /// Modbus数据格式
    /// </summary>
    public enum ModbusDataFormat
    {
        [Description("16位整数")]
        Int16,      // 16位整数
        [Description("16位无符号整数")]
        UInt16,     // 16位无符号整数
        [Description("32位整数")]
        Int32,      // 32位整数
        [Description("32位无符号整数")]
        UInt32,     // 32位无符号整数
        [Description("32位浮点数")]
        Float32,    // 32位浮点数
        [Description("64位浮点数")]
        Float64     // 64位浮点数
    }
}
