using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Services;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 串口配置视图模型
    /// </summary>
    public class DeviceConfigViewModel : ViewModelBase, INavigationAware
    {
        private readonly IDeviceService deviceService;
        public List<DataBits> DataBits { get; set; } = Enum.GetValues(typeof(DataBits)).Cast<DataBits>().ToList();
        public List<StopBits> StopBits { get; set; } = Enum.GetValues(typeof(StopBits)).Cast<StopBits>().ToList();
        public List<Parity> Parity { get; set; } = Enum.GetValues(typeof(Parity)).Cast<Parity>().ToList();
        public List<BaudRate> BaudRates { get; set; } = Enum.GetValues(typeof(BaudRate)).Cast<BaudRate>().ToList();

        private HumitureDevices selectDevice;

        private SerialPortConfig serialPort;

        private DataPoint _selectedDataPoint;

        public DataPoint SelectedDataPoint
        {
            get { return _selectedDataPoint; }
            set { _selectedDataPoint = value;RaisePropertyChanged(); }
        }
        private int _selectedTabIndex;

        /// <summary>
        /// 绑定选项卡
        /// </summary>
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    RaisePropertyChanged();
                    if (value == 1 )
                    {
                        // 执行数据点初始化逻辑
                        ExecuteSwitchToSecondTab();
                    }

                }
            }
        }

        private async Task ExecuteSwitchToSecondTab()
        {
            var response =  await deviceService.GetDataPointByDevice(SelectDevice.Id);
            DataPoints.AddRange(response.Data);
        }

        /// <summary>
        /// 选中的设备数据
        /// </summary>
        public HumitureDevices SelectDevice
        {
            get { return selectDevice; }
            set { selectDevice = value; RaisePropertyChanged();SerialPort = value.SerialPortConfig; }
        }
        /// <summary>
        /// 选中设备对应的串口配置
        /// </summary>
        public SerialPortConfig SerialPort
        {
            get { return serialPort; }
            set { serialPort = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 所选设备所包含的所有数据点
        /// </summary>
        public ObservableCollection<DataPoint> DataPoints { get; set; }=new ObservableCollection<DataPoint>();
        /// <summary>
        /// 所有设备集合
        /// </summary>
        public ObservableCollection<HumitureDevices> Devices { get; set; } = new ObservableCollection<HumitureDevices>();
        public DeviceConfigViewModel(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
           
        }

        private async void LoadDevices() {
            var result = await deviceService.GetAllDevicesAsync();
            Devices.AddRange(result.Data);            
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
            LoadDevices();
        }
    }
}
