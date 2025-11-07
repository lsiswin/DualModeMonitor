using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// 数据点（传感器的单个参数，如温度、湿度）
    /// </summary>
    public class DataPoint :BindableBase
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 外键：关联的传感器ID
        /// </summary>
        [ForeignKey("HumitureDevices")]
        public int DeviceId { get; set; }

        /// <summary>
        /// 数据点名称（如“温度”“湿度”）
        /// </summary>
        [Required, MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 数据点编码（唯一标识，如“Temp”“Hum”）
        /// </summary>
        [Required, MaxLength(20)]
        public string Code { get; set; }

        /// <summary>
        /// 单位（如“℃”“%RH”）
        /// </summary>
        [Required, MaxLength(10)]
        public string Unit { get; set; }

        /// <summary>
        /// 采集间隔（秒）
        /// </summary>
        [Required, Range(1, 3600)]
        public int CollectInterval { get; set; } = 10;

        /// <summary>
        /// 上限阈值（超此值告警）
        /// </summary>
        [Required]
        public decimal UpperLimit { get; set; }

        /// <summary>
        /// 下限阈值（低此值告警）
        /// </summary>
        [Required]
        public decimal LowerLimit { get; set; }

        /// <summary>
        /// 有效范围最小值（过滤异常值）
        /// </summary>
        [Required]
        public decimal ValidMin { get; set; }

        /// <summary>
        /// 有效范围最大值（过滤异常值）
        /// </summary>
        [Required]
        public decimal ValidMax { get; set; }

        /// <summary>
        /// 数据保留天数
        /// </summary>
        [Required, Range(1, 3650)]
        public int DataRetentionDays { get; set; } = 30;

        /// <summary>
        /// 是否启用告警
        /// </summary>
        public bool EnableAlarm { get; set; } = true;

        /// <summary>
        /// 告警延迟（秒，避免瞬时波动）
        /// </summary>
        public int AlarmDelay { get; set; } = 30;

        // 导航属性
        /// <summary>
        /// 关联的传感器
        /// </summary>
        public HumitureDevices HumitureDevices { get; set; }

        /// <summary>
        /// 一对一：该数据点的Modbus配置
        /// </summary>
        public ModbusConfig ModbusConfig { get; set; }

        /// <summary>
        /// 一对多：该数据点的历史记录
        /// </summary>
        public ICollection<DataPointRecord> Records { get; set; } = new List<DataPointRecord>();

      
    }
}
