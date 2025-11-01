using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Models
{
    /// <summary>
    /// 系统配置模型
    /// </summary>
    public class SystemConfig : ModelBase
    {
        private string _systemName;
        private string _version;
        private int _dataRefreshInterval;
        private int _alertCheckInterval;
        private bool _enableAutoReconnect;
        private int _reconnectDelay;
        private bool _enableDataLogging;
        private string _dataLogPath;
        private int _maxLogFileSize;
        private bool _enableAlertSound;
        private string _language;
        private int _id;
        /// <summary>
        /// ID 标识
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; SetPropertyWithValidation(ref _id, value); }
        }
        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName
        {
            get => _systemName;
            set => SetProperty(ref _systemName, value);
        }
        /// <summary>
        /// 版本号
        /// </summary>

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        /// <summary>
        /// 数据刷新间隔（毫秒）
        /// </summary>
        public int DataRefreshInterval
        {
            get => _dataRefreshInterval;
            set => SetPropertyWithValidation(ref _dataRefreshInterval, value);
        }
        /// <summary>
        /// 告警检查间隔（毫秒）
        /// </summary>

        public int AlertCheckInterval
        {
            get => _alertCheckInterval;
            set => SetPropertyWithValidation(ref _alertCheckInterval, value);
        }
        /// <summary>
        /// 连接自动重连
        /// </summary>

        public bool EnableAutoReconnect
        {
            get => _enableAutoReconnect;
            set => SetProperty(ref _enableAutoReconnect, value);
        }

        /// <summary>
        /// 连接重连延迟（毫秒）
        /// </summary>
        public int ReconnectDelay
        {
            get => _reconnectDelay;
            set => SetPropertyWithValidation(ref _reconnectDelay, value);
        }

        /// <summary>
        /// 数据日志记录
        /// </summary>
        public bool EnableDataLogging
        {
            get => _enableDataLogging;
            set => SetProperty(ref _enableDataLogging, value);
        }

        /// <summary>
        /// 数据日志存储路径
        /// </summary>
        public string DataLogPath
        {
            get => _dataLogPath;
            set => SetProperty(ref _dataLogPath, value);
        }
        /// <summary>
        /// 最大日志文件大小（MB）
        /// </summary>
        public int MaxLogFileSize
        {
            get => _maxLogFileSize;
            set => SetPropertyWithValidation(ref _maxLogFileSize, value);
        }
        /// <summary>
        /// 告警声音提示
        /// </summary>
        public bool EnableAlertSound
        {
            get => _enableAlertSound;
            set => SetProperty(ref _enableAlertSound, value);
        }
        /// <summary>
        /// 语言
        /// </summary>
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public SystemConfig()
        {
            SystemName = "双模监控系统";
            Version = "1.0.0";
            DataRefreshInterval = 1000;
            AlertCheckInterval = 5000;
            EnableAutoReconnect = true;
            ReconnectDelay = 5000;
            EnableDataLogging = true;
            DataLogPath = @"C:\Logs\MonitorSystem";
            MaxLogFileSize = 10;
            EnableAlertSound = true;
            Language = "zh-CN";
        }

        protected override string ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(DataRefreshInterval):
                    if (DataRefreshInterval < 100)
                        return "数据刷新间隔不能小于100毫秒";
                    if (DataRefreshInterval > 60000)
                        return "数据刷新间隔不能超过60秒";
                    break;

                case nameof(AlertCheckInterval):
                    if (AlertCheckInterval < 1000)
                        return "告警检查间隔不能小于1秒";
                    if (AlertCheckInterval > 300000)
                        return "告警检查间隔不能超过5分钟";
                    break;

                case nameof(ReconnectDelay):
                    if (ReconnectDelay < 1000)
                        return "重连延迟不能小于1秒";
                    if (ReconnectDelay > 60000)
                        return "重连延迟不能超过60秒";
                    break;

                case nameof(MaxLogFileSize):
                    if (MaxLogFileSize < 1)
                        return "日志文件大小不能小于1MB";
                    if (MaxLogFileSize > 1024)
                        return "日志文件大小不能超过1GB";
                    break;
            }
            return null;
        }
    }

}
