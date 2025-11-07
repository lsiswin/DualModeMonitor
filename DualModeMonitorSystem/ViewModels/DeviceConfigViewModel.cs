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

        public ObservableCollection<HumitureDevices> Devices { get; set; } = new ObservableCollection<HumitureDevices>();

        public List<DataBits> DataBits { get; set; } = Enum.GetValues(typeof(DataBits)).Cast<DataBits>().ToList();
        public List<StopBits> StopBits { get; set; } = Enum.GetValues(typeof(StopBits)).Cast<StopBits>().ToList();
        public List<Parity> Parity { get; set; } = Enum.GetValues(typeof(Parity)).Cast<Parity>().ToList();

        private HumitureDevices selectDevice;

        public HumitureDevices SelectDevice
        {
            get { return selectDevice; }
            set { selectDevice = value; RaisePropertyChanged();SerialPort = value.SerialPortConfig; }
        }
        private SerialPortConfig serialPort;

        public SerialPortConfig SerialPort
        {
            get { return serialPort; }
            set { serialPort = value; RaisePropertyChanged(); }
        }

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
