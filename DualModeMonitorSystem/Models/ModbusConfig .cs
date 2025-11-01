using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Models.Enums;

namespace DualModeMonitorSystem.Models
{
    #region Modbus协议配置模型

    /// <summary>
    /// Modbus协议配置模型
    /// </summary>
    public class ModbusConfig : ModelBase
    {
        #region 私有字段

        private ModbusProtocolType _protocolType;
        private byte _slaveAddress;
        private ModbusFunctionCode _functionCode;
        private ushort _startAddress;
        private ushort _registerCount;
        private ModbusDataFormat _dataFormat;
        private ModbusByteOrder _byteOrder;
        private bool _isEnabled;
        private int _timeout;
        private int _retryCount;
        private string _description;
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

        /// <summary>
        /// 协议类型 (RTU/TCP)
        /// </summary>
        public ModbusProtocolType ProtocolType
        {
            get => _protocolType;
            set => SetPropertyWithValidation(ref _protocolType, value);
        }

        /// <summary>
        /// 从站地址 (1-247)
        /// </summary>
        public byte SlaveAddress
        {
            get => _slaveAddress;
            set => SetPropertyWithValidation(ref _slaveAddress, value);
        }

        /// <summary>
        /// 功能码
        /// </summary>
        public ModbusFunctionCode FunctionCode
        {
            get => _functionCode;
            set => SetPropertyWithValidation(ref _functionCode, value);
        }

        /// <summary>
        /// 起始寄存器地址 (0-65535)
        /// </summary>
        public ushort StartAddress
        {
            get => _startAddress;
            set => SetPropertyWithValidation(ref _startAddress, value);
        }

        /// <summary>
        /// 寄存器数量 (1-125)
        /// </summary>
        public ushort RegisterCount
        {
            get => _registerCount;
            set => SetPropertyWithValidation(ref _registerCount, value);
        }

        /// <summary>
        /// 数据格式
        /// </summary>
        public ModbusDataFormat DataFormat
        {
            get => _dataFormat;
            set => SetPropertyWithValidation(ref _dataFormat, value);
        }

        /// <summary>
        /// 字节序
        /// </summary>
        public ModbusByteOrder ByteOrder
        {
            get => _byteOrder;
            set => SetPropertyWithValidation(ref _byteOrder, value);
        }

        /// <summary>
        /// 是否启用Modbus协议
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int Timeout
        {
            get => _timeout;
            set => SetPropertyWithValidation(ref _timeout, value);
        }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount
        {
            get => _retryCount;
            set => SetPropertyWithValidation(ref _retryCount, value);
        }

        /// <summary>
        /// 配置描述
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        #endregion

        #region 计算属性

        /// <summary>
        /// 结束地址
        /// </summary>
        [NotMapped]
        public ushort EndAddress => (ushort)(StartAddress + RegisterCount - 1);

        /// <summary>
        /// 配置摘要
        /// </summary>
        [NotMapped]
        public string Summary => $"{ProtocolType} - 从站{SlaveAddress} - 功能码{(byte)FunctionCode:X2} - 地址{StartAddress}~{EndAddress}";

        /// <summary>
        /// 是否为读操作
        /// </summary>
        [NotMapped]
        public bool IsReadOperation => FunctionCode == ModbusFunctionCode.ReadCoils ||
                                       FunctionCode == ModbusFunctionCode.ReadDiscreteInputs ||
                                       FunctionCode == ModbusFunctionCode.ReadHoldingRegisters ||
                                       FunctionCode == ModbusFunctionCode.ReadInputRegisters;

        /// <summary>
        /// 是否为写操作
        /// </summary>
        [NotMapped] 
        public bool IsWriteOperation => !IsReadOperation;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认配置
        /// </summary>
        public ModbusConfig()
        {
            ProtocolType = ModbusProtocolType.RTU;
            SlaveAddress = 1;
            FunctionCode = ModbusFunctionCode.ReadHoldingRegisters;
            StartAddress = 0;
            RegisterCount = 2;
            DataFormat = ModbusDataFormat.Float32;
            ByteOrder = ModbusByteOrder.BigEndian;
            IsEnabled = true;
            Timeout = 1000;
            RetryCount = 3;
            Description = "默认Modbus配置";

        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public ModbusConfig(byte slaveAddress, ushort startAddress, ushort registerCount)
            : this()
        {
            SlaveAddress = slaveAddress;
            StartAddress = startAddress;
            RegisterCount = registerCount;
        }

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证属性
        /// </summary>
        protected override string ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SlaveAddress):
                    if (SlaveAddress < 1 || SlaveAddress > 247)
                        return "从站地址必须在 1-247 之间";
                    break;

                case nameof(StartAddress):
                    if (StartAddress > 65535)
                        return "起始地址必须在 0-65535 之间";
                    break;

                case nameof(RegisterCount):
                    if (RegisterCount < 1)
                        return "寄存器数量必须大于 0";
                    if (RegisterCount > 125)
                        return "寄存器数量不能超过 125";
                    if (StartAddress + RegisterCount > 65536)
                        return "寄存器范围超出最大地址";
                    break;

                case nameof(Timeout):
                    if (Timeout < 100)
                        return "超时时间不能小于 100 毫秒";
                    if (Timeout > 60000)
                        return "超时时间不能超过 60 秒";
                    break;

                case nameof(RetryCount):
                    if (RetryCount < 0)
                        return "重试次数不能为负数";
                    if (RetryCount > 10)
                        return "重试次数不能超过 10 次";
                    break;
            }

            return null;
        }

        #endregion

        #region 公共方法

        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            ProtocolType = ModbusProtocolType.RTU;
            SlaveAddress = 1;
            FunctionCode = ModbusFunctionCode.ReadHoldingRegisters;
            StartAddress = 0;
            RegisterCount = 2;
            DataFormat = ModbusDataFormat.Float32;
            ByteOrder = ModbusByteOrder.BigEndian;
            IsEnabled = true;
            Timeout = 1000;
            RetryCount = 3;
            Description = "默认Modbus配置";

        }

        /// <summary>
        /// 导出配置为字符串
        /// </summary>
        public string ExportToString()
        {
            return $"{ProtocolType}|{SlaveAddress}|{(byte)FunctionCode}|{StartAddress}|{RegisterCount}|{DataFormat}|{ByteOrder}|{Timeout}|{RetryCount}";
        }

        /// <summary>
        /// 从字符串导入配置
        /// </summary>
        public static ModbusConfig ImportFromString(string configString)
        {
            if (string.IsNullOrWhiteSpace(configString))
                return null;

            try
            {
                var parts = configString.Split('|');
                if (parts.Length < 9)
                    return null;

                var config = new ModbusConfig
                {
                    ProtocolType = Enum.Parse<ModbusProtocolType>(parts[0]),
                    SlaveAddress = byte.Parse(parts[1]),
                    FunctionCode = (ModbusFunctionCode)byte.Parse(parts[2]),
                    StartAddress = ushort.Parse(parts[3]),
                    RegisterCount = ushort.Parse(parts[4]),
                    DataFormat = Enum.Parse<ModbusDataFormat>(parts[5]),
                    ByteOrder = Enum.Parse<ModbusByteOrder>(parts[6]),
                    Timeout = int.Parse(parts[7]),
                    RetryCount = int.Parse(parts[8])
                };

                return config;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 重写方法

        public override string ToString()
        {
            return Summary;
        }

        #endregion
    }

    #endregion
}
