using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Convert
{
    public enum AlertStatus
    {
        //未处理
        Unhandled,
        //已处理
        Handled,
        //重试
        Retry
    }
}
