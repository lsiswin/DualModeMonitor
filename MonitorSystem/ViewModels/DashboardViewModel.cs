using System.Collections.ObjectModel;
using DualModeMonitorSystem.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Options;
using MonitorLibrary.Models.Dto;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Models;
using MonitorRabbitMQService.Services;
using SkiaSharp;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 仪表盘视图模型
    /// </summary>
    public class DashboardViewModel : ViewModelBase, INavigationAware
    {
        private readonly IDeviceService deviceService;
        private readonly IRabbitMQConnectionService connectionService;
        private readonly QueueConfiguration _queueConfig;
        private readonly IMessageConsumer _messageConsumer;

        public ObservableCollection<DeviceInfoDto> Devices { get; set; }

        private DeviceInfoDto _selectedDevice;
        private bool _isSubscribed;

        public DeviceInfoDto SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                RaisePropertyChanged();
            }
        }

        public DashboardViewModel(
            IDeviceService deviceService,
            IRabbitMQConnectionService connectionService,
            IOptions<QueueConfiguration> queueConfig,
            IMessageConsumer messageConsumer
        )
        {
            this.deviceService = deviceService;
            _queueConfig =
                queueConfig?.Value ?? throw new ArgumentNullException(nameof(queueConfig));
            this.connectionService = connectionService;
            this._messageConsumer = messageConsumer;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _messageConsumer.StopConsuming(_queueConfig.OpcData);
            _isSubscribed = false;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            InitializaAsync();
        }

        private async Task InitializaAsync()
        {
            if (Devices == null)
                Devices = new ObservableCollection<DeviceInfoDto>();
            var response = await deviceService.GetAllDevicesAsync();
            if (response.Success)
            {
                SelectedDevice = new DeviceInfoDto();
                Devices.Clear();
                foreach (var device in response.Data)
                {
                    var temp = device.DataPoints.FirstOrDefault(dp => dp.Code.Contains("Temp"));
                    var humidity = device.DataPoints.FirstOrDefault(dp => dp.Code.Contains("Hum"));

                    Devices.Add(
                        new DeviceInfoDto()
                        {
                            Name = device.Name,
                            Temperature = new DataPointDto()
                            {
                                Name = temp.Name,
                                Code = temp.Code,
                                UpperLimit = temp.UpperLimit,
                                LowerLimit = temp.LowerLimit,
                                Value = 0,
                            },
                            Humidity = new DataPointDto()
                            {
                                Name = humidity.Name,
                                Code = humidity.Code,
                                UpperLimit = humidity.UpperLimit,
                                LowerLimit = humidity.LowerLimit,
                                Value = 0.00,
                            },
                        }
                    );
                }
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
                SelectedDevice = Devices.FirstOrDefault();
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
            var device = Devices.FirstOrDefault(dp => dp.Name == opcMsg.Name);
            if (device == null)
            {
                return;
            }
            if (opcMsg.DataPointCode.Equals(device.Temperature.Code))
            {
                device.Temperature.Value = opcMsg.Value;
            }
            if (opcMsg.DataPointCode.Equals(device.Humidity.Code))
            {
                device.Humidity.Value = opcMsg.Value;
            }
        }
    }
}
