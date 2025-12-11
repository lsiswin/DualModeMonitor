using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    public static class DataTypeHelper
    {
        /// <summary>
        /// 获取数据类型占用的寄存器数量 (Word Count)
        /// </summary>
        public static ushort GetRegisterCount(this DataType type)
        {
            return type switch
            {
                // 1 Bit (在ReadCoils中算1个点，但在计算寄存器长度时通常视为1个单位操作)
                DataType.Coil => 1,
                DataType.DiscreteInput => 1,

                // 16 Bit = 1 Register
                DataType.Int16 => 1,
                DataType.UInt16 => 1,
                DataType.InputRegister => 1,

                // 32 Bit = 2 Registers
                DataType.Int32 => 2,
                DataType.UInt32 => 2,
                DataType.Float => 2,

                // 64 Bit = 4 Registers
                DataType.Int64 => 4,
                DataType.UInt64 => 4,
                DataType.Double => 4,

                _ => 1, // 默认为1，防止崩溃
            };
        }

        /// <summary>
        /// 从字符串解析 DataType (不区分大小写，兼容旧别名)
        /// </summary>
        public static DataType Parse(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                return DataType.Unknown;

            return typeString.ToLower().Trim() switch
            {
                // Boolean / Bit
                "coil" => DataType.Coil,
                "bool" or "boolean" => DataType.Coil, // 默认 bool 视为 coil
                "input" or "discrete" or "discreteinput" => DataType.DiscreteInput,

                // 16-bit
                "int16" or "short" => DataType.Int16,
                "uint16" or "ushort" => DataType.UInt16,
                "inputregister" => DataType.InputRegister,

                // 32-bit
                "int32" or "int" or "integer" => DataType.Int32,
                "uint32" or "uint" => DataType.UInt32,
                "float" or "single" => DataType.Float,

                // 64-bit
                "int64" or "long" => DataType.Int64,
                "uint64" or "ulong" => DataType.UInt64,
                "double" => DataType.Double,

                _ => DataType.UInt16, // 默认值
            };
        }
    }
}
