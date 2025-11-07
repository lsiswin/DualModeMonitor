using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitorLibrary.Models;

namespace MonitorApi.Services
{
    /// <summary>
    /// 传感器配置
    /// </summary>
    public class HumitureConfiguration : IEntityTypeConfiguration<HumitureDevices>
    {
        public void Configure(EntityTypeBuilder<HumitureDevices> builder)
        {
            // 主键配置
            builder.HasKey(hd => hd.Id);

            // 唯一索引
            builder.HasIndex(hd => hd.DeviceCode).IsUnique();

            // 状态字段约束
            builder.Property(hd => hd.Status)
                   .HasConversion<string>()
                   .IsRequired();


            // 关系配置：传感器 -> 数据点（一对多）
            builder.HasMany<DataPoint>()
                   .WithOne(dp => dp.HumitureDevices)
                   .HasForeignKey(dp => dp.DeviceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    /// <summary>
    /// 数据点实体配置
    /// </summary>
    public class DataPointConfiguration : IEntityTypeConfiguration<DataPoint>
    {
        public void Configure(EntityTypeBuilder<DataPoint> builder)
        {
            // 主键配置
            builder.HasKey(dp => dp.Id);

            // 索引配置
            builder.HasIndex(dp => dp.Code).IsUnique();

            // 关系配置：数据点 -> 传感器（多对一）
            builder.HasOne(dp => dp.HumitureDevices)
                   .WithMany()
                   .HasForeignKey(dp => dp.DeviceId)
                   .OnDelete(DeleteBehavior.Cascade);


            // 关系配置：数据点 -> 记录（一对多）
            builder.HasMany(dp => dp.Records)
                   .WithOne(dpr => dpr.DataPoint)
                   .HasForeignKey(dpr => dpr.DataPointId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    /// <summary>
    /// 数据点记录实体配置
    /// </summary>
    public class DataPointRecordConfiguration : IEntityTypeConfiguration<DataPointRecord>
    {
        public void Configure(EntityTypeBuilder<DataPointRecord> builder)
        {
            // 主键配置
            builder.HasKey(dpr => dpr.Id);

            

            // 过滤索引：只索引有效数据
            builder.HasIndex(dpr => dpr.IsValid)
                   .HasFilter("[IsValid] = 1");
        }
    }

    /// <summary>
    /// Modbus配置实体配置（修正版）
    /// </summary>
    public class ModbusConfigConfiguration : IEntityTypeConfiguration<ModbusConfig>
    {
        public void Configure(EntityTypeBuilder<ModbusConfig> builder)
        {
            // 主键配置
            builder.HasKey(mc => mc.Id);

            // 枚举类型映射（使用字符串存储，便于数据库查看和维护）
            builder.Property(mc => mc.FunctionCode)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(mc => mc.DataFormat)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(mc => mc.Endianness)
                   .HasConversion<string>()
                   .IsRequired(); // 补充必填约束，确保字节序有值

            // 寄存器长度约束（合理范围：1-125字节，符合Modbus协议规范）
            builder.Property(mc => mc.RegisterLength)
                   .IsRequired()
                   .HasDefaultValue(4)
                   .HasMaxLength(125);

            // 数据倍率约束（避免极端值）
            builder.Property(mc => mc.DataMultiplier)
                   .IsRequired()
                   .HasPrecision(10, 4); // 精度：10位有效数字，4位小数


            
        }
    }

    /// <summary>
    /// 串口配置实体配置（补充完整）
    /// </summary>
    public class SerialPortConfigConfiguration : IEntityTypeConfiguration<SerialPortConfig>
    {
        public void Configure(EntityTypeBuilder<SerialPortConfig> builder)
        {
            // 主键配置
            builder.HasKey(sp => sp.Id);

            // 串口号唯一索引
            builder.HasIndex(sp => sp.PortName).IsUnique();

            // 波特率约束
            builder.Property(sp => sp.BaudRate)
                   .HasConversion<string>();

            // 数据位约束
            builder.Property(sp => sp.DataBits)
                   .HasConversion<string>();

            // 停止位约束
            builder.Property(sp => sp.StopBits)
                   .HasConversion<string>();

            // 校验位约束
            builder.Property(sp => sp.Parity)
                   .HasConversion<string>();
        }
    }

}
