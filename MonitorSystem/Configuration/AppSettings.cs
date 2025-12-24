using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSystem.Configuration
{
    /// <summary>
    /// MonitorApi配置
    /// </summary>
    public class MonitorApiConfiguration
    {
        /// <summary>
        /// API基础地址
        /// </summary>
        public string BaseUrl { get; set; } = "https://localhost:7137";
    }

    /// <summary>
    /// 应用程序设置
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 数据刷新间隔(毫秒)
        /// </summary>
        public int DataRefreshIntervalMs { get; set; } = 1000;

        /// <summary>
        /// 启用告警声音
        /// </summary>
        public bool EnableAlarmSound { get; set; } = true;

        /// <summary>
        /// 启用告警弹窗
        /// </summary>
        public bool EnableAlarmPopup { get; set; } = true;

        /// <summary>
        /// 最大历史数据点数
        /// </summary>
        public int MaxHistoryDataPoints { get; set; } = 1000;

        /// <summary>
        /// 主题 (Light/Dark)
        /// </summary>
        public string Theme { get; set; } = "Light";
    }
}
