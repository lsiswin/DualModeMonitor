using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitorApi.Migrations
{
    /// <inheritdoc />
    public partial class init22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceAddress",
                table: "ModbusConfigs");

            migrationBuilder.AddColumn<byte>(
                name: "DeviceAddress",
                table: "SerialPortConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceAddress",
                table: "SerialPortConfigs");

            migrationBuilder.AddColumn<byte>(
                name: "DeviceAddress",
                table: "ModbusConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
