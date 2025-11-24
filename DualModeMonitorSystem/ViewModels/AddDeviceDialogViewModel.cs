using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.ViewModels
{
    public class AddDeviceDialogViewModel : BindableBase, IDialogAware
    {
        private string _title = "添加设备";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _deviceCode;
        public string DeviceCode
        {
            get => _deviceCode;
            set => SetProperty(ref _deviceCode, value);
        }

        private string _location;
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private string _remark;
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        // --- 串口配置字段 ---
        private string _portName = "COM1";
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        private BaudRate _baudRate = BaudRate.B9600;
        public BaudRate BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        private DataBits _dataBits = DataBits.Eight;
        public DataBits DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private StopBits _stopBits = StopBits.One;
        public StopBits StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private Parity _parity = Parity.None;
        public Parity Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        private int _timeout = 1000;
        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }

        public DialogCloseListener RequestClose { get; }

        private DelegateCommand _confirmCommand;
        public DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(OnConfirm);

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(OnCancel);

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }

        private void OnConfirm()
        {
            var p = new DialogParameters();
            var device = new HumitureDevices
            {
                Name = this.Name,
                DeviceCode = this.DeviceCode,
                Location = this.Location,
                Remark = this.Remark,
                SerialPortConfig = new SerialPortConfig
                {
                    PortName = this.PortName,
                    BaudRate = this.BaudRate,
                    DataBits = this.DataBits,
                    StopBits = this.StopBits,
                    Parity = this.Parity,
                    Timeout = this.Timeout
                }
            };
            p.Add("device", device);
            RequestClose.Invoke(new DialogResult(ButtonResult.OK) { Parameters = p });
        }

        private void OnCancel()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }
    }
}
