using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MonitorLibrary.Models.Dto;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Models;
using MonitorRabbitMQService.Services;
using MonitorSystem.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 实时监控视图模型
    /// </summary>
    public class RealTimeMonitorViewModel : ViewModelBase, INavigationAware
    {
        private readonly IDeviceDataService _dataService;
        private string _displayMode;

        public string DisplayMode
        {
            get => _displayMode;
            set => SetProperty(ref _displayMode, value); // 使用 Prism 的 SetProperty 触发通知
        }

        // 切换模式的命令
        public DelegateCommand<string> ChangeModeCommand { get; }

        public RealTimeMonitorViewModel(IDeviceDataService dataService)
        {
            this._dataService = dataService;
            ChangeModeCommand = new DelegateCommand<string>(mode =>
            {
                DisplayMode = mode;
            });
        }

        public IDeviceDataService DataService => _dataService;

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _dataService.InitializeAsync();
            Debug.WriteLine($"ViewModel Instance ID: {this.GetHashCode()}");
        }
    }
}
