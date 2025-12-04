using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorRabbitMQService.Configuration
{
    /// <summary>
    /// RabbitMQ连接配置
    /// </summary>
    public class RabbitMQConfiguration
    {
        /// <summary>
        /// 主机地址
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// 虚拟主机
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 连接超时(毫秒)
        /// </summary>
        public int RequestedConnectionTimeout { get; set; } = 30000;

        /// <summary>
        /// 心跳间隔(秒)
        /// </summary>
        public ushort RequestedHeartbeat { get; set; } = 60;

        /// <summary>
        /// 自动重连
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; } = true;

        /// <summary>
        /// 重连间隔(秒)
        /// </summary>
        public int NetworkRecoveryInterval { get; set; } = 10;
    }

    /// <summary>
    /// Exchange配置
    /// </summary>
    public class ExchangeConfiguration
    {
        /// <summary>
        /// 数据采集主题Exchange
        /// </summary>
        public string DataTopic { get; set; } = "monitor.data.topic";

        /// <summary>
        /// 命令控制Exchange
        /// </summary>
        public string CommandDirect { get; set; } = "monitor.command.direct";

        /// <summary>
        /// 事件广播Exchange
        /// </summary>
        public string EventsFanout { get; set; } = "monitor.events.fanout";

        /// <summary>
        /// 告警Exchange
        /// </summary>
        public string AlarmTopic { get; set; } = "monitor.alarm.topic";

        /// <summary>
        /// OPC数据Exchange
        /// </summary>
        public string OpcTopic { get; set; } = "monitor.opc.topic";
    }

    /// <summary>
    /// Queue配置
    /// </summary>
    public class QueueConfiguration
    {
        /// <summary>
        /// 实时数据队列(WPF订阅)
        /// </summary>
        public string RealtimeData { get; set; } = "realtime.data.queue";

        /// <summary>
        /// 持久化数据队列(DB服务消费)
        /// </summary>
        public string PersistData { get; set; } = "persist.data.queue";

        /// <summary>
        /// OPC数据队列
        /// </summary>
        public string OpcData { get; set; } = "opc.data.queue";

        /// <summary>
        /// 告警队列
        /// </summary>
        public string Alarm { get; set; } = "alarm.queue";

        /// <summary>
        /// 设备状态队列
        /// </summary>
        public string DeviceStatus { get; set; } = "device.status.queue";
    }

    /// <summary>
    /// 路由键配置
    /// </summary>
    public class RoutingKeyConfiguration
    {
        /// <summary>
        /// 实时数据路由键
        /// </summary>
        public string RealtimeData { get; set; } = "data.realtime";

        /// <summary>
        /// 历史数据路由键
        /// </summary>
        public string HistoricalData { get; set; } = "data.historical";

        /// <summary>
        /// 告警路由键
        /// </summary>
        public string Alarm { get; set; } = "alarm.#";

        /// <summary>
        /// 设备状态路由键
        /// </summary>
        public string DeviceStatus { get; set; } = "device.status";

        /// <summary>
        /// OPC数据路由键
        /// </summary>
        public string OpcData { get; set; } = "opc.data";
    }
}
