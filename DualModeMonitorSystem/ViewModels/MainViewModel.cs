using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Services;
using MonitorLibrary.Models;

namespace DualModeMonitorSystem.ViewModels
{
    public class MainViewModel:BindableBase
    {
        private readonly IRegionManager regionManager;

        public DelegateCommand<MenuItem> NavigateCommand { get; }

        public ObservableCollection<MenuItem> MenuItems { get; } = new ObservableCollection<MenuItem>
        {
            new MenuItem{ Title = "仪表盘", ViewName = "DashboardView",Icon="MonitorDashboard" },
            new MenuItem{ Title = "实时监控", ViewName = "RealTimeMonitorView",Icon= "MonitorEye" },
            new MenuItem{ Title = "设备配置", ViewName = "DeviceConfigView" ,Icon = "Devices"},
            new MenuItem{ Title = "历史数据", ViewName = "HistoryDataView" ,Icon = "History"},
            new MenuItem{ Title = "设置", ViewName = "SettingsView" ,Icon="Setting"},
        };

        private string _latestLog;

        public string LatestLog
        {
            get { return _latestLog; }
            set { _latestLog = value;RaisePropertyChanged(); }
        }

        public MainViewModel(IRegionManager regionManager,IModbusService modbusService)
        {

            this.regionManager = regionManager;
            modbusService.LogMessage.Subscribe(LogSet);
            NavigateCommand = new DelegateCommand<MenuItem>(NavigateTo);
        }

        private void LogSet(string obj)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LatestLog = obj+timeStamp;
        }

        public void NavigateTo(MenuItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.ViewName))
                return;

            // 验证区域名称是否正确（需与视图中prism:RegionManager.RegionName一致）
            regionManager.RequestNavigate("MainContentRegion", item.ViewName);
        }
    }
}
