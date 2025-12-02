using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// 数据点（传感器的单个参数，如温度、湿度）
    /// </summary>
    public class DataPoint : BindableBase, ICloneable
    {
        private int _id;
        private int _deviceId;
        private string _name;
        private string _code;
        private string _unit;
        private int _collectInterval = 10;
        private decimal _upperLimit;
        private decimal _lowerLimit;
        private decimal _validMin;
        private decimal _validMax;
        private int _dataRetentionDays = 30;
        private bool _enableAlarm = true;
        private int _alarmDelay = 30;

        [Key]
        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        /// <summary>
        /// 外键：关联的传感器ID
        /// </summary>
        [ForeignKey("HumitureDevices")]
        public int DeviceId
        {
            get { return _deviceId; }
            set { SetProperty(ref _deviceId, value); }
        }

        /// <summary>
        /// 数据点名称（如"温度""湿度"）
        /// </summary>
        [Required, MaxLength(50)]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 数据点编码（唯一标识，如"Temp""Hum"）
        /// </summary>
        [Required, MaxLength(20)]
        public string Code
        {
            get { return _code; }
            set { SetProperty(ref _code, value); }
        }

        /// <summary>
        /// 单位（如"℃""%RH"）
        /// </summary>
        [Required, MaxLength(10)]
        public string Unit
        {
            get { return _unit; }
            set { SetProperty(ref _unit, value); }
        }

        /// <summary>
        /// 采集间隔（秒）
        /// </summary>
        [Required, Range(1, 3600)]
        public int CollectInterval
        {
            get { return _collectInterval; }
            set { SetProperty(ref _collectInterval, value); }
        }

        /// <summary>
        /// 上限阈值（超此值告警）
        /// </summary>
        [Required]
        public decimal UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }

        /// <summary>
        /// 下限阈值（低此值告警）
        /// </summary>
        [Required]
        public decimal LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        /// <summary>
        /// 有效范围最小值（过滤异常值）
        /// </summary>
        [Required]
        public decimal ValidMin
        {
            get { return _validMin; }
            set { SetProperty(ref _validMin, value); }
        }

        /// <summary>
        /// 有效范围最大值（过滤异常值）
        /// </summary>
        [Required]
        public decimal ValidMax
        {
            get { return _validMax; }
            set { SetProperty(ref _validMax, value); }
        }

        /// <summary>
        /// 数据保留天数
        /// </summary>
        [Required, Range(1, 3650)]
        public int DataRetentionDays
        {
            get { return _dataRetentionDays; }
            set { SetProperty(ref _dataRetentionDays, value); }
        }

        /// <summary>
        /// 是否启用告警
        /// </summary>
        public bool EnableAlarm
        {
            get { return _enableAlarm; }
            set { SetProperty(ref _enableAlarm, value); }
        }

        /// <summary>
        /// 告警延迟（秒，避免瞬时波动）
        /// </summary>
        public int AlarmDelay
        {
            get { return _alarmDelay; }
            set { SetProperty(ref _alarmDelay, value); }
        }

        // 导航属性
        /// <summary>
        /// 关联的传感器
        /// </summary>
        public HumitureDevices? HumitureDevices { get; set; }

        /// <summary>
        /// 一对一：该数据点的Modbus配置
        /// </summary>
        public ModbusConfig ModbusConfig { get; set; }

        /// <summary>
        /// 一对多：该数据点的历史记录
        /// </summary>
        public ICollection<DataPointRecord> Records { get; set; } = new List<DataPointRecord>();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
