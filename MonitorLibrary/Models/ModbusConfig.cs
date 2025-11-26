using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// Modbus协议配置（与数据点一对一，每个参数独立配置）
    /// </summary>
    public class ModbusConfig : BindableBase
    {
        #region Private Fields
        private int _id;
        private int _dataPointId;
        private byte _deviceAddress = 1;
        private ushort _registerStart;
        private int _registerLength = 4;
        private ModbusFunctionCode _functionCode = ModbusFunctionCode.ReadHoldingRegisters;
        private ModbusDataFormat _dataFormat = ModbusDataFormat.Float32;
        private decimal _dataMultiplier = 1.0m;
        private decimal _offset = 0m;
        private ModbusByteOrder _endianness = ModbusByteOrder.BigEndian;
        #endregion

        #region Public Properties
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
        /// 外键：关联的数据点ID
        /// </summary>
        [ForeignKey("DataPoint")]
        public int DataPointId
        {
            get => _dataPointId;
            set => SetProperty(ref _dataPointId, value);
        }

        /// <summary>
        /// Modbus从机地址（1-255）
        /// </summary>
        [Required(ErrorMessage = "从机地址不能为空")]
        [Range(1, 255, ErrorMessage = "从机地址必须在1到255之间")]
        public byte DeviceAddress
        {
            get => _deviceAddress;
            set => SetProperty(ref _deviceAddress, value);
        }

        /// <summary>
        /// 寄存器起始地址（如0x0000）
        /// </summary>
        [Required(ErrorMessage = "寄存器起始地址不能为空")]
        [Range(0, 65535, ErrorMessage = "寄存器地址必须在0到65535之间")]
        public ushort RegisterStart
        {
            get => _registerStart;
            set => SetProperty(ref _registerStart, value);
        }

        /// <summary>
        /// 寄存器长度（字节数，如4字节=2个16位寄存器）
        /// </summary>
        [Required(ErrorMessage = "寄存器长度不能为空")]
        [Range(1, 250, ErrorMessage = "寄存器长度必须在1到250之间")]
        public int RegisterLength
        {
            get => _registerLength;
            set
            {
                if (SetProperty(ref _registerLength, value))
                {
                    // 寄存器长度变化时，可能需要更新数据格式的有效性
                    ValidateRegisterLength();
                }
            }
        }

        /// <summary>
        /// 功能码（如0x03=读保持寄存器）
        /// </summary>
        [Required(ErrorMessage = "功能码不能为空")]
        public ModbusFunctionCode FunctionCode
        {
            get => _functionCode;
            set => SetProperty(ref _functionCode, value);
        }

        /// <summary>
        /// 数据格式（如Float32/Int16）
        /// </summary>
        [Required(ErrorMessage = "数据格式不能为空")]
        public ModbusDataFormat DataFormat
        {
            get => _dataFormat;
            set
            {
                if (SetProperty(ref _dataFormat, value))
                {
                    // 数据格式变化时，自动调整寄存器长度
                    AutoAdjustRegisterLength();
                }
            }
        }

        /// <summary>
        /// 数据倍率（如0.1=实际值=寄存器值×0.1）
        /// </summary>
        [Required(ErrorMessage = "数据倍率不能为空")]
        [Range(-999999, 999999, ErrorMessage = "数据倍率超出范围")]
        public decimal DataMultiplier
        {
            get => _dataMultiplier;
            set => SetProperty(ref _dataMultiplier, value);
        }

        /// <summary>
        /// 数据偏移（如10=实际值=寄存器值+10）
        /// </summary>
        [Required(ErrorMessage = "数据偏移不能为空")]
        [Range(-999999, 999999, ErrorMessage = "数据偏移超出范围")]
        public decimal Offset
        {
            get => _offset;
            set => SetProperty(ref _offset, value);
        }

        /// <summary>
        /// 字节序（BigEndian/LittleEndian）
        /// </summary>
        [Required(ErrorMessage = "字节序不能为空")]
        public ModbusByteOrder Endianness
        {
            get => _endianness;
            set => SetProperty(ref _endianness, value);
        }
        #endregion

        #region Calculated Properties
        /// <summary>
        /// 计算寄存器数量（16位寄存器的个数）
        /// </summary>
        [NotMapped]
        public int RegisterCount => (RegisterLength + 1) / 2;

        /// <summary>
        /// 获取格式化的地址显示（十六进制）
        /// </summary>
        [NotMapped]
        public string RegisterStartHex => $"0x{RegisterStart:X4}";

        /// <summary>
        /// 获取完整的配置描述
        /// </summary>
        [NotMapped]
        public string ConfigDescription =>
            $"从机:{DeviceAddress} 功能码:{(byte)FunctionCode:X2} 地址:{RegisterStartHex} 长度:{RegisterLength}字节 格式:{DataFormat}";
        #endregion

        #region Constructors
        public ModbusConfig()
        {
            // 设置默认值
            DeviceAddress = 1;
            RegisterStart = 0;
            RegisterLength = 4;
            FunctionCode = ModbusFunctionCode.ReadHoldingRegisters;
            DataFormat = ModbusDataFormat.Float32;
            DataMultiplier = 1.0m;
            Offset = 0m;
            Endianness = ModbusByteOrder.BigEndian;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 验证寄存器长度的合理性
        /// </summary>
        private void ValidateRegisterLength()
        {
            // 根据数据格式验证寄存器长度
            int requiredLength = GetRequiredRegisterLength(DataFormat);
            if (RegisterLength < requiredLength)
            {
                // 可以在这里触发验证错误或自动调整
                // RegisterLength = requiredLength;
            }
        }

        /// <summary>
        /// 根据数据格式自动调整寄存器长度
        /// </summary>
        private void AutoAdjustRegisterLength()
        {
            int requiredLength = GetRequiredRegisterLength(DataFormat);
            if (RegisterLength != requiredLength)
            {
                RegisterLength = requiredLength;
            }
        }

        /// <summary>
        /// 获取指定数据格式所需的寄存器长度（字节数）
        /// </summary>
        private int GetRequiredRegisterLength(ModbusDataFormat format)
        {
            return format switch
            {
                ModbusDataFormat.Int16 => 2,
                ModbusDataFormat.UInt16 => 2,
                ModbusDataFormat.Int32 => 4,
                ModbusDataFormat.UInt32 => 4,
                ModbusDataFormat.Float32 => 4,
                ModbusDataFormat.Float64 => 8,
                _ => 4 // 默认4字节
            };

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 验证配置的有效性
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            // 验证从机地址
            if (DeviceAddress < 1 || DeviceAddress > 255)
            {
                errorMessage = "从机地址必须在1到255之间";
                return false;
            }

            // 验证寄存器地址
            if (RegisterStart < 0 || RegisterStart > 65535)
            {
                errorMessage = "寄存器地址必须在0到65535之间";
                return false;
            }

            // 验证寄存器长度
            if (RegisterLength < 1 || RegisterLength > 250)
            {
                errorMessage = "寄存器长度必须在1到250字节之间";
                return false;
            }

            // 验证寄存器长度与数据格式的匹配
            int requiredLength = GetRequiredRegisterLength(DataFormat);
            if (RegisterLength < requiredLength)
            {
                errorMessage = $"当前数据格式 {DataFormat} 至少需要 {requiredLength} 字节";
                return false;
            }

            // 验证数据倍率不为0
            if (DataMultiplier == 0)
            {
                errorMessage = "数据倍率不能为0";
                return false;
            }

            // 验证寄存器地址范围
            if (RegisterStart + RegisterCount > 65536)
            {
                errorMessage = "寄存器地址范围超出限制（最大65535）";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 计算实际值（根据倍率和偏移）
        /// </summary>
        public decimal CalculateActualValue(decimal rawValue)
        {
            return (rawValue * DataMultiplier) + Offset;
        }

        /// <summary>
        /// 反向计算寄存器值（根据实际值）
        /// </summary>
        public decimal CalculateRegisterValue(decimal actualValue)
        {
            if (DataMultiplier == 0)
                throw new System.InvalidOperationException("数据倍率不能为0");

            return (actualValue - Offset) / DataMultiplier;
        }

        /// <summary>
        /// 克隆配置对象
        /// </summary>
        public ModbusConfig Clone()
        {
            return new ModbusConfig
            {
                Id = this.Id,
                DataPointId = this.DataPointId,
                DeviceAddress = this.DeviceAddress,
                RegisterStart = this.RegisterStart,
                RegisterLength = this.RegisterLength,
                FunctionCode = this.FunctionCode,
                DataFormat = this.DataFormat,
                DataMultiplier = this.DataMultiplier,
                Offset = this.Offset,
                Endianness = this.Endianness
            };
        }

        /// <summary>
        /// 从另一个配置复制值
        /// </summary>
        public void CopyFrom(ModbusConfig source)
        {
            if (source == null) return;

            this.DeviceAddress = source.DeviceAddress;
            this.RegisterStart = source.RegisterStart;
            this.RegisterLength = source.RegisterLength;
            this.FunctionCode = source.FunctionCode;
            this.DataFormat = source.DataFormat;
            this.DataMultiplier = source.DataMultiplier;
            this.Offset = source.Offset;
            this.Endianness = source.Endianness;
        }

        /// <summary>
        /// 重写ToString方法，方便调试
        /// </summary>
        public override string ToString()
        {
            return ConfigDescription;
        }

        /// <summary>
        /// 重置为默认值
        /// </summary>
        public void Reset()
        {
            DeviceAddress = 1;
            RegisterStart = 0;
            RegisterLength = 4;
            FunctionCode = ModbusFunctionCode.ReadHoldingRegisters;
            DataFormat = ModbusDataFormat.Float32;
            DataMultiplier = 1.0m;
            Offset = 0m;
            Endianness = ModbusByteOrder.BigEndian;
        }
        #endregion
    }
}