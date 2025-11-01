using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Models.Enums;

namespace DualModeMonitorSystem.Models
{
    /// <summary>
    /// 寄存器映射模型
    /// </summary>
    public class RegisterMapping : ModelBase
    {
        #region 私有字段

        private string _dataType;
        private ushort _address;
        private ModbusDataFormat _format;
        private string _unit;
        private double _factor;
        private double _offset;
        private string _status;
        private double _minValue;
        private double _maxValue;
        private bool _isEnabled;
        private int _id;
        #endregion

        #region 公共属性
        /// <summary>
        /// ID 标识
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; SetPropertyWithValidation(ref _id, value); }
        }
        public string DataType
        {
            get => _dataType;
            set => SetPropertyWithValidation(ref _dataType, value);
        }

        public ushort Address
        {
            get => _address;
            set => SetPropertyWithValidation(ref _address, value);
        }

        public ModbusDataFormat Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public double Factor
        {
            get => _factor;
            set => SetPropertyWithValidation(ref _factor, value);
        }

        public double Offset
        {
            get => _offset;
            set => SetProperty(ref _offset, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public double MinValue
        {
            get => _minValue;
            set => SetPropertyWithValidation(ref _minValue, value);
        }

        public double MaxValue
        {
            get => _maxValue;
            set => SetPropertyWithValidation(ref _maxValue, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        #endregion

        #region 计算属性
        [NotMapped]
        public string AddressHex => $"0x{Address:X4}";
        [NotMapped]
        public string FullDescription => $"{DataType} ({AddressHex}) - {Unit}";

        #endregion

        #region 命令
        [NotMapped]
        public DelegateCommand EditCommand { get; private set; }
        [NotMapped]
        public DelegateCommand DeleteCommand { get; private set; }

        #endregion

        #region 构造函数

        public RegisterMapping()
        {
            DataType = "未命名";
            Address = 0;
            Format = ModbusDataFormat.Float32;
            Unit = "";
            Factor = 1.0;
            Offset = 0.0;
            Status = "正常";
            MinValue = double.MinValue;
            MaxValue = double.MaxValue;
            IsEnabled = true;

            EditCommand = new DelegateCommand(OnEdit);
            DeleteCommand = new DelegateCommand(OnDelete);
        }

        #endregion

        #region 验证方法

        protected override string ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(DataType):
                    if (string.IsNullOrWhiteSpace(DataType))
                        return "数据类型不能为空";
                    break;

                case nameof(Factor):
                    if (Factor == 0)
                        return "系数不能为零";
                    break;

                case nameof(MinValue):
                case nameof(MaxValue):
                    if (MinValue >= MaxValue)
                        return "最小值必须小于最大值";
                    break;
            }
            return null;
        }

        #endregion

        #region 数据转换方法

        public double CalculateValue(double rawValue) => rawValue * Factor + Offset;
        public bool IsValueValid(double value) => value >= MinValue && value <= MaxValue;
        public string FormatValue(double value) => $"{value:F2} {Unit}";

        #endregion

        #region 命令处理

        private void OnEdit()
        {
            // 编辑逻辑由外部处理
        }

        private void OnDelete()
        {
            // 删除逻辑由外部处理
        }

        #endregion

        #region 重写方法

        public override string ToString() => FullDescription;

        #endregion
    }
}
