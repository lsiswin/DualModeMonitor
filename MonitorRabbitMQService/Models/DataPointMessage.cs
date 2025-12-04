using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorRabbitMQService.Models
{
    /// <summary>
    /// 数据点实时数据消息
    /// </summary>
    public class DataPointMessage
    {
        /// <summary>
        /// 消息唯一标识
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 数据点ID
        /// </summary>
        public int DataPointId { get; set; }

        /// <summary>
        /// 数据点名称
        /// </summary>
        public string DataPointName { get; set; }

        /// <summary>
        /// 数据点编码
        /// </summary>
        public string DataPointCode { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// 采集值
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否为有效数据
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 是否触发告警
        /// </summary>
        public bool IsAlarm { get; set; } = false;

        /// <summary>
        /// 上限阈值
        /// </summary>
        public decimal UpperLimit { get; set; }

        /// <summary>
        /// 下限阈值
        /// </summary>
        public decimal LowerLimit { get; set; }

        /// <summary>
        /// 消息发送时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
