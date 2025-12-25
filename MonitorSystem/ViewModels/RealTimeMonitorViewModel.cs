using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MonitorLibrary.Models.Dto;
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
        private readonly IMessageConsumer _messageConsumer;
        private readonly QueueConfiguration _queueConfig;
        private string _displayMode;
        private bool _isSubscribed;

        public string DisplayMode
        {
            get => _displayMode;
            set => SetProperty(ref _displayMode, value); // 使用 Prism 的 SetProperty 触发通知
        }

        // 切换模式的命令
        public DelegateCommand<string> ChangeModeCommand { get; }

        public RealTimeMonitorViewModel(
            IRabbitMQConnectionService connectionService,
            IOptions<QueueConfiguration> queueConfig,
            IMessageConsumer messageConsumer
        )
        {
            _queueConfig =
                queueConfig?.Value ?? throw new ArgumentNullException(nameof(queueConfig));
            this.connectionService = connectionService;
            this._messageConsumer = messageConsumer;
            ChangeModeCommand = new DelegateCommand<string>(mode =>
            {
                DisplayMode = mode;
            });
            MonitorNodes = new ObservableCollection<SensorMonitorDto>();
        }

        public ObservableCollection<SensorMonitorDto> MonitorNodes { get; set; }

        public async Task Initialize()
        {
            try
            {
                if (!_isSubscribed)
                {
                    if (_messageConsumer is MessageConsumer consumerImpl)
                    {
                        await consumerImpl.InitializeChannelAsync();
                    }

                    await _messageConsumer.SubscribeOpcData<OpcDataMessage>(
                        async (message) =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateMonitorNodes(message);
                            });
                            await Task.CompletedTask;
                        }
                    );

                    _isSubscribed = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"消费启动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新监控节点集合
        /// </summary>
        /// <param name="opcDataMessage"></param>
        private void UpdateMonitorNodes(OpcDataMessage opcMsg)
        {
            if (opcMsg == null || string.IsNullOrEmpty(opcMsg.Name))
                return;

            // 1. 根据设备名称查找 DTO
            var node = MonitorNodes.FirstOrDefault(x => x.DeviceName == opcMsg.Name);

            if (node == null)
            {
                // 2. 首次发现设备，初始化 DTO
                node = new SensorMonitorDto { DeviceName = opcMsg.Name };
                System.Windows.Application.Current.Dispatcher.Invoke(() => MonitorNodes.Add(node));
            }

            // 3. 根据 DataPointCode 聚合数据 (Temp-xxx 或 Humi-xxx)
            if (opcMsg.DataPointCode.StartsWith("Temp", StringComparison.OrdinalIgnoreCase))
            {
                node.Temperature = opcMsg.Value;
                node.TempQuality = opcMsg.Quality;
            }
            else if (opcMsg.DataPointCode.StartsWith("Humi", StringComparison.OrdinalIgnoreCase))
            {
                node.Humidity = opcMsg.Value;
                node.HumQuality = opcMsg.Quality;
            }

            node.LastUpdate = opcMsg.Timestamp;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //_messageConsumer.StopConsuming(_queueConfig.OpcData);
            MonitorNodes.Clear();
            _isSubscribed = false;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Initialize();
        }
    }
}
