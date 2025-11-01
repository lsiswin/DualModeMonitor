using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Models.Enums;

namespace DualModeMonitorSystem.Models
{
    /// <summary>
    /// 设备信息模型
    /// </summary>
    public class DeviceInfo : ModelBase
    {
        #region 私有字段

        private string _portName;
        private string _position;
        private double _temperature;
        private double _humidity;
        private double _voltage;
        private DeviceStatus _status;
        private DateTime _lastUpdated;
        private string _deviceId;
        private string _deviceName;
        private bool _isOnline;
        private int _id;
        #endregion

        #region 公共属性
        /// <summary>
        /// ID 标识
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; SetPropertyWithValidation(ref _id, value); }
        }
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public string Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public double Temperature
        {
            get => _temperature;
            set
            {
                if (SetProperty(ref _temperature, value))
                {
                    RaisePropertyChanged(nameof(TemperatureText));
                    CheckTemperatureStatus();
                }
            }
        }

        public double Humidity
        {
            get => _humidity;
            set
            {
                if (SetProperty(ref _humidity, value))
                {
                    RaisePropertyChanged(nameof(HumidityText));
                    CheckHumidityStatus();
                }
            }
        }

        public double Voltage
        {
            get => _voltage;
            set
            {
                if (SetProperty(ref _voltage, value))
                {
                    RaisePropertyChanged(nameof(VoltageText));
                }
            }
        }

        public DeviceStatus Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    RaisePropertyChanged(nameof(StatusText));
                }
            }
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set
            {
                if (SetProperty(ref _lastUpdated, value))
                {
                    RaisePropertyChanged(nameof(LastUpdatedText));
                }
            }
        }

        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        #endregion

        #region 计算属性

        [NotMapped]
        public string TemperatureText => $"{Temperature:F1}°C";
        [NotMapped]
        public string HumidityText => $"{Humidity:F1}%";
        [NotMapped]
        public string VoltageText => $"{Voltage:F2}V";
        [NotMapped]
        public string StatusText => Status switch
        {
            DeviceStatus.Normal => "正常",
            DeviceStatus.Warning => "警告",
            DeviceStatus.Error => "错误",
            DeviceStatus.Offline => "离线",
            _ => "未知"
        };
        [NotMapped]
        public string LastUpdatedText => LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");

        #endregion

        #region 构造函数

        public DeviceInfo()
        {
            DeviceId = Guid.NewGuid().ToString("N").Substring(0, 8);
            DeviceName = "未命名设备";
            PortName = "COM1";
            Position = "未知位置";
            Temperature = 0;
            Humidity = 0;
            Voltage = 0;
            Status = DeviceStatus.Offline;
            LastUpdated = DateTime.Now;
            IsOnline = false;
        }

        #endregion

        #region 私有方法

        private void CheckTemperatureStatus()
        {
            if (Temperature > 30 || Temperature < 0)
            {
                Status = DeviceStatus.Warning;
            }
            else if (Temperature > 40 || Temperature < -10)
            {
                Status = DeviceStatus.Error;
            }
        }

        private void CheckHumidityStatus()
        {
            if (Humidity > 80 || Humidity < 20)
            {
                Status = DeviceStatus.Warning;
            }
            else if (Humidity > 95 || Humidity < 10)
            {
                Status = DeviceStatus.Error;
            }
        }

        #endregion

        #region 公共方法

        public void UpdateData(double temperature, double humidity, double voltage)
        {
            Temperature = temperature;
            Humidity = humidity;
            Voltage = voltage;
            LastUpdated = DateTime.Now;
            IsOnline = true;
            if (Status == DeviceStatus.Offline)
                Status = DeviceStatus.Normal;
        }

        public override string ToString() => $"{DeviceName} ({PortName}) - {Position}";

        #endregion
    }
}
