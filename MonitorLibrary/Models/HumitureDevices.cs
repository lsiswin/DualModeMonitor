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
        private SerialPortConfig _serialPortConfig;
        private ObservableCollection<ModbusConfig> _modbusConfigs;
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

        /// <summary>
        /// 一对一关联：串口配置
        /// </summary>
        public SerialPortConfig SerialPortConfig
        {
            get => _serialPortConfig;
            set
            {
                // 取消旧对象的事件订阅
                if (_serialPortConfig != null)
                {
                    _serialPortConfig.PropertyChanged -= OnSerialPortConfigPropertyChanged;
                }

                if (SetProperty(ref _serialPortConfig, value))
                {
                    // 订阅新对象的属性变更事件
                    if (_serialPortConfig != null)
                    {
                        _serialPortConfig.PropertyChanged += OnSerialPortConfigPropertyChanged;
                    }
                }
            }
        }

        /// <summary>
        /// 一对多关联：Modbus配置集合
        /// </summary>
        public ObservableCollection<ModbusConfig> ModbusConfigs
        {
            get => _modbusConfigs ??= new ObservableCollection<ModbusConfig>();
            set
            {
                // 取消旧集合的事件订阅
                if (_modbusConfigs != null)
                {
                    _modbusConfigs.CollectionChanged -= OnModbusConfigsCollectionChanged;
                }

                if (SetProperty(ref _modbusConfigs, value))
                {
                    // 订阅新集合的变更事件
                    if (_modbusConfigs != null)
                    {
                        _modbusConfigs.CollectionChanged += OnModbusConfigsCollectionChanged;

                        // 订阅集合中每个元素的属性变更
                        foreach (var config in _modbusConfigs)
                        {
                            if (config != null)
                            {
                                config.PropertyChanged -= OnModbusConfigPropertyChanged;
                                config.PropertyChanged += OnModbusConfigPropertyChanged;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Constructors
        public HumitureDevices()
        {
            // 初始化默认值
            Status = DeviceStatus.Offline;
            ModbusConfigs = new ObservableCollection<ModbusConfig>();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 串口配置属性变更事件处理
        /// </summary>
        private void OnSerialPortConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 通知外部：SerialPortConfig 的某个属性发生了变化
            RaisePropertyChanged(nameof(SerialPortConfig));
        }

        /// <summary>
        /// Modbus配置集合变更事件处理
        /// </summary>
        private void OnModbusConfigsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 处理新增的项
            if (e.NewItems != null)
            {
                foreach (ModbusConfig item in e.NewItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= OnModbusConfigPropertyChanged;
                        item.PropertyChanged += OnModbusConfigPropertyChanged;
                    }
                }
            }

            // 处理移除的项
            if (e.OldItems != null)
            {
                foreach (ModbusConfig item in e.OldItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= OnModbusConfigPropertyChanged;
                    }
                }
            }

            // 通知集合发生了变化
            RaisePropertyChanged(nameof(ModbusConfigs));
        }

        /// <summary>
        /// Modbus配置项属性变更事件处理
        /// </summary>
        private void OnModbusConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 通知外部：ModbusConfigs 中某个配置的属性发生了变化
            RaisePropertyChanged(nameof(ModbusConfigs));
        }
        #endregion

        #region Methods
        /// <summary>
        /// 添加Modbus配置
        /// </summary>
        public void AddModbusConfig(ModbusConfig config)
        {
            if (config != null && !ModbusConfigs.Contains(config))
            {
                ModbusConfigs.Add(config);
            }
        }

        /// <summary>
        /// 移除Modbus配置
        /// </summary>
        public void RemoveModbusConfig(ModbusConfig config)
        {
            if (config != null && ModbusConfigs.Contains(config))
            {
                ModbusConfigs.Remove(config);
            }
        }

        /// <summary>
        /// 清空所有Modbus配置
        /// </summary>
        public void ClearModbusConfigs()
        {
            ModbusConfigs.Clear();
        }

        /// <summary>
        /// 克隆设备对象（用于编辑场景）
        /// </summary>
        public HumitureDevices Clone()
        {
            var cloned = new HumitureDevices
            {
                Id = this.Id,
                Name = this.Name,
                DeviceCode = this.DeviceCode,
                Location = this.Location,
                Status = this.Status,
                Remark = this.Remark
            };

            // 深拷贝串口配置
            if (this.SerialPortConfig != null)
            {
                cloned.SerialPortConfig = new SerialPortConfig
                {
                    Id = this.SerialPortConfig.Id,
                    DeviceId = this.SerialPortConfig.DeviceId,
                    PortName = this.SerialPortConfig.PortName,
                    BaudRate = this.SerialPortConfig.BaudRate,
                    DataBits = this.SerialPortConfig.DataBits,
                    StopBits = this.SerialPortConfig.StopBits,
                    Parity = this.SerialPortConfig.Parity,
                    Timeout = this.SerialPortConfig.Timeout
                };
            }

            // 深拷贝Modbus配置集合
            foreach (var config in this.ModbusConfigs)
            {
                // 这里需要根据 ModbusConfig 的实际结构进行克隆
                cloned.ModbusConfigs.Add(config);
            }

            return cloned;
        }

        /// <summary>
        /// 验证设备数据的完整性
        /// </summary>
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("设备名称不能为空");

            if (string.IsNullOrWhiteSpace(DeviceCode))
                errors.Add("设备编号不能为空");

            if (SerialPortConfig == null)
                errors.Add("串口配置不能为空");
            else if (string.IsNullOrWhiteSpace(SerialPortConfig.PortName))
                errors.Add("串口名称不能为空");

            return errors.Count == 0;
        }

        /// <summary>
        /// 重写ToString方法，方便调试
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({DeviceCode}) - {Status}";
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// 清理资源和事件订阅
        /// </summary>
        public void Dispose()
        {
            if (SerialPortConfig != null)
            {
                SerialPortConfig.PropertyChanged -= OnSerialPortConfigPropertyChanged;
            }

            if (ModbusConfigs != null)
            {
                ModbusConfigs.CollectionChanged -= OnModbusConfigsCollectionChanged;

                foreach (var config in ModbusConfigs)
                {
                    if (config != null)
                    {
                        config.PropertyChanged -= OnModbusConfigPropertyChanged;
                    }
                }
            }
        }
        #endregion
    }
}