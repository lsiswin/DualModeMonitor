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
    /// 数据点采集记录
    /// </summary>
    public class DataPointRecord
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 外键：关联的数据点ID
        /// </summary>
        [ForeignKey("DataPoint")]
        public int DataPointId { get; set; }

        /// <summary>
        /// 采集到的数值
        /// </summary>
        [Required]
        public decimal Value { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        [Required]
        public DateTime CollectTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否为有效数据（未超出ValidMin/ValidMax）
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 是否触发告警（超出UpperLimit/LowerLimit）
        /// </summary>
        public bool IsAlarm { get; set; } = false;

        // 导航属性：反向关联数据点
        public DataPoint DataPoint { get; set; }

       
    }
}
