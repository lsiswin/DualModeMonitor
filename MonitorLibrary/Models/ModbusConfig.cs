using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonitorLibrary.Models.Enums;

public class ModbusConfig : BindableBase
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("DataPoint")]
    public int DataPointId { get; set; }

    // --- Modbus基础参数 ---

    /// <summary>
    /// 寄存器起始地址
    /// </summary>
    public ushort RegisterStart { get; set; }

    /// <summary>
    /// Modbus功能码
    /// </summary>
    public ModbusFunctionCode FunctionCode { get; set; } = ModbusFunctionCode.ReadHoldingRegisters;

    /// <summary>数据格式会自动决定寄存器长度</summary>
    public ModbusDataFormat DataFormat { get; set; } = ModbusDataFormat.Float32;

    /// <summary>倍率 × 偏移</summary>
    public decimal DataMultiplier { get; set; } = 1.0m;
    public decimal Offset { get; set; } = 0;

    /// <summary>BigEndian / LittleEndian</summary>
    public ModbusByteOrder Endianness { get; set; } = ModbusByteOrder.BigEndian;

    // --- 计算属性 ---
    [NotMapped]
    public int RegisterLength =>
        DataFormat switch
        {
            ModbusDataFormat.Int16 => 2,
            ModbusDataFormat.UInt16 => 2,
            ModbusDataFormat.Int32 => 4,
            ModbusDataFormat.UInt32 => 4,
            ModbusDataFormat.Float32 => 4,
            ModbusDataFormat.Float64 => 8,
            _ => 4,
        };

    [NotMapped]
    public int RegisterCount => RegisterLength / 2;

    [NotMapped]
    public string RegisterStartHex => $"0x{RegisterStart:X4}";

    [NotMapped]
    public string Description => $"功能:{(byte)FunctionCode:X2} 地址:{RegisterStartHex}";

    // --- 通用方法 ---
    public decimal ToActual(decimal raw) => raw * DataMultiplier + Offset;

    public decimal ToRaw(decimal actual) => (actual - Offset) / DataMultiplier;
}
