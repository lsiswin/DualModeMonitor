using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorRabbitMQService.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MonitorRabbitMQService.Services
{
    /// <summary>
    /// 消息发布者接口
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// 发布消息到指定Exchange
        /// </summary>
        Task PublishAsync<T>(
            string exchange,
            T message,
            string routingKey = "",
            bool persistent = true
        );
        Task InitializeChannelAsync();

        /// <summary>
        /// 发布数据点消息
        /// </summary>
        Task PublishDataPointAsync<T>(T message);

        /// <summary>
        /// 发布告警消息
        /// </summary>
        Task PublishAlarmAsync<T>(T message);

        /// <summary>
        /// 发布设备状态消息
        /// </summary>
        Task PublishDeviceStatusAsync<T>(T message);

        /// <summary>
        /// 发布OPC数据消息
        /// </summary>
        Task PublishOpcDataAsync<T>(T message);
    }

    /// <summary>
    /// 消息发布者实现
    /// </summary>
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IRabbitMQConnectionService _connectionService;
        private readonly ILogger<MessagePublisher> _logger;
        private readonly ExchangeConfiguration _exchangeConfig;
        private readonly RoutingKeyConfiguration _routingKeyConfig;
        private IChannel _channel;
        private bool _disposed;

        public MessagePublisher(
            IRabbitMQConnectionService connectionService,
            IOptions<ExchangeConfiguration> exchangeConfig,
            IOptions<RoutingKeyConfiguration> routingKeyConfig,
            ILogger<MessagePublisher> logger
        )
        {
            _connectionService =
                connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exchangeConfig =
                exchangeConfig.Value ?? throw new ArgumentNullException(nameof(exchangeConfig));
            _routingKeyConfig =
                routingKeyConfig.Value ?? throw new ArgumentNullException(nameof(routingKeyConfig));
        }

        public async Task InitializeChannelAsync()
        {
            _channel = await _connectionService.CreateChannel();
            // 声明所有Exchange
            await DeclareExchanges();

            _logger.LogInformation("消息发布者已初始化");
        }

        private async Task DeclareExchanges()
        {
            // 数据采集主题Exchange (Topic类型)
            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeConfig.DataTopic,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            // 命令控制Exchange (Direct类型)
            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeConfig.CommandDirect,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false
            );

            // 事件广播Exchange (Fanout类型)
            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeConfig.EventsFanout,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false
            );

            // 告警Exchange (Topic类型)
            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeConfig.AlarmTopic,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            // OPC数据Exchange (Topic类型)
            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeConfig.OpcTopic,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            _logger.LogInformation("所有Exchange已声明");
        }

        public async Task PublishAsync<T>(
            string exchange,
            T message,
            string routingKey = "",
            bool persistent = true
        )
        {
            try
            {
                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);
                var properties = new BasicProperties(); //创建消息属性
                properties.Persistent = persistent; //消息持久化
                properties.ContentType = "application/json"; //消息内容类型
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); //消息时间戳
                properties.MessageId = Guid.NewGuid().ToString(); //唯一消息ID
                await _channel.BasicPublishAsync(
                    exchange: exchange, //交换机
                    routingKey: routingKey, //路由键
                    mandatory: true, //强制投递
                    basicProperties: properties, //消息属性
                    body: body //消息体
                );
                _logger.LogDebug(
                    $"消息已发布到Exchange: {exchange}, RoutingKey: {routingKey}, MessageType: {typeof(T).Name}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发布消息失败: Exchange={exchange}, RoutingKey={routingKey}");
                throw;
            }
        }

        public async Task PublishAlarmAsync<T>(T message)
        {
            await PublishAsync(
                exchange: _exchangeConfig.AlarmTopic,
                message: message,
                routingKey: _routingKeyConfig.Alarm
            );
        }

        public async Task PublishDataPointAsync<T>(T message)
        {
            await PublishAsync(
                exchange: _exchangeConfig.DataTopic,
                message: message,
                routingKey: _routingKeyConfig.RealtimeData
            );
        }

        public async Task PublishDeviceStatusAsync<T>(T message)
        {
            await PublishAsync(
                exchange: _exchangeConfig.CommandDirect,
                message: message,
                routingKey: _routingKeyConfig.DeviceStatus
            );
        }

        public async Task PublishOpcDataAsync<T>(T message)
        {
            await PublishAsync(
                exchange: _exchangeConfig.OpcTopic,
                message: message,
                routingKey: _routingKeyConfig.OpcData
            );
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _channel?.CloseAsync();
                _channel?.Dispose();
                _logger.LogInformation("消息发布者已释放");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放消息发布者时出错");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
