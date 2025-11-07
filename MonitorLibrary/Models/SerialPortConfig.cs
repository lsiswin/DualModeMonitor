using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// 串口配置类（与传感器一对一）
    /// </summary>
    public class SerialPortConfig : BindableBase
    {
        /// <summary>
        /// 配置ID（主键）
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 关联的传感器ID（外键）
        /// </summary>
        [ForeignKey("HumitureDevices")]
        public int DeviceId { get; set; }

        /// <summary>
        /// 串口号（如COM1、COM3）
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string PortName { get; set; }

        /// <summary>
        /// 波特率（使用枚举约束）
        /// </summary>
        [Required]
        public BaudRate BaudRate { get; set; } = BaudRate.B9600;  // 默认9600

        /// <summary>
        /// 数据位（使用枚举约束）
        /// </summary>
        [Required]
        public DataBits DataBits { get; set; } = DataBits.Eight;  // 默认8位


        /// <summary>
        /// 停止位（1/1.5/2）
        /// </summary>
        [Required]
        [MaxLength(5)]
        public StopBits StopBits { get; set; }

        /// <summary>
        /// 校验位（None/Odd/Even/Mark/Space）
        /// </summary>
        [Required]
        [MaxLength(10)]
        public Parity Parity { get; set; }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int Timeout { get; set; } = 1000;
    }
}