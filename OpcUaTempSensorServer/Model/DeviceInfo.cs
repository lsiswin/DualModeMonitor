using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace OpcUaTempSensorServer.Model
{
    public class DeviceInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// 安装位置
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 串口配置
        /// </summary>
        public SerialPortConfig PortConfig { get; set; }

        /// <summary>
        /// 启用状态
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
