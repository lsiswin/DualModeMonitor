using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Dto
{
    public class DataPointDto : BindableBase
    {
        private double _value;
        public double Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 数据点名称（如"温度""湿度"）
        /// </summary>
        [Required, MaxLength(50)]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;

        private string _code;

        /// <summary>
        /// 数据点编码（唯一标识，如"Temp""Hum"）
        /// </summary>
        [Required, MaxLength(20)]
        public string Code
        {
            get { return _code; }
            set { SetProperty(ref _code, value); }
        }
        private decimal _upperLimit;
        private decimal _lowerLimit;

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
    }
}
