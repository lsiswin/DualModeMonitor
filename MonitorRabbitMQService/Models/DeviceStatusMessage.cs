using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorRabbitMQService.Models
{
    /// <summary>
    /// 设备状态变更消息
    /// </summary>
    public class DeviceStatusMessage
    {
        /// <summary>
        /// 消息唯一标识
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

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
        /// 设备状态 (0=离线, 1=在线, 2=故障)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 状态变更时间
        /// </summary>
        public DateTime StatusChangeTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 消息发送时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 备注信息(如错误原因)
        /// </summary>
        public string Remark { get; set; }
    }
}
