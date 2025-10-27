using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DualModeMonitorSystem.Models;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 仪表盘视图模型
    /// </summary>
    public class DashboardViewModel : ViewModelBase, INavigationAware
    {
        public List<Statistica> StatisticsResult { get; set; }
        public ISeries[] Series { get; set; }
            = new ISeries[]
            {                
                new LineSeries<double>
                {                    
                    Name = "cold",
                    Values = new double[] { 32,31, 33, 35, 33, 34, 36 },
                    Fill = null,
                    ScalesYAt = 0,
                    

                },
                new LineSeries<double>
                {
                    Name = "shidu",
                    Values = new double[] { 35,41, 39, 65, 73, 54, 66 },
                    ScalesYAt = 1,

                },
            };

        public ObservableCollection<DeviceDto> Devies { get; set; } 
        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "日期",
                    MinLimit = 0
                }
            };
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Tag = "Tempe",
                Name = "Temperature",
                Labeler = value => $"{value} °C",
                DrawTicksPath = true,
                SubticksPaint = new SolidColorPaint(SKColors.Blue)
                {
                    StrokeThickness = 1
                },
                TicksPaint = new SolidColorPaint(SKColors.Red)
                {
                    StrokeThickness = 1
                },
                TicksAtCenter = true,
                Position = LiveChartsCore.Measure.AxisPosition.Start

            },
            new Axis
            {
                Name = "shidu",
                Position = LiveChartsCore.Measure.AxisPosition.End,
                ShowSeparatorLines = false,
                SubticksPaint = new SolidColorPaint(SKColors.Blue)
                {
                    StrokeThickness = 1
                },
                TicksPaint = new SolidColorPaint(SKColors.Red)
                {
                    StrokeThickness = 1
                },
                Labeler = value => $"{value}%",

            }
        };
        public DashboardViewModel()
        {
            StatisticsResult = new List<Statistica>
            {
                new Statistica { Title = "设备总数", Value = 1200 ,Icon = "DesktopClassic"},
                new Statistica { Title = "在线设备", Value = 350 ,Icon = "MonitorCellphoneStar"},
                new Statistica { Title = "今日警告", Value = 8760 ,Icon = "Alert"},
                new Statistica { Title = "MES上传成功率", Value = 45 ,Icon = "ProgressUpload"}
            };
            Devies =  new ObservableCollection<DeviceDto>
        {
            new DeviceDto { PortName="COM1",Position="位置1",Temperature="36.5",Humidity="45",Tag="Warning",LastUpdated=DateTime.Now},
            new DeviceDto { PortName="COM2",Position="位置2",Temperature="38.2",Humidity="50",Tag="Normal",LastUpdated=DateTime.Now},
            new DeviceDto { PortName="COM3",Position="位置3",Temperature="40.1",Humidity="55",Tag="Error",LastUpdated=DateTime.Now},
new DeviceDto { PortName="COM1",Position="位置1",Temperature="36.5",Humidity="45",Tag="Warning",LastUpdated=DateTime.Now},
            new DeviceDto { PortName="COM2",Position="位置2",Temperature="38.2",Humidity="50",Tag="Normal",LastUpdated=DateTime.Now},
            new DeviceDto { PortName="COM3",Position="位置3",Temperature="40.1",Humidity="55",Tag="Error",LastUpdated=DateTime.Now},
new DeviceDto { PortName="COM1",Position="位置1",Temperature="36.5",Humidity="45",Tag="Warning",LastUpdated=DateTime.Now},
            new DeviceDto { PortName="COM2",Position="位置2",Temperature="38.2",Humidity="50",Tag="Normal",LastUpdated=DateTime.Now},
            new DeviceDto { PortName="COM3",Position="位置3",Temperature="40.1",Humidity="55",Tag="Error",LastUpdated=DateTime.Now},

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
