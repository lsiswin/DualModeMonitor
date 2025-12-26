using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DualModeMonitorSystem.Services;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Dto;
using MonitorRabbitMQService.Models;
using MonitorRabbitMQService.Services;

namespace MonitorSystem.Services
{
    public class OpcDataService : IDeviceDataService
    {
        private readonly IMessageConsumer _messageConsumer;
        private readonly IDeviceService _deviceService;
        private bool _isSubscribed = false;

        // 页面绑定的数据源，单例模式下它永远不会被销毁
        public ObservableCollection<DeviceInfoDto> Devices { get; } = new();
        public event Action<OpcDataMessage> DataReceived;

        public OpcDataService(IMessageConsumer messageConsumer, IDeviceService deviceService)
        {
            _messageConsumer = messageConsumer;
            _deviceService = deviceService;
        }

        public async Task InitializeAsync()
        {
            // 1. 确保只加载一次初始数据
            if (Devices.Count == 0)
            {
                var response = await _deviceService.GetAllDevicesAsync();
                if (response.Success)
                {
                    foreach (var device in response.Data)
                    {
                        // 这里复用你之前的转换逻辑
                        Devices.Add(MapToDto(device));
                    }
                }
            }

            // 2. 确保只订阅一次 RabbitMQ
            if (!_isSubscribed)
            {
                if (_messageConsumer is MessageConsumer consumerImpl)
                {
                    await consumerImpl.InitializeChannelAsync();
                }

                await _messageConsumer.SubscribeOpcData<OpcDataMessage>(
                    async (message) =>
                    {
                        // 使用 Dispatcher 回到 UI 线程更新集合
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            UpdateMonitorNodes(message);
                        });
                    }
                );

                _isSubscribed = true;
                Debug.WriteLine("[Service] 全局订阅已启动，监控中...");
            }
        }

        private void UpdateMonitorNodes(OpcDataMessage opcMsg)
        {
            if (opcMsg == null)
                return;
            var device = Devices.FirstOrDefault(dp => dp.Name == opcMsg.Name);
            if (device == null)
                return;

            // 根据编码更新值
            if (opcMsg.DataPointCode.Equals(device.Temperature.Code))
                device.Temperature.Value = opcMsg.Value;
            else if (opcMsg.DataPointCode.Equals(device.Humidity.Code))
                device.Humidity.Value = opcMsg.Value;
            device.LastUpdate = opcMsg.Timestamp;
            // 2. 关键：触发事件，通知 ViewModel 更新图表
            DataReceived?.Invoke(opcMsg);
        }

        private DeviceInfoDto MapToDto(HumitureDevices device)
        {
            var temp = device.DataPoints.FirstOrDefault(dp => dp.Code.Contains("Temp"));
            var humidity = device.DataPoints.FirstOrDefault(dp => dp.Code.Contains("Hum"));

            return new DeviceInfoDto()
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
            };
        }
    }
}
