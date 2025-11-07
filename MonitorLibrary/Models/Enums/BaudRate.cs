using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Enums
{
    /// <summary>
    /// 串口波特率（工业常用标准值）
    /// </summary>
    public enum BaudRate
    {
        B2400 = 2400,
        B4800 = 4800,
        B9600 = 9600,   // 最常用
        B19200 = 19200,
        B38400 = 38400,
        B57600 = 57600,
        B115200 = 115200
    }

}
