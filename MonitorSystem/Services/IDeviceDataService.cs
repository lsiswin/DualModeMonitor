using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models.Dto;
using MonitorRabbitMQService.Models;

namespace MonitorSystem.Services
{
    public interface IDeviceDataService
    {
        // 全局唯一的设备列表
        ObservableCollection<DeviceInfoDto> Devices { get; }

        // 新增：数据到达时的事件
        event Action<OpcDataMessage> DataReceived;

        // 初始化方法
        Task InitializeAsync();
    }
}
