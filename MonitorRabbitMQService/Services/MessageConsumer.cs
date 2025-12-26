using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorLibrary.Reactive;
using MonitorRabbitMQService.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MonitorRabbitMQService.Services
{
    /// <summary>
    /// 消息消费者接口
    /// </summary>
    public interface IMessageConsumer
    {
        /// <summary>
        /// 订阅队列消息
        /// </summary>
        Task Subscribe<T>(
            string queueName,
            Func<T, Task> onMessageReceived,
            string exchange = "",
            string routingKey = ""
        );

        /// <summary>
        /// 订阅实时数据消息
        /// </summary>
        Task SubscribeRealtimeData<T>(Func<T, Task> onMessageReceived);

        /// <summary>
        /// 订阅告警消息
        /// </summary>
        Task SubscribeAlarm<T>(Func<T, Task> onMessageReceived);

        /// <summary>
        /// 订阅设备状态消息
        /// </summary>
        Task SubscribeDeviceStatus<T>(Func<T, Task> onMessageReceived);

        /// <summary>
        /// 订阅OPC数据消息
        /// </summary>
        Task SubscribeOpcData<T>(Func<T, Task> onMessageReceived);

        /// <summary>
        /// 停止消费
        /// </summary>
        void StopConsuming(string consumerTag);
    }

    /// <summary>
    /// 消息消费者实现
    /// </summary>
    public class MessageConsumer : IMessageConsumer, IDisposable
    {
        private readonly IRabbitMQConnectionService _connectionService;
        private readonly ReactiveLogger _logger;
        private readonly ExchangeConfiguration _exchangeConfig;
        private readonly QueueConfiguration _queueConfig;
        private readonly RoutingKeyConfiguration _routingKeyConfig;
        private IChannel _channel;
        private bool _disposed;

        public MessageConsumer(
            IRabbitMQConnectionService connectionService,
            IOptions<ExchangeConfiguration> exchangeConfig,
            IOptions<QueueConfiguration> queueConfig,
            IOptions<RoutingKeyConfiguration> routingKeyConfig,
            ReactiveLogger logger
        )
        {
            _connectionService =
                connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exchangeConfig =
                exchangeConfig.Value ?? throw new ArgumentNullException(nameof(exchangeConfig));
            _queueConfig =
                queueConfig.Value ?? throw new ArgumentNullException(nameof(queueConfig));
            _routingKeyConfig =
                routingKeyConfig.Value ?? throw new ArgumentNullException(nameof(routingKeyConfig));
        }

        public async Task InitializeChannelAsync()
        {
            _channel = await _connectionService.CreateChannel();

            // 设置QoS - 每次只预取1条消息
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            // 声明所有队列
            DeclareQueues();
            _channel.ChannelShutdownAsync += (sender, args) =>
            {
                _logger.LogWarning(
                    $"[警告] 信道已关闭！原因: {args.ReplyText}, 引起原因: {args.Initiator}"
                );
                return Task.CompletedTask;
            };
            _logger.LogInformation("消息消费者已初始化");
        }

        private void DeclareQueues()
        {
            // 声明实时数据队列并绑定到数据Exchange
            _channel.QueueDeclareAsync(
                queue: _queueConfig.RealtimeData,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBindAsync(
                queue: _queueConfig.RealtimeData,
                exchange: _exchangeConfig.DataTopic,
                routingKey: _routingKeyConfig.RealtimeData
            );

            // 声明持久化数据队列
            _channel.QueueDeclareAsync(
                queue: _queueConfig.PersistData,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBindAsync(
                queue: _queueConfig.PersistData,
                exchange: _exchangeConfig.DataTopic,
                routingKey: _routingKeyConfig.RealtimeData
            );

            // 声明告警队列
            _channel.QueueDeclareAsync(
                queue: _queueConfig.Alarm,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBindAsync(
                queue: _queueConfig.Alarm,
                exchange: _exchangeConfig.AlarmTopic,
                routingKey: _routingKeyConfig.Alarm
            );

            // 声明设备状态队列
            _channel.QueueDeclareAsync(
                queue: _queueConfig.DeviceStatus,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBindAsync(
                queue: _queueConfig.DeviceStatus,
                exchange: _exchangeConfig.DataTopic,
                routingKey: _routingKeyConfig.DeviceStatus
            );

            // 声明OPC数据队列
            _channel.QueueDeclareAsync(
                queue: _queueConfig.OpcData,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBindAsync(
                queue: _queueConfig.OpcData,
                exchange: _exchangeConfig.OpcTopic,
                routingKey: _routingKeyConfig.OpcData
            );

            _logger.LogInformation("所有队列已声明并绑定");
        }

        public Task Subscribe<T>(
            string queueName,
            Func<T, Task> onMessageReceived,
            string exchange = "",
            string routingKey = ""
        )
        {
            try
            {
                // 如果提供了exchange和routingKey,则绑定队列
                if (!string.IsNullOrEmpty(exchange) && !string.IsNullOrEmpty(routingKey))
                {
                    _channel.QueueBindAsync(
                        queue: queueName,
                        exchange: exchange,
                        routingKey: routingKey
                    );
                }
                var consumer = new AsyncEventingBasicConsumer(_channel);
                // 开始消费消息，手动确认模式
                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageJson = Encoding.UTF8.GetString(body);
                        var message = JsonConvert.DeserializeObject<T>(messageJson);

                        _logger.LogDebug(
                            $"收到消息: Queue={queueName}, MessageType={typeof(T).Name}, RoutingKey={ea.RoutingKey}"
                        );

                        await onMessageReceived(message);

                        // 手动确认消息
                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);

                        _logger.LogDebug(
                            $"消息处理成功: Queue={queueName}, DeliveryTag={ea.DeliveryTag}"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            $"处理消息时出错: Queue={queueName}, RoutingKey={ea.RoutingKey}"
                        );

                        // 拒绝消息并重新入队(可根据需求调整)
                        await _channel.BasicNackAsync(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            requeue: true
                        );
                    }
                };
                var consumerTag = _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false, // 手动确认
                    consumer: consumer
                );

                _logger.LogInformation($"开始消费队列: {queueName}, ConsumerTag={consumerTag}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"订阅队列消息时出错: Queue={queueName}");
                throw;
            }
        }

        public async Task SubscribeAlarm<T>(Func<T, Task> onMessageReceived)
        {
            await Subscribe(_queueConfig.Alarm, onMessageReceived);
        }

        public async Task SubscribeDeviceStatus<T>(Func<T, Task> onMessageReceived)
        {
            await Subscribe(_queueConfig.DeviceStatus, onMessageReceived);
        }

        public async Task SubscribeOpcData<T>(Func<T, Task> onMessageReceived)
        {
            await Subscribe(_queueConfig.OpcData, onMessageReceived);
        }

        public async Task SubscribeRealtimeData<T>(Func<T, Task> onMessageReceived)
        {
            await Subscribe(_queueConfig.RealtimeData, onMessageReceived);
        }

        public void StopConsuming(string consumerTag)
        {
            try
            {
                _channel.BasicCancelAsync(consumerTag);
                _logger.LogInformation($"已停止消费: ConsumerTag={consumerTag}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"停止消费失败: ConsumerTag={consumerTag}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _channel?.CloseAsync();
                _channel?.Dispose();
                _logger.LogInformation("消息消费者已释放");
            }
            catch (Exception ex)
            {
                _logger.LogError("释放消息消费者时出错", ex);
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
