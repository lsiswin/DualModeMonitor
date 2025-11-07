using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    /// <summary>
    /// 警报级别枚举
    /// </summary>
    public enum AlertLevel
    {
        
        Normal,//正常
        Warning,//警告
        Error,//错误
        Offline//离线
    }
}
