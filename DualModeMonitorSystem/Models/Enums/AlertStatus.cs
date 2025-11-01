using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Models.Enums
{
    /// <summary>
    /// 告警状态枚举
    /// </summary>
    public enum AlertStatus
    {
        New,            // 新告警
        Acknowledged,   // 已确认
        Processing,     // 处理中
        Resolved,       // 已解决
        Closed          // 已关闭
    }
}
