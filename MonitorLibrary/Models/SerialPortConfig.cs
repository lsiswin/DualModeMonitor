using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Ports;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// 串口配置类（与传感器一对一）
    /// </summary>
    public class SerialPortConfig : BindableBase
    {
        private int _id;
        private int _deviceId;
        private string _portName;
        private BaudRate _baudRate = BaudRate.B9600;
        private DataBits _dataBits = DataBits.Eight;
        private StopBits _stopBits = StopBits.One;
        private Parity _parity = Parity.None;
        private int _timeout = 1000;

        /// <summary>
        /// 配置ID（主键）
        /// </summary>
        [Key]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// 关联的传感器ID（外键）
        /// </summary>
        [ForeignKey("HumitureDevices")]
        public int DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        /// <summary>
        /// 设备地址（从机ID）
        /// </summary>
        public byte _deviceAddress;
        public byte DeviceAddress
        {
            get => _deviceAddress;
            set => SetProperty(ref _deviceAddress, value);
        }

        /// <summary>
        /// 串口号（如COM1、COM3）
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        /// <summary>
        /// 波特率（使用枚举约束）
        /// </summary>
        [Required]
        public BaudRate BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        /// <summary>
        /// 数据位（使用枚举约束）
        /// </summary>
        [Required]
        public DataBits DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        /// <summary>
        /// 停止位（1/1.5/2）
        /// </summary>
        [Required]
        public StopBits StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        /// <summary>
        /// 校验位（None/Odd/Even/Mark/Space）
        /// </summary>
        [Required]
        public Parity Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        [Range(100, 10000, ErrorMessage = "超时时间必须在100到10000毫秒之间")]
        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }
    }
}
