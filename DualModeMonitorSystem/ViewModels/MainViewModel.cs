using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace DualModeMonitorSystem.ViewModels
{
    public class MainViewModel
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
        public MainViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            NavigateCommand = new DelegateCommand<MenuItem>(NavigateTo);
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
