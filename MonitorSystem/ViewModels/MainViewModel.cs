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
    public class MainViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;

        public DelegateCommand<MenuItem> NavigateCommand { get; }

        public ObservableCollection<MenuItem> MenuItems { get; } =
            new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Title = "仪表盘",
                    ViewName = "DashboardView",
                    Icon = "MonitorDashboard",
                },
                new MenuItem
                {
                    Title = "实时监控",
                    ViewName = "RealTimeMonitorView",
                    Icon = "MonitorEye",
                },
                new MenuItem
                {
                    Title = "设备配置",
                    ViewName = "DeviceConfigView",
                    Icon = "Devices",
                },
                new MenuItem
                {
                    Title = "历史数据",
                    ViewName = "HistoryDataView",
                    Icon = "History",
                },
                new MenuItem
                {
                    Title = "设置",
                    ViewName = "SettingsView",
                    Icon = "Setting",
                },
            };

        private string _latestLog;

        public string LatestLog
        {
            get { return _latestLog; }
            set
            {
                _latestLog = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            NavigateCommand = new DelegateCommand<MenuItem>(NavigateTo);
        }

        private void LogSet(string obj)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LatestLog = obj + timeStamp;
        }

        public void NavigateTo(MenuItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.ViewName))
                return;

            regionManager.RequestNavigate(
                "MainContentRegion",
                item.ViewName,
                nr =>
                {
                    if (nr.Success == false)
                    {
                        // 在这里打断点，查看 nr.Error 的详细信息
                        var error = nr.Exception;
                        Console.WriteLine($"导航失败原因: {error?.Message}");
                        if (error?.InnerException != null)
                        {
                            Console.WriteLine($"内部异常: {error.InnerException.Message}");
                        }
                    }
                }
            );
        }
    }
}
