using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Models.Enums
{
    /// <summary>
    /// 连接状态枚举
    /// </summary>
    public enum ConnectionStatus
    {
        Disconnected,   // 未连接
        Connected,      // 已连接
        Connecting,     // 连接中
        Error          // 错误
    }
}
