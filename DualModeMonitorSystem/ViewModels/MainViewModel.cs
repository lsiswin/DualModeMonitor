using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Models;

namespace DualModeMonitorSystem.ViewModels
{
    public class MainViewModel
    {
        private readonly IRegionManager regionManager;

        public DelegateCommand<MenuItem> NavigateCommand => new DelegateCommand<MenuItem>(NavigateTo);
        public List<MenuItem> MenuList { get; }

        public MainViewModel()
        {
            
        }
        public MainViewModel(IRegionManager regionManager)
        {
            MenuList = new List<MenuItem>
            {
                new MenuItem { Title = "仪表盘", ViewName = "DashboardView" },
                new MenuItem { Title = "系统设置", ViewName = "SettingsView" },
                new MenuItem { Title = "串口配置", ViewName = "SerialConfigView" },
                new MenuItem { Title = "历史数据", ViewName = "HistoryDataView" },
                new MenuItem{Title="实时监控",ViewName="RealTimeMonitorView" }
            };
            this.regionManager = regionManager;
        }

        public void NavigateTo(MenuItem item)
        {
            regionManager.RequestNavigate("MainContentRegion", item.ViewName);
        }
    }
}
