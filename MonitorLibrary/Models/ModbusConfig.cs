using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models
{
    /// <summary>
    /// Modbus协议配置（与数据点一对一，每个参数独立配置）
    /// </summary>
    public class ModbusConfig: BindableBase
    {
        [Key]
        public int Id { get; set; }



        /// <summary>
        /// 外键：关联的数据点ID
        /// </summary>
        [ForeignKey("DataPoint")]
        public int DataPointId { get; set; }

        /// <summary>
        /// Modbus从机地址（1-255）
        /// </summary>
        [Required, Range(1, 255)]
        public byte DeviceAddress { get; set; }

        /// <summary>
        /// 寄存器起始地址（如0x0000）
        /// </summary>
        [Required]
        public ushort RegisterStart { get; set; }

        /// <summary>
        /// 寄存器长度（字节数，如4字节=2个16位寄存器）
        /// </summary>
        [Required]
        public int RegisterLength { get; set; } = 4;

        /// <summary>
        /// 功能码（如0x03=读保持寄存器）
        /// </summary>
        [Required, MaxLength(5)]
        public ModbusFunctionCode FunctionCode { get; set; } = ModbusFunctionCode.ReadHoldingRegisters;

        /// <summary>
        /// 数据格式（如Float32/Int16）
        /// </summary>
        [Required, MaxLength(20)]
        public ModbusDataFormat DataFormat { get; set; } = ModbusDataFormat.Float32;

        /// <summary>
        /// 数据倍率（如0.1=实际值=寄存器值×0.1）
        /// </summary>
        [Required]
        public decimal DataMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// 数据偏移（如10=实际值=寄存器值+10）
        /// </summary>
        [Required]
        public decimal Offset { get; set; } = 0m;

        /// <summary>
        /// 字节序（BigEndian/LittleEndian）
        /// </summary>
        [MaxLength(20)]
        public ModbusByteOrder Endianness { get; set; } = ModbusByteOrder.BigEndian;


    }
}