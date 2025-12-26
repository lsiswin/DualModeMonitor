using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using DryIoc.Messages;
using DualModeMonitorSystem.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Options;
using MonitorLibrary.Models.Dto;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Models;
using MonitorRabbitMQService.Services;
using MonitorSystem.Services;
using SkiaSharp;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 仪表盘视图模型
    /// </summary>
    public class DashboardViewModel : ViewModelBase, INavigationAware
    {
        private readonly IDeviceService deviceService;
        private readonly IDeviceDataService _dataService;
        public IDeviceDataService DataService => _dataService;
        private DeviceInfoDto _selectedDevice;

        private readonly ObservableCollection<ObservableValue> _tempValues = new();
        private readonly ObservableCollection<ObservableValue> _humiValues = new();
        private string _chartName;

        public string ChartName
        {
            get { return _chartName; }
            set
            {
                _chartName = value;
                RaisePropertyChanged();
            }
        }

        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public DeviceInfoDto SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                // 切换设备时，清空旧设备的趋势图
                _tempValues.Clear();
                _humiValues.Clear();
                ChartName = $"{_selectedDevice.Name} - 时间 (s)";
                RaisePropertyChanged();
            }
        }

        public DashboardViewModel(
            IDeviceService deviceService,
            IDeviceDataService dataService,
            IRabbitMQConnectionService connectionService,
            IOptions<QueueConfiguration> queueConfig,
            IMessageConsumer messageConsumer
        )
        {
            this.deviceService = deviceService;
            this._dataService = dataService;
            InitChat();
        }

        private void InitChat()
        {
            // 定义颜色
            var tempColor = SKColors.OrangeRed;
            var humiColor = SKColors.DeepSkyBlue;
            var axisLabelColor = SKColor.Parse("#86909C");
            // 定义数据点中心填充色（白色或其他高亮色）
            var pointFillColor = SKColors.WhiteSmoke;
            // 解决中文乱码：获取系统中的中文字体（如微软雅黑）
            var chineseTypeface = SKTypeface.FromFamilyName("Microsoft YaHei");
            var axisPaint = new SolidColorPaint(axisLabelColor) { SKTypeface = chineseTypeface };
            var tempPaint = new SolidColorPaint(tempColor) { SKTypeface = chineseTypeface };
            var humiPaint = new SolidColorPaint(humiColor) { SKTypeface = chineseTypeface };
            var dataLabelPaint = new SolidColorPaint(SKColors.WhiteSmoke)
            {
                SKTypeface = chineseTypeface,
                ZIndex = 6,
                // 可以根据需要调整标签字体大小
            };
            Series = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Name = "温度",
                    Values = _tempValues,
                    GeometrySize = 3,
                    // 设置点的填充（中心颜色）
                    GeometryFill = new SolidColorPaint(pointFillColor),
                    // 设置点的边框（使用与线条相同的颜色）
                    GeometryStroke = new SolidColorPaint(tempColor) { StrokeThickness = 2 },
                    Stroke = new SolidColorPaint(tempColor) { StrokeThickness = 3 },
                    Fill = new LinearGradientPaint(
                        new[] { tempColor.WithAlpha(40), SKColors.Transparent }
                    ),
                    DataLabelsPaint = dataLabelPaint, // 设置带中文字体的画笔
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top, // 标签位置
                    DataLabelsFormatter = (point) =>
                    {
                        // point.Context.Index 是当前点的索引
                        // 比如：每隔 5 个点显示一个标签，其余返回空字符串
                        if (point.Context.Index % 3 != 0)
                            return "";

                        return $"{point.PrimaryValue:F1}℃";
                    },
                    ScalesYAt = 0, // 关联左轴
                    LineSmoothness = 1, // 曲线平滑度，1表示完全平滑
                },
                new LineSeries<ObservableValue>
                {
                    Name = "湿度",
                    Values = _humiValues,
                    Stroke = new SolidColorPaint(humiColor) { StrokeThickness = 3 },
                    Fill = new LinearGradientPaint(
                        new[] { humiColor.WithAlpha(40), SKColors.Transparent }
                    ),
                    DataLabelsPaint = dataLabelPaint, // 设置带中文字体的画笔
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top, // 标签位置
                    DataLabelsFormatter = (point) =>
                    {
                        // point.Context.Index 是当前点的索引
                        // 比如：每隔 5 个点显示一个标签，其余返回空字符串
                        if (point.Context.Index % 3 != 0)
                            return "";

                        return $"{point.PrimaryValue:F1}%";
                    },
                    GeometrySize = 3, // 设置点的大小
                    // 设置点的填充
                    GeometryFill = new SolidColorPaint(pointFillColor),
                    // 设置点的边框
                    GeometryStroke = new SolidColorPaint(humiColor) { StrokeThickness = 2 },
                    ScalesYAt = 1, // 关联右轴
                    LineSmoothness = 1, // 曲线平滑度，1表示完全平滑
                },
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "时间 (s)",
                    NamePaint = axisPaint, // 标题使用带字体的画笔
                    LabelsPaint = axisPaint, // 标签使用带字体的画笔
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(10)),
                },
            };

            YAxes = new Axis[]
            {
                // Index 0: 温度轴 (左侧)
                new Axis
                {
                    Name = "温度 (°C)",
                    NamePaint = tempPaint,
                    LabelsPaint = tempPaint,
                    Position = LiveChartsCore.Measure.AxisPosition.Start,
                    Labeler = value => $"{value:F0}℃",
                    // --- 解决密集重叠的关键配置 ---
                    MinStep = 10, // 强制最小步长为10，防止刻度过密
                    ForceStepToMin = true, // 强制生效
                    MinLimit = 0,
                    MaxLimit = 60,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(20)),
                },
                // Index 1: 湿度轴 (右侧)
                new Axis
                {
                    Name = "湿度 (%)",
                    NamePaint = humiPaint,
                    LabelsPaint = humiPaint,
                    Position = LiveChartsCore.Measure.AxisPosition.End,
                    Labeler = value => $"{value:F0}%",
                    // --- 解决密集重叠的关键配置 ---
                    MinStep = 20, // 湿度跨度大，步长设为20
                    ForceStepToMin = true,
                    MinLimit = 0,
                    MaxLimit = 100,
                    SeparatorsPaint = null, // 右侧轴不重复显示网格线，让画面更干净
                },
            };
        }

        public SolidColorPaint TooltipTextPaint { get; set; } =
            new SolidColorPaint(SKColors.Black)
            {
                SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei"),
            };

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _dataService.DataReceived -= OnNewDataArrived;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _dataService.InitializeAsync();
            // 订阅服务事件
            _dataService.DataReceived += OnNewDataArrived;
            Debug.WriteLine($"ViewModel Instance ID: {this.GetHashCode()}");
        }

        private void OnNewDataArrived(OpcDataMessage msg)
        {
            if (_dataService.Devices.Count > 0 && SelectedDevice == null)
                SelectedDevice = _dataService.Devices[0];
            // 过滤：只处理当前选中的设备
            if (SelectedDevice == null || msg.Name != SelectedDevice.Name)
                return;

            // 回到 UI 线程更新图表
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                const int maxPoints = 16;
                if (msg.DataPointCode == SelectedDevice.Temperature.Code)
                {
                    _tempValues.Add(new ObservableValue(msg.Value));
                    if (_tempValues.Count > maxPoints)
                        _tempValues.RemoveAt(0);
                }
                else if (msg.DataPointCode == SelectedDevice.Humidity.Code)
                {
                    _humiValues.Add(new ObservableValue(msg.Value));
                    if (_humiValues.Count > maxPoints)
                        _humiValues.RemoveAt(0);
                }
            });
        }
    }
}
