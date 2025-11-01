using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Convert;
using DualModeMonitorSystem.Models.Enums;

namespace DualModeMonitorSystem.Models
{
    #region 串口配置模型

    /// <summary>
    /// 串口配置模型
    /// </summary>
    public class SerialPortConfig : ModelBase
    {
        #region 私有字段
        private int _id;
        private string _portName;
        private int _baudRate;
        private int _dataBits;
        private StopBits _stopBits;
        private Parity _parity;
        private int _readTimeout;
        private int _writeTimeout;
        private bool _isEnabled;
        private ConnectionStatus _status;
        private DateTime _lastCommunicationTime;
        private long _bytesSent;
        private long _bytesReceived;
        private string _description;

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
            set => SetPropertyWithValidation(ref _portName, value);
        }

        public int BaudRate
        {
            get => _baudRate;
            set => SetPropertyWithValidation(ref _baudRate, value);
        }

        public int DataBits
        {
            get => _dataBits;
            set => SetPropertyWithValidation(ref _dataBits, value);
        }

        public StopBits StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        public Parity Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        public int ReadTimeout
        {
            get => _readTimeout;
            set => SetPropertyWithValidation(ref _readTimeout, value);
        }

        public int WriteTimeout
        {
            get => _writeTimeout;
            set => SetPropertyWithValidation(ref _writeTimeout, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ConnectionStatus Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    RaisePropertyChanged(nameof(StatusText));
                    RaisePropertyChanged(nameof(IsConnected));
                }
            }
        }

        public DateTime LastCommunicationTime
        {
            get => _lastCommunicationTime;
            set
            {
                if (SetProperty(ref _lastCommunicationTime, value))
                {
                    RaisePropertyChanged(nameof(LastCommunicationText));
                }
            }
        }

        public long BytesSent
        {
            get => _bytesSent;
            set
            {
                if (SetProperty(ref _bytesSent, value))
                {
                    RaisePropertyChanged(nameof(BytesSentText));
                }
            }
        }

        public long BytesReceived
        {
            get => _bytesReceived;
            set
            {
                if (SetProperty(ref _bytesReceived, value))
                {
                    RaisePropertyChanged(nameof(BytesReceivedText));
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        #endregion

        #region 计算属性
        [NotMapped]
        public string Summary => $"{PortName} - {BaudRate},{DataBits},{StopBits},{Parity}";
        [NotMapped]
        public string StatusText => Status switch
        {
            ConnectionStatus.Connected => "已连接",
            ConnectionStatus.Connecting => "连接中",
            ConnectionStatus.Disconnected => "未连接",
            ConnectionStatus.Error => "错误",
            _ => "未知"
        };
        [NotMapped]
        public bool IsConnected => Status == ConnectionStatus.Connected;
        [NotMapped]
        public string LastCommunicationText
        {
            get
            {
                var span = DateTime.Now - LastCommunicationTime;
                if (span.TotalSeconds < 60)
                    return $"{(int)span.TotalSeconds}秒前";
                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes}分钟前";
                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours}小时前";
                return $"{(int)span.TotalDays}天前";
            }
        }
        [NotMapped]
        public string BytesSentText => FormatBytes(BytesSent);
        [NotMapped]
        public string BytesReceivedText => FormatBytes(BytesReceived);

        #endregion

        #region 构造函数

        public SerialPortConfig()
        {
            PortName = "COM1";
            BaudRate = 9600;
            DataBits = 8;
            StopBits = StopBits.One;
            Parity = Parity.None;
            ReadTimeout = 1000;
            WriteTimeout = 1000;
            IsEnabled = true;
            Status = ConnectionStatus.Disconnected;
            LastCommunicationTime = DateTime.Now;
            BytesSent = 0;
            BytesReceived = 0;
            Description = "默认串口配置";
        }

        #endregion

        #region 验证方法

        protected override string ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(PortName):
                    if (string.IsNullOrWhiteSpace(PortName))
                        return "端口名称不能为空";
                    if (!PortName.StartsWith("COM"))
                        return "端口名称必须以 COM 开头";
                    break;

                case nameof(BaudRate):
                    var validBaudRates = new[] { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
                    if (Array.IndexOf(validBaudRates, BaudRate) == -1)
                        return "波特率必须是标准值";
                    break;

                case nameof(DataBits):
                    if (DataBits < 5 || DataBits > 8)
                        return "数据位必须在 5-8 之间";
                    break;

                case nameof(ReadTimeout):
                case nameof(WriteTimeout):
                    if (ReadTimeout < 0 || WriteTimeout < 0)
                        return "超时时间不能为负数";
                    break;
            }
            return null;
        }

        #endregion

        #region 公共方法

        public static ObservableCollection<string> GetAvailablePorts()
        {
            var ports = System.IO.Ports.SerialPort.GetPortNames();
            return new ObservableCollection<string>(ports);
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} bytes";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }

        public override string ToString() => Summary;

        #endregion
    }

    #endregion
}