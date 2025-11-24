using LiveChartsCore;
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
