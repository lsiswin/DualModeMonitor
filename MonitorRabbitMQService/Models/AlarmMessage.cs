using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorRabbitMQService.Models
{
    /// <summary>
    /// 告警消息
    /// </summary>
    public class AlarmMessage
    {
        /// <summary>
        /// 消息唯一标识
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 告警ID
        /// </summary>
        public string AlarmId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 数据点ID
        /// </summary>
        public int DataPointId { get; set; }

        /// <summary>
        /// 数据点名称
        /// </summary>
        public string DataPointName { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 告警级别 (0=正常, 1=警告, 2=严重)
        /// </summary>
        public int AlarmLevel { get; set; }

        /// <summary>
        /// 告警类型 (UpperLimit=上限告警, LowerLimit=下限告警)
        /// </summary>
        public string AlarmType { get; set; }

        /// <summary>
        /// 当前值
        /// </summary>
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        public decimal ThresholdValue { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 告警时间
        /// </summary>
        public DateTime AlarmTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 告警消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否已确认
        /// </summary>
        public bool IsAcknowledged { get; set; } = false;

        /// <summary>
        /// 消息发送时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
