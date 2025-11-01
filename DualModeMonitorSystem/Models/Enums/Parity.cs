using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Models.Enums
{
    /// <summary>
    /// 校验位枚举
    /// </summary>
    public enum Parity
    {
        None = 0,// 无校验
        Odd = 1,// 奇校验
        Even = 2,// 偶校验
        Mark = 3,// 标记校验
        Space = 4// 空格校验
    }
}
