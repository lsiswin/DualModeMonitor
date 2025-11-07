using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// 温湿度传感器设备主类
    /// </summary>
    public class HumitureDevices : BindableBase
    {
        /// <summary>
        /// 传感器唯一标识（主键）
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 传感器名称
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 设备编号（唯一）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string DeviceCode { get; set; }

        /// <summary>
        /// 安装位置
        /// </summary>
        [MaxLength(100)]
        public string Location { get; set; }

        /// <summary>
        /// 设备状态（在线/离线）
        /// </summary>
        [MaxLength(10)]
        public DeviceStatus Status { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }

        // 导航属性
        /// <summary>
        /// 一对一关联：串口配置
        /// </summary>
        public SerialPortConfig SerialPortConfig { get; set; }

        /// <summary>
        /// 一对多关联：Modbus配置集合
        /// </summary>
        public ICollection<ModbusConfig> ModbusConfigs { get; set; } = new List<ModbusConfig>();

    }
}
