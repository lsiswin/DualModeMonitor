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
        private readonly IModbusService modbusService;
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

        public DelegateCommand SaveModbusConfigCommand { get; private set; }
        public DelegateCommand TestConnectionCommand { get; private set; }
        public DelegateCommand TestReadCommand { get; private set; }
        public DelegateCommand AddRegisterMappingCommand { get; private set; }
        public DelegateCommand AddDeviceCommand { get; private set; }
        public DeviceConfigViewModel(IDeviceService deviceService,IDialogService dialogService,IModbusService modbusService)
        {
            this.deviceService = deviceService;
            this.dialogService = dialogService;
            this.modbusService = modbusService;
            AddRegisterMappingCommand = new DelegateCommand(AddRegisterMapping);
            AddDeviceCommand = new DelegateCommand(ExecuteAddDevice);
            TestConnectionCommand = new DelegateCommand(TestConnection);
            SaveModbusConfigCommand = new DelegateCommand(SaveModbusConfig);
            TestReadCommand = new DelegateCommand(TestRead);
        }

        private async void SaveModbusConfig()
        {
            if (SelectedDataPoint == null)
            {
                dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "请先选择要保存的数据点！" } });
                return;
            }

            try
            {
                // 把当前编辑的 ModbusConfig 赋回到数据点
                SelectedDataPoint.ModbusConfig = ModbusConfig;

                var response = await deviceService.UpdateDataPointAsync(SelectedDataPoint);

                if (response != null && response.Success)
                {
                    var updated = response.Data ?? SelectedDataPoint;

                    // 更新本地 DataPoints 集合
                    var existing = DataPoints.FirstOrDefault(dp => dp.Id == updated.Id);
                    if (existing != null)
                    {
                        var idx = DataPoints.IndexOf(existing);
                        DataPoints[idx] = updated;
                    }

                    // 更新 RegisterMappings 显示内容
                    var mapping = RegisterMappings.FirstOrDefault(r => r.DataPointId == updated.Id);
                    if (mapping != null && updated.ModbusConfig != null)
                    {
                        mapping.Address = updated.ModbusConfig.RegisterStart;
                        mapping.Format = updated.ModbusConfig.DataFormat;
                        mapping.Factor = updated.ModbusConfig.DataMultiplier;
                        mapping.Offset = updated.ModbusConfig.Offset;
                        mapping.IsEnabled = updated.EnableAlarm;
                    }

                    dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "保存Modbus配置成功！" } });
                }
                else
                {
                    var msg = response?.Message ?? "保存Modbus配置失败，请重试。";
                    dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", msg } });
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", $"保存失败: {ex.Message}" } });
            }
        }

        private async void TestConnection()
        {
            if (SelectDevice == null)
            {
                dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "请先选择设备！" } });
            }
            else
            {
                bool isSuccess =  await modbusService.ConnectAsync(SelectDevice.SerialPortConfig);
                if (isSuccess)
                {
                    dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "连接成功！" } });
                }
                else
                {
                    dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "连接失败，请检查配置！" } });
                }
            }
        }

        private void AddRegisterMapping()
        {
            dialogService.ShowDialog("AddRegisterMappingDialog", () =>
            {
                // 对话框关闭后的回调，可以刷新数据等操作
            });
        }

        private async void ExecuteAddDevice()
        {
            dialogService.ShowDialog("AddDeviceDialog", (result) =>
            {
                if (result != null && result.Parameters != null && result.Parameters.ContainsKey("device"))
                {
                    var device = result.Parameters.GetValue<HumitureDevices>("device");
                    // 异步创建设备并刷新列表
                    _ = Task.Run(async () =>
                    {
                        var resp = await deviceService.CreateDeviceAsync(device);
                        if (resp != null && resp.Success)
                        {
                            // UI 线程更新
                            App.Current.Dispatcher.Invoke(async () =>
                            {
                                Devices.Add(resp.Data);
                                dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "添加设备成功" } });
                            });
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(() => dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", resp?.Message ?? "添加失败" } }));
                        }
                    });
                }
            });
        }

        private async void LoadDevices() {
            Devices.Clear();
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

        private async void TestRead()
        {
            try
            {
                // Ensure connection
                if (!modbusService.IsConnected)
                {
                    var ok = await modbusService.ConnectAsync(SelectDevice?.SerialPortConfig);
                    if (!ok)
                    {
                        dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "连接设备失败，无法读取" } });
                        return;
                    }
                }

                var cfg = SelectedDataPoint.ModbusConfig;
                ushort start = cfg.RegisterStart;
                int regCount = Math.Max(1, cfg.RegisterLength / 2);
                var regs = await modbusService.ReadHoldingRegistersAsync(start, (ushort)regCount);
                if (regs == null || regs.Length == 0)
                {
                    dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", "未收到响应" } });
                    return;
                }
                else
                {
                    dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", $"读取结果: {regs.FirstOrDefault()}" } });
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog("MessageDialog", new DialogParameters { { "message", $"读取失败: {ex.Message}" } });
            }
        }
    }
}
