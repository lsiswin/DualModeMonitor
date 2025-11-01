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
    /// 告警信息模型
    /// </summary>
    public class AlertInfo : ModelBase
    {
        #region 私有字段

        private string _alertId;
        private string _title;
        private string _description;
        private DateTime _time;
        private AlertLevel _level;
        private AlertStatus _status;
        private string _deviceId;
        private string _deviceName;
        private double _value;
        private double _threshold;
        private bool _isAcknowledged;
        private string _acknowledgedBy;
        private DateTime? _acknowledgedTime;
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
        public string AlertId
        {
            get => _alertId;
            set => SetProperty(ref _alertId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime Time
        {
            get => _time;
            set
            {
                if (SetProperty(ref _time, value))
                {
                    RaisePropertyChanged(nameof(TimeText));
                    RaisePropertyChanged(nameof(ElapsedTime));
                }
            }
        }

        public AlertLevel Level
        {
            get => _level;
            set
            {
                if (SetProperty(ref _level, value))
                {
                    RaisePropertyChanged(nameof(LevelText));
                }
            }
        }

        public AlertStatus Status
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

        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public double Threshold
        {
            get => _threshold;
            set => SetProperty(ref _threshold, value);
        }

        public bool IsAcknowledged
        {
            get => _isAcknowledged;
            set => SetProperty(ref _isAcknowledged, value);
        }

        public string AcknowledgedBy
        {
            get => _acknowledgedBy;
            set => SetProperty(ref _acknowledgedBy, value);
        }

        public DateTime? AcknowledgedTime
        {
            get => _acknowledgedTime;
            set => SetProperty(ref _acknowledgedTime, value);
        }

        #endregion

        #region 计算属性
        [NotMapped]
        public string TimeText => Time.ToString("HH:mm");
        [NotMapped]
        public string LevelText => Level switch
        {
            AlertLevel.Normal => "信息",
            AlertLevel.Warning => "警告",
            AlertLevel.Error => "错误",
            AlertLevel.Offline => "严重",
            _ => "未知"
        };
        [NotMapped]
        public string StatusText => Status switch
        {
            AlertStatus.New => "新告警",
            AlertStatus.Acknowledged => "已确认",
            AlertStatus.Processing => "处理中",
            AlertStatus.Resolved => "已解决",
            AlertStatus.Closed => "已关闭",
            _ => "未知"
        };
        [NotMapped]
        public string ElapsedTime
        {
            get
            {
                var span = DateTime.Now - Time;
                if (span.TotalMinutes < 1) return "刚刚";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}分钟前";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours}小时前";
                return $"{(int)span.TotalDays}天前";
            }
        }

        #endregion

        #region 命令
        [NotMapped]
        public DelegateCommand ProcessAlertCommand { get; private set; }

        #endregion

        #region 构造函数

        public AlertInfo()
        {
            AlertId = Guid.NewGuid().ToString("N");
            Title = "告警";
            Description = "";
            Time = DateTime.Now;
            Level = AlertLevel.Normal;
            Status = AlertStatus.New;
            DeviceId = "";
            DeviceName = "";
            Value = 0;
            Threshold = 0;
            IsAcknowledged = false;
            ProcessAlertCommand = new DelegateCommand(OnProcessAlert);

        }

        public AlertInfo(string title, string description, AlertLevel level)
            : this()
        {
            Title = title;
            Description = description;
            Level = level;
        }

        #endregion

        #region 公共方法

        public void Acknowledge(string user)
        {
            IsAcknowledged = true;
            AcknowledgedBy = user;
            AcknowledgedTime = DateTime.Now;
            if (Status == AlertStatus.New)
                Status = AlertStatus.Acknowledged;
        }

        public void Resolve()
        {
            Status = AlertStatus.Resolved;
        }

        public void Close()
        {
            Status = AlertStatus.Closed;
        }

        public override string ToString() => $"[{LevelText}] {Title} - {Description}";

        #endregion

        #region 命令处理

        private void OnProcessAlert()
        {
            // 处理告警逻辑由外部处理
            if (!IsAcknowledged)
            {
                Acknowledge("系统");
            }
        }

        #endregion
    }

}
