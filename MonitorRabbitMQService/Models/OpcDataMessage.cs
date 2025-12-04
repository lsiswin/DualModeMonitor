using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorRabbitMQService.Models
{
    /// <summary>
    /// OPC数据消息(用于与OPC Server交互)
    /// </summary>
    public class OpcDataMessage
    {
        /// <summary>
        /// 消息唯一标识
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// OPC节点ID
        /// </summary>
        public string? NodeId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据点ID (对应本地数据点)
        /// </summary>
        public int? DataPointId { get; set; }

        /// <summary>
        /// 数据点编码
        /// </summary>
        public string DataPointCode { get; set; }

        /// <summary>
        /// 数据值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 数据质量 (Good, Bad, Uncertain)
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 命令类型 (Read, Write, Subscribe)
        /// </summary>
        public OpcCommandType CommandType { get; set; }
    }

    public enum OpcCommandType
    {
        Read,
        Write,
        Subscribe,
    }
}
