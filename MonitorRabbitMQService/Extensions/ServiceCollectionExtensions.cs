using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Services;

namespace MonitorRabbitMQService.Extensions
{
    /// <summary>
    /// RabbitMQ服务注册扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加RabbitMQ消息服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQMessageService(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // 注册配置
            services.Configure<RabbitMQConfiguration>(config =>
                configuration.GetSection("RabbitMQ")
            );

            services.Configure<ExchangeConfiguration>(config =>
                configuration.GetSection("RabbitMQ:Exchanges")
            );

            services.Configure<QueueConfiguration>(config =>
                configuration.GetSection("RabbitMQ:Queues")
            );

            services.Configure<RoutingKeyConfiguration>(config =>
                configuration.GetSection("RabbitMQ:RoutingKeys")
            );

            // 注册服务(单例模式)
            services.AddSingleton<IRabbitMQConnectionService, RabbitMQConnectionService>();
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();

            return services;
        }

        /// <summary>
        /// 添加RabbitMQ消息服务(自定义配置)
        /// </summary>
        public static IServiceCollection AddRabbitMQMessageService(
            this IServiceCollection services,
            RabbitMQConfiguration rabbitMQConfig,
            ExchangeConfiguration? exchangeConfig = null,
            QueueConfiguration? queueConfig = null,
            RoutingKeyConfiguration? routingKeyConfig = null
        )
        {
            // 使用默认配置(如果未提供)
            exchangeConfig ??= new ExchangeConfiguration();
            queueConfig ??= new QueueConfiguration();
            routingKeyConfig ??= new RoutingKeyConfiguration();

            // 注册配置
            services.Configure<RabbitMQConfiguration>(config =>
            {
                config.HostName = rabbitMQConfig.HostName;
                config.Port = rabbitMQConfig.Port;
                config.UserName = rabbitMQConfig.UserName;
                config.Password = rabbitMQConfig.Password;
                config.VirtualHost = rabbitMQConfig.VirtualHost;
                config.RequestedConnectionTimeout = rabbitMQConfig.RequestedConnectionTimeout;
                config.RequestedHeartbeat = rabbitMQConfig.RequestedHeartbeat;
                config.AutomaticRecoveryEnabled = rabbitMQConfig.AutomaticRecoveryEnabled;
                config.NetworkRecoveryInterval = rabbitMQConfig.NetworkRecoveryInterval;
            });

            services.Configure<ExchangeConfiguration>(config =>
            {
                config.DataTopic = exchangeConfig.DataTopic;
                config.CommandDirect = exchangeConfig.CommandDirect;
                config.EventsFanout = exchangeConfig.EventsFanout;
                config.AlarmTopic = exchangeConfig.AlarmTopic;
                config.OpcTopic = exchangeConfig.OpcTopic;
            });

            services.Configure<QueueConfiguration>(config =>
            {
                config.RealtimeData = queueConfig.RealtimeData;
                config.PersistData = queueConfig.PersistData;
                config.OpcData = queueConfig.OpcData;
                config.Alarm = queueConfig.Alarm;
                config.DeviceStatus = queueConfig.DeviceStatus;
            });

            services.Configure<RoutingKeyConfiguration>(config =>
            {
                config.RealtimeData = routingKeyConfig.RealtimeData;
                config.HistoricalData = routingKeyConfig.HistoricalData;
                config.Alarm = routingKeyConfig.Alarm;
                config.DeviceStatus = routingKeyConfig.DeviceStatus;
                config.OpcData = routingKeyConfig.OpcData;
            });

            // 注册服务
            services.AddSingleton<IRabbitMQConnectionService, RabbitMQConnectionService>();
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();

            return services;
        }
    }
}
