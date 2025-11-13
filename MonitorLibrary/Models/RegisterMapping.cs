using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models
{
    public class RegisterMapping : BindableBase
    {
        // ==================== 属性 ====================

        /// <summary>
        /// 数据点ID（唯一标识）
        /// </summary>
        public int DataPointId { get; set; }

        /// <summary>
        /// 数据类型名称（如“温度”、“湿度”）
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Modbus寄存器起始地址（如 40001 对应 0x0000）
        /// </summary>
        public ushort Address { get; set; }

        /// <summary>
        /// 数据格式（Float32, Int16等）
        /// </summary>
        public ModbusDataFormat Format { get; set; }

        /// <summary>
        /// 单位（如 ℃, %RH）
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 系数（即 DataMultiplier，用于缩放原始值）
        /// </summary>
        public decimal Factor { get; set; }

        /// <summary>
        /// 偏移量（可选校准参数，如 +2.5°C 补偿）
        /// </summary>
        public decimal Offset { get; set; } = 0m;

        /// <summary>
        /// 是否启用该数据点采集
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        // ==================== 命令 ====================

        /// <summary>
        /// 编辑当前项的命令
        /// </summary>
        public DelegateCommand EditCommand { get; set; }

        /// <summary>
        /// 删除当前项的命令
        /// </summary>
        public DelegateCommand DeleteCommand { get; set; }
    }
}
