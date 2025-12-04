using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Models;
using MonitorRabbitMQService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 实时监控视图模型
    /// </summary>
    public class RealTimeMonitorViewModel : ViewModelBase, INavigationAware
    {
        private readonly IRabbitMQConnectionService connectionService;
        private readonly QueueConfiguration _queueConfig;

        public RealTimeMonitorViewModel(
            IRabbitMQConnectionService connectionService,
            IOptions<QueueConfiguration> queueConfig
        )
        {
            _queueConfig =
                queueConfig.Value ?? throw new ArgumentNullException(nameof(queueConfig));
            this.connectionService = connectionService;
        }

        public ObservableCollection<OpcDataMessage> MonitorNodes { get; set; }

        public async Task Initialize()
        {
            MonitorNodes = new ObservableCollection<OpcDataMessage>();
            var channel = await connectionService.CreateChannel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var opcDataMessage = System.Text.Json.JsonSerializer.Deserialize<OpcDataMessage>(
                    message
                );
                if (opcDataMessage != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        var existingNode = MonitorNodes.FirstOrDefault(n =>
                            n.DataPointId == opcDataMessage.DataPointId
                        );
                        if (existingNode != null)
                        {
                            // 更新现有节点的数据
                            existingNode.Value = opcDataMessage.Value;
                            existingNode.Quality = opcDataMessage.Quality;
                            existingNode.Timestamp = opcDataMessage.Timestamp;
                        }
                        else
                        {
                            // 添加新节点
                            MonitorNodes.Add(opcDataMessage);
                        }
                    });
                }
                await Task.Yield();
            };
            string consumerTag = await channel.BasicConsumeAsync(
                queue: _queueConfig.OpcData,
                autoAck: true,
                consumer: consumer
            );
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext) { }
    }
}
