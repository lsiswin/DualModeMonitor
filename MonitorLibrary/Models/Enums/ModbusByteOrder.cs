using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    /// <summary>
    /// 字节序
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ModbusByteOrder
    {
        [Description("大端 (Big-Endian)")]
        BigEndian,

        [Description("小端 (Little-Endian)")]
        LittleEndian
    }
}
