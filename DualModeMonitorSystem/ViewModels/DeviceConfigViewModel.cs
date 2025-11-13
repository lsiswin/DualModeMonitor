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
        private readonly IDialogService dialogService;
        private HumitureDevices selectDevice;

        private SerialPortConfig serialPort;

        private ModbusConfig _modbusConfig;

        public ModbusConfig ModbusConfig
        {
            get { return _modbusConfig; }
            set { _modbusConfig = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<RegisterMapping> RegisterMappings { get; set; } = new ObservableCollection<RegisterMapping>();
        private DataPoint _selectedDataPoint;

        public DataPoint SelectedDataPoint
        {
            get { return _selectedDataPoint; }
            set { _selectedDataPoint = value;RaisePropertyChanged(); if(value != null) ModbusConfig = value.ModbusConfig; }
        }


        private async Task ExecuteSwitchToSecondTab()
        {            
            DataPoints.Clear();
            RegisterMappings.Clear();
            var response =  await deviceService.GetDataPointByDevice(SelectDevice.Id);
            DataPoints.AddRange(response.Data);
            var mappings = DataPoints.Select(dp => new RegisterMapping
            {
                DataPointId = dp.Id,
                DataType = dp.Name,
                Unit = dp.Unit,
                Address = dp.ModbusConfig.RegisterStart,
                Format = dp.ModbusConfig.DataFormat,
                Factor = dp.ModbusConfig.DataMultiplier,
                Offset = dp.ModbusConfig.Offset, // 如果已添加
                IsEnabled = dp.EnableAlarm,
                EditCommand = new DelegateCommand(() => EditMapping(dp.Id)),
                DeleteCommand = new DelegateCommand(() => DeleteMapping(dp.Id))
            }).ToList();
            RegisterMappings.AddRange(mappings);
        }

        private void DeleteMapping(int id)
        {
            DataPoints.Remove(DataPoints.FirstOrDefault(dp => dp.Id == id));
        }

        private void EditMapping(int id)
        {
            SelectedDataPoint = DataPoints.FirstOrDefault(dp => dp.Id == id);
        }

        /// <summary>
        /// 选中的设备数据
        /// </summary>
        public HumitureDevices SelectDevice
        {
            get { return selectDevice; }
            set { selectDevice = value; RaisePropertyChanged();SerialPort = value.SerialPortConfig; ExecuteSwitchToSecondTab(); }
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

        public DelegateCommand AddRegisterMappingCommand { get; private set; }
        public DeviceConfigViewModel(IDeviceService deviceService,IDialogService dialogService)
        {
            this.deviceService = deviceService;
            this.dialogService = dialogService;
            AddRegisterMappingCommand = new DelegateCommand(AddRegisterMapping);
        }

        private void AddRegisterMapping()
        {
            dialogService.ShowDialog("AddRegisterMappingDialog", () =>
            {
                // 对话框关闭后的回调，可以刷新数据等操作
            });
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
