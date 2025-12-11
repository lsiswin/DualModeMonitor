using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    /// <summary>
    /// Modbus 数据类型枚举
    /// </summary>
    public enum DataType
    {
        [Description("Unknown")]
        Unknown = 0,

        // ==============================
        // 1. 布尔/位操作 (1 Bit)
        // ==============================

        /// <summary>
        /// 线圈 (读写: FC01/FC05)
        /// 对应: coil, bool (output)
        /// </summary>
        Coil,

        /// <summary>
        /// 离散输入 (只读: FC02)
        /// 对应: input, discrete
        /// </summary>
        DiscreteInput,

        // ==============================
        // 2. 16位整数 (1 Register)
        // ==============================

        /// <summary>
        /// 有符号16位整数 (-32768 ~ 32767)
        /// 对应: int16, short
        /// </summary>
        Int16,

        /// <summary>
        /// 无符号16位整数 (0 ~ 65535)
        /// 对应: uint16, ushort
        /// </summary>
        UInt16,

        /// <summary>
        /// 输入寄存器 (只读: FC04 - 通常为UInt16)
        /// 对应: inputregister
        /// </summary>
        InputRegister,

        // ==============================
        // 3. 32位整数/浮点 (2 Registers)
        // ==============================

        /// <summary>
        /// 有符号32位整数
        /// 对应: int32, int
        /// </summary>
        Int32,

        /// <summary>
        /// 无符号32位整数
        /// 对应: uint32, uint
        /// </summary>
        UInt32,

        /// <summary>
        /// 32位浮点数
        /// 对应: float, single
        /// </summary>
        Float,

        // ==============================
        // 4. 64位整数/浮点 (4 Registers)
        // ==============================

        /// <summary>
        /// 有符号64位整数
        /// 对应: int64, long
        /// </summary>
        Int64,

        /// <summary>
        /// 无符号64位整数
        /// 对应: uint64, ulong
        /// </summary>
        UInt64,

        /// <summary>
        /// 64位双精度浮点数
        /// 对应: double
        /// </summary>
        Double,
    }
}
