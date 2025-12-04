using LiveChartsCore;
using LiveChartsCore.Defaults;
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
        public static DateTime[] Dates  = new DateTime[]
        {
            new DateTime(2025, 11, 24,11,12,1),
            new DateTime(2025, 11, 24,11,13,2),
            new DateTime(2025, 11, 24,11,14,3),
            new DateTime(2025, 11, 24,11,15,4),
            new DateTime(2025, 11, 24,11,16,5)
        };

        public ISeries[] Series { get; set; }
            

        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
               new Axis
                {
                    Name = "日期",
                    Labeler = value => new DateTime((long)value).ToString("hh:mm"), // 日期格式化
                    TextSize = 12,
                    // 时间轴配置
                    UnitWidth = TimeSpan.FromSeconds(1).Ticks, // 每个数据点代表1天
                    // 刻度线样式
                    LabelsPaint = new SolidColorPaint(SKColors.White),
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
                LabelsPaint = new SolidColorPaint(SKColors.White),
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100)),
                Position = LiveChartsCore.Measure.AxisPosition.Start

            },
            new Axis
            {
                Name = "shidu",
                Position = LiveChartsCore.Measure.AxisPosition.End,
                ShowSeparatorLines = false,
                LabelsPaint = new SolidColorPaint(SKColors.White),
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(100)),
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
            this.Series = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Name = "cold",
                    Values = GetDateTimePoints(Dates, new double[] { 32,31, 33, 35, 33 }),
                    Fill = null,
                    ScalesYAt = 0,
                    Stroke = new SolidColorPaint(SKColors.Blue, 3),
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColors.DarkBlue, 2),
                    GeometryFill = new SolidColorPaint(SKColors.LightBlue)
                },
                new LineSeries<DateTimePoint>
                {
                    Name = "shidu",
                    Values = GetDateTimePoints(Dates, new double[] { 35,41, 39, 65, 73, 54, 66 }),
                    ScalesYAt = 1,

                },
            };
        }
        public List<DateTimePoint> GetDateTimePoints(DateTime[] dates, double[] values)
        {
            var points = new List<DateTimePoint>();
            for (int i = 0; i < dates.Length; i++)
            {
                points.Add(new DateTimePoint(dates[i], values[i]));
            }
            return points;
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
