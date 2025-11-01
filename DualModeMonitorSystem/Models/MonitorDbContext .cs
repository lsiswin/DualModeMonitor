using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DualModeMonitorSystem.Models
{
    public class MonitorDbContext : DbContext
    {
        public DbSet<AlertInfo> Alerts { get; set; }
        public DbSet<DeviceInfo> Devices { get; set; }
        public DbSet<ModbusConfig> ModbusConfigs { get; set; }
        public DbSet<RegisterMapping> RegisterMappings { get; set; }
        public DbSet<SerialPortConfig> SerialPortConfigs { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //配置SQLite数据库连接字符串
            optionsBuilder.UseSqlite("Data Source=monitoring_system.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // AlertInfo
            modelBuilder.Entity<AlertInfo>(entity =>
            {
                entity.HasKey(e => e.AlertId);
                entity.Ignore(e => e.TimeText);
                entity.Ignore(e => e.LevelText);
                entity.Ignore(e => e.StatusText);
                entity.Ignore(e => e.ElapsedTime);
            });

            // DeviceInfo
            modelBuilder.Entity<DeviceInfo>(entity =>
            {
                entity.HasKey(e => e.DeviceId);
                entity.Ignore(e => e.TemperatureText);
                entity.Ignore(e => e.HumidityText);
                entity.Ignore(e => e.VoltageText);
                entity.Ignore(e => e.StatusText);
                entity.Ignore(e => e.LastUpdatedText);
            });

            // ModbusConfig
            modelBuilder.Entity<ModbusConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Ignore(e => e.EndAddress);
                entity.Ignore(e => e.Summary);
                entity.Ignore(e => e.IsReadOperation);
                entity.Ignore(e => e.IsWriteOperation);
            });

            // RegisterMapping
            modelBuilder.Entity<RegisterMapping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Ignore(e => e.AddressHex);
                entity.Ignore(e => e.FullDescription);
            });

            // SerialPortConfig
            modelBuilder.Entity<SerialPortConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Ignore(e => e.Summary);
                entity.Ignore(e => e.StatusText);
                entity.Ignore(e => e.IsConnected);
                entity.Ignore(e => e.LastCommunicationText);
                entity.Ignore(e => e.BytesSentText);
                entity.Ignore(e => e.BytesReceivedText);
            });

            // SystemConfig
            modelBuilder.Entity<SystemConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
