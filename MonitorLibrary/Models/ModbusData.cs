using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// Modbus数据容器，用于存储批量读取的设备数据
    /// </summary>
    public class ModbusData
    {
        /// <summary>
        /// 保持寄存器数据
        /// </summary>
        public ushort[] HoldingRegisters { get; set; }

        /// <summary>
        /// 输入寄存器数据
        /// </summary>
        public ushort[] InputRegisters { get; set; }

        /// <summary>
        /// 线圈状态数据
        /// </summary>
        public bool[] Coils { get; set; }

        /// <summary>
        /// 输入状态数据
        /// </summary>
        public bool[] Inputs { get; set; }

        /// <summary>
        /// 数据读取时间
        /// </summary>
        public DateTime ReadTime { get; set; }
    }
}
