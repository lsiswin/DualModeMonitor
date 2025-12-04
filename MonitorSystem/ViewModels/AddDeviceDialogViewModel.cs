using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.ViewModels
{
    public class AddDeviceDialogViewModel : BindableBase, IDialogAware
    {
        #region Constants
        private const int MAX_NAME_LENGTH = 50;
        private const int MAX_DEVICE_CODE_LENGTH = 20;
        private const int MIN_TIMEOUT = 100;
        private const int MAX_TIMEOUT = 10000;
        #endregion

        #region Properties
        private string _title = "添加设备";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private HumitureDevices _currentDevice;
        public HumitureDevices CurrentDevice
        {
            get => _currentDevice;
            set => SetProperty(ref _currentDevice, value);
        }

        private string _validationMessage;
        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public DialogCloseListener RequestClose { get; }
        #endregion

        #region Commands
        private DelegateCommand _confirmCommand;
        public DelegateCommand ConfirmCommand =>
            _confirmCommand ??= new DelegateCommand(OnConfirm, CanConfirm);

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ??= new DelegateCommand(OnCancel);
        #endregion

        #region Constructor
        public AddDeviceDialogViewModel()
        {
        }
        #endregion

        #region Dialog Methods
        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
            // 清理资源
            if (CurrentDevice != null)
            {
                CurrentDevice.PropertyChanged -= OnDevicePropertyChanged;
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            InitializeDevice();
        }
        #endregion

        #region Private Methods
        private void InitializeDevice()
        {
            CurrentDevice = new HumitureDevices();

            if (CurrentDevice.SerialPortConfig == null)
            {
                CurrentDevice.SerialPortConfig = new SerialPortConfig
                {
                    // 设置默认值
                    BaudRate = BaudRate.B9600,
                    DataBits = DataBits.Eight,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                    Timeout = 1000
                };
            }

            // 订阅属性变化事件
            CurrentDevice.PropertyChanged += OnDevicePropertyChanged;
        }

        private void OnDevicePropertyChanged(object sender, EventArgs e)
        {
            ConfirmCommand.RaiseCanExecuteChanged();
            // 实时验证并更新提示信息
            ValidateDevice(out _);
        }

        private bool CanConfirm()
        {
            if (CurrentDevice == null) return false;

            string errorMessage;
            bool isValid = ValidateDevice(out errorMessage);
            ValidationMessage = errorMessage;

            return isValid;
        }

        private bool ValidateDevice(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (CurrentDevice == null)
            {
                errorMessage = "设备信息不能为空";
                return false;
            }

            // 验证设备名称
            if (!ValidateDeviceName(out errorMessage))
                return false;

            // 验证设备编号
            if (!ValidateDeviceCode(out errorMessage))
                return false;

            // 验证串口配置
            if (!ValidateSerialPortConfig(out errorMessage))
                return false;

            return true;
        }

        private bool ValidateDeviceName(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentDevice.Name))
            {
                errorMessage = "设备名称不能为空";
                return false;
            }

            if (CurrentDevice.Name.Length > MAX_NAME_LENGTH)
            {
                errorMessage = $"设备名称不能超过{MAX_NAME_LENGTH}个字符";
                return false;
            }

            // 可选: 检查特殊字符
            if (ContainsInvalidCharacters(CurrentDevice.Name))
            {
                errorMessage = "设备名称包含非法字符";
                return false;
            }

            return true;
        }

        private bool ValidateDeviceCode(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentDevice.DeviceCode))
            {
                errorMessage = "设备编号不能为空";
                return false;
            }

            if (CurrentDevice.DeviceCode.Length > MAX_DEVICE_CODE_LENGTH)
            {
                errorMessage = $"设备编号不能超过{MAX_DEVICE_CODE_LENGTH}个字符";
                return false;
            }

            if (CurrentDevice.DeviceCode.Contains(" "))
            {
                errorMessage = "设备编号不能包含空格";
                return false;
            }

            // 验证设备编号格式 (只允许字母、数字、下划线、连字符)
            if (!Regex.IsMatch(CurrentDevice.DeviceCode, @"^[a-zA-Z0-9_-]+$"))
            {
                errorMessage = "设备编号只能包含字母、数字、下划线和连字符";
                return false;
            }

            return true;
        }

        private bool ValidateSerialPortConfig(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (CurrentDevice.SerialPortConfig == null)
            {
                errorMessage = "串口配置不能为空";
                return false;
            }

            // 验证串口名称
            if (string.IsNullOrWhiteSpace(CurrentDevice.SerialPortConfig.PortName))
            {
                errorMessage = "串口名称不能为空";
                return false;
            }

            // 验证串口名称格式 (例如: COM1, COM10)
            if (!Regex.IsMatch(CurrentDevice.SerialPortConfig.PortName, @"^COM\d{1,3}$", RegexOptions.IgnoreCase))
            {
                errorMessage = "串口名称格式不正确 (例如: COM1, COM2)";
                return false;
            }

            // 可选: 检查串口是否存在
            string[] availablePorts = SerialPort.GetPortNames();
            if (!availablePorts.Contains(CurrentDevice.SerialPortConfig.PortName, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = $"串口 {CurrentDevice.SerialPortConfig.PortName} 不存在或不可用";
                return false;
            }

            // 验证超时时间
            if (CurrentDevice.SerialPortConfig.Timeout < MIN_TIMEOUT ||
                CurrentDevice.SerialPortConfig.Timeout > MAX_TIMEOUT)
            {
                errorMessage = $"超时时间必须在 {MIN_TIMEOUT} 到 {MAX_TIMEOUT} 毫秒之间";
                return false;
            }

            return true;
        }

        private bool ContainsInvalidCharacters(string input)
        {
            // 定义不允许的特殊字符
            char[] invalidChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            return input.IndexOfAny(invalidChars) >= 0;
        }

        private void OnConfirm()
        {
            // 最终验证
            if (!ValidateDevice(out string errorMessage))
            {
                // 可以通过消息服务显示错误
                ValidationMessage = errorMessage;
                return;
            }

            var parameters = new DialogParameters
            {
                { "device", CurrentDevice }
            };

            RequestClose.Invoke(new DialogResult(ButtonResult.OK) { Parameters = parameters });
        }

        private void OnCancel()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }
        #endregion
    }
}