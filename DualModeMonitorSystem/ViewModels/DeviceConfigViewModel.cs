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
            set
            {
                _modbusConfig = value;
                RaisePropertyChanged();
            }
        }

        private async Task ExecuteSwitchToSecondTab()
        {
            DataPoints.Clear();
            var response = await deviceService.GetDataPointByDevice(SelectDevice.Id);
            DataPoints.AddRange(response.Data);
        }

        private void DeleteMapping(int id)
        {
            var dataPoint = DataPoints.FirstOrDefault(dp => dp.Id == id);
            if (dataPoint == null)
                return;

            // 可以添加确认对话框
            dialogService.ShowDialog(
                "ConfirmDialog",
                new DialogParameters
                {
                    { "Message", $"确定要删除数据点 '{dataPoint.Name}' 及其 Modbus 配置吗？" },
                },
                async (confirmResult) =>
                {
                    if (confirmResult.Result == ButtonResult.OK)
                    {
                        try
                        {
                            var response = await deviceService.DeleteDataPointAsync(id);

                            if (response != null && response.Success)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    // 从集合中移除
                                    DataPoints.Remove(dataPoint);

                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters { { "message", "删除成功！" } }
                                    );
                                });
                            }
                            else
                            {
                                var msg = response?.Message ?? "删除失败";
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters { { "message", msg } }
                                    );
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                dialogService.ShowDialog(
                                    "MessageDialog",
                                    new DialogParameters
                                    {
                                        { "message", $"删除失败: {ex.Message}" },
                                    }
                                );
                            });
                        }
                    }
                }
            );
        }

        private void EditMapping(DataPoint data)
        {
            var dataPoint = DataPoints.FirstOrDefault(dp => dp.Id == data.Id);
            if (dataPoint == null || dataPoint.ModbusConfig == null)
            {
                dialogService.ShowDialog(
                    "MessageDialog",
                    new DialogParameters { { "message", "未找到数据点或 Modbus 配置！" } }
                );
                return;
            }

            var parameters = new DialogParameters
            {
                { "IsEdit", true },
                { "DataPoint", dataPoint.Clone() },
            };

            dialogService.ShowDialog(
                "AddRegisterMappingDialog",
                parameters,
                async (result) =>
                {
                    if (result.Result == ButtonResult.OK && result.Parameters != null)
                    {
                        try
                        {
                            var currentDataPoint = result.Parameters.GetValue<DataPoint>(
                                "DataPoint"
                            );
                            // 调用更新服务
                            var response = await deviceService.UpdateDataPointAsync(
                                currentDataPoint
                            );

                            if (response != null && response.Success)
                            {
                                var updated = response.Data ?? dataPoint;

                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    // 更新本地集合
                                    var existing = DataPoints.FirstOrDefault(dp =>
                                        dp.Id == updated.Id
                                    );
                                    if (existing != null)
                                    {
                                        var idx = DataPoints.IndexOf(existing);
                                        DataPoints[idx] = updated;
                                    }
                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters { { "message", "更新成功！" } }
                                    );
                                });
                            }
                            else
                            {
                                var msg = response?.Message ?? "更新失败，请重试。";
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters { { "message", msg } }
                                    );
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                dialogService.ShowDialog(
                                    "MessageDialog",
                                    new DialogParameters
                                    {
                                        { "message", $"更新失败: {ex.Message}" },
                                    }
                                );
                            });
                        }
                    }
                }
            );
        }

        /// <summary>
        /// 选中的设备数据
        /// </summary>
        public HumitureDevices SelectDevice
        {
            get { return selectDevice; }
            set
            {
                selectDevice = value;
                RaisePropertyChanged();
                SerialPort = value.SerialPortConfig;
                ExecuteSwitchToSecondTab();
            }
        }

        /// <summary>
        /// 选中设备对应的串口配置
        /// </summary>
        public SerialPortConfig SerialPort
        {
            get { return serialPort; }
            set
            {
                serialPort = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 所选设备所包含的所有数据点
        /// </summary>
        public ObservableCollection<DataPoint> DataPoints { get; set; } =
            new ObservableCollection<DataPoint>();

        /// <summary>
        /// 所有设备集合
        /// </summary>
        public ObservableCollection<HumitureDevices> Devices { get; set; } =
            new ObservableCollection<HumitureDevices>();

        public DelegateCommand SaveModbusConfigCommand { get; private set; }
        public DelegateCommand TestConnectionCommand { get; private set; }
        public DelegateCommand TestReadCommand { get; private set; }
        public DelegateCommand AddRegisterMappingCommand { get; private set; }
        public DelegateCommand AddDeviceCommand { get; private set; }

        public DelegateCommand<DataPoint> EditCommand { get; private set; }
        public DelegateCommand<DataPoint> DeleteCommand { get; private set; }
        public object IsEdit { get; private set; }

        public DeviceConfigViewModel(
            IDeviceService deviceService,
            IDialogService dialogService,
            IModbusService modbusService
        )
        {
            this.deviceService = deviceService;
            this.dialogService = dialogService;
            this.modbusService = modbusService;
            AddRegisterMappingCommand = new DelegateCommand(AddRegisterMapping);
            AddDeviceCommand = new DelegateCommand(ExecuteAddDevice);
            TestConnectionCommand = new DelegateCommand(TestConnection);
            TestReadCommand = new DelegateCommand(TestRead);
            EditCommand = new DelegateCommand<DataPoint>(dp => EditMapping(dp));
            DeleteCommand = new DelegateCommand<DataPoint>(dp => DeleteMapping(dp.Id));
        }

        private async void TestConnection()
        {
            if (SelectDevice == null)
            {
                dialogService.ShowDialog(
                    "MessageDialog",
                    new DialogParameters { { "message", "请先选择设备！" } }
                );
            }
            else
            {
                bool isSuccess = await modbusService.ConnectAsync(SelectDevice.SerialPortConfig);
                if (isSuccess)
                {
                    dialogService.ShowDialog(
                        "MessageDialog",
                        new DialogParameters { { "message", "连接成功！" } }
                    );
                }
                else
                {
                    dialogService.ShowDialog(
                        "MessageDialog",
                        new DialogParameters { { "message", "连接失败，请检查配置！" } }
                    );
                }
            }
        }

        private async void AddRegisterMapping()
        {
            // 传递当前设备的默认从站地址
            var dialogParams = new DialogParameters();
            dialogParams.Add("IsEdit", false);
            dialogService.ShowDialog(
                "AddRegisterMappingDialog",
                dialogParams,
                async (result) =>
                {
                    if (result.Result == ButtonResult.OK && result.Parameters != null)
                    {
                        try
                        {
                            var resDataPoint = result.Parameters.GetValue<DataPoint>("DataPoint");
                            resDataPoint.DeviceId = SelectDevice.Id;
                            // 调用服务创建数据点
                            var response = await deviceService.CreateDataPointAsync(resDataPoint);

                            if (response != null && response.Success)
                            {
                                // 获取创建后的数据点（包含服务器生成的 ID）
                                var createdDataPoint = response.Data ?? resDataPoint;

                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    // 添加到数据点集合
                                    DataPoints.Add(createdDataPoint);

                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters
                                        {
                                            {
                                                "message",
                                                $"成功添加寄存器映射：{createdDataPoint.Name}\n地址: 0x{createdDataPoint.ModbusConfig.RegisterStart:X4}, 格式: {createdDataPoint.ModbusConfig.DataFormat}"
                                            },
                                        }
                                    );
                                });
                            }
                            else
                            {
                                var errorMsg = response?.Message ?? "添加失败，请重试";
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters
                                        {
                                            { "message", $"添加失败：{errorMsg}" },
                                        }
                                    );
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                dialogService.ShowDialog(
                                    "MessageDialog",
                                    new DialogParameters
                                    {
                                        { "message", $"添加失败: {ex.Message}" },
                                    }
                                );
                            });
                        }
                    }
                }
            );
        }

        private async void ExecuteAddDevice()
        {
            dialogService.ShowDialog(
                "AddDeviceDialog",
                (result) =>
                {
                    if (
                        result != null
                        && result.Parameters != null
                        && result.Parameters.ContainsKey("device")
                    )
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
                                    dialogService.ShowDialog(
                                        "MessageDialog",
                                        new DialogParameters { { "message", "添加设备成功" } }
                                    );
                                });
                            }
                            else
                            {
                                App.Current.Dispatcher.Invoke(
                                    () =>
                                        dialogService.ShowDialog(
                                            "MessageDialog",
                                            new DialogParameters
                                            {
                                                { "message", resp?.Message ?? "添加失败" },
                                            }
                                        )
                                );
                            }
                        });
                    }
                }
            );
        }

        private async void LoadDevices()
        {
            Devices.Clear();
            var result = await deviceService.GetAllDevicesAsync();
            Devices.AddRange(result.Data);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

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
                        dialogService.ShowDialog(
                            "MessageDialog",
                            new DialogParameters { { "message", "连接设备失败，无法读取" } }
                        );
                        return;
                    }
                }

                var cfg = SelectDevice.DataPoints.FirstOrDefault()?.ModbusConfig;
                ushort start = cfg.RegisterStart;
                int regCount = Math.Max(1, cfg.RegisterLength / 2);
                var regs = await modbusService.ReadHoldingRegistersAsync(start, (ushort)regCount);
                if (regs == null || regs.Length == 0)
                {
                    dialogService.ShowDialog(
                        "MessageDialog",
                        new DialogParameters { { "message", "未收到响应" } }
                    );
                    return;
                }
                else
                {
                    dialogService.ShowDialog(
                        "MessageDialog",
                        new DialogParameters { { "message", $"读取结果: {regs.FirstOrDefault()}" } }
                    );
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog(
                    "MessageDialog",
                    new DialogParameters { { "message", $"读取失败: {ex.Message}" } }
                );
            }
        }
    }
}
