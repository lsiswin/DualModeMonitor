using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        #region Private Fields
        private int _id;
        private string _name;
        private string _deviceCode;
        private string _location;
        private DeviceStatus _status = DeviceStatus.Offline;
        private string _remark;
        #endregion

        #region Public Properties
        /// <summary>
        /// 传感器唯一标识（主键）
        /// </summary>
        [Key]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// 传感器名称
        /// </summary>
        [Required(ErrorMessage = "设备名称不能为空")]
        [MaxLength(50, ErrorMessage = "设备名称不能超过50个字符")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 设备编号（唯一）
        /// </summary>
        [Required(ErrorMessage = "设备编号不能为空")]
        [MaxLength(20, ErrorMessage = "设备编号不能超过20个字符")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "设备编号只能包含字母、数字、下划线和连字符")]
        public string DeviceCode
        {
            get => _deviceCode;
            set => SetProperty(ref _deviceCode, value);
        }

        /// <summary>
        /// 安装位置
        /// </summary>
        [MaxLength(100, ErrorMessage = "安装位置不能超过100个字符")]
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        /// <summary>
        /// 设备状态（在线/离线）
        /// </summary>
        public DeviceStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// 备注信息
        /// </summary>
        [MaxLength(500, ErrorMessage = "备注信息不能超过500个字符")]
        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        public SerialPortConfig SerialPortConfig { get; set; }

        public ICollection<DataPoint> DataPoints { get; set; } = new List<DataPoint>();

        #endregion

        #region Constructors
        public HumitureDevices()
        {
            // 初始化默认值
            Status = DeviceStatus.Offline;
        }
        #endregion

        
        
        /// <summary>
        /// 重写ToString方法，方便调试
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({DeviceCode}) - {Status}";
        }

        
    }
}