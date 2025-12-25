using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;

namespace MonitorApi.Services
{
    public class ApplicationDbContext : DbContext
    {
        // 数据库表集合
        public DbSet<DataPoint> DataPoints { get; set; }
        public DbSet<DataPointRecord> DataPointRecords { get; set; }
        public DbSet<HumitureDevices> HumitureDevices { get; set; }
        public DbSet<SerialPortConfig> SerialPortConfigs { get; set; }
        public DbSet<ModbusConfig> ModbusConfigs { get; set; }

        // 构造函数：接收数据库配置
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information); // 将 SQL 打印到控制台
        }

        // 应用配置
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 应用所有实体配置
            modelBuilder.ApplyConfiguration(new DataPointConfiguration());
            modelBuilder.ApplyConfiguration(new DataPointRecordConfiguration());
            modelBuilder.ApplyConfiguration(new HumitureConfiguration());
            modelBuilder.ApplyConfiguration(new ModbusConfigConfiguration());
            modelBuilder.ApplyConfiguration(new SerialPortConfigConfiguration());
        }
    }
}
