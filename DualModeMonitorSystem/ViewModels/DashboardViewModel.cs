using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Models;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 仪表盘视图模型
    /// </summary>
    public class DashboardViewModel : ViewModelBase, INavigationAware
    {
        public List<Statistica> StatisticsResult { get; set; }

        public DashboardViewModel()
        {
            StatisticsResult = new List<Statistica>
            {
                new Statistica { Title = "设备总数", Value = 1200 ,Icon = "DesktopClassic"},
                new Statistica { Title = "在线设备", Value = 350 ,Icon = "MonitorCellphoneStar"},
                new Statistica { Title = "今日警告", Value = 8760 ,Icon = "Alert"},
                new Statistica { Title = "MES上传成功率", Value = 45 ,Icon = "ProgressUpload"}
            };
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
    }
}
