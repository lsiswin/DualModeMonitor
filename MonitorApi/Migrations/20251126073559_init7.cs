using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitorApi.Migrations
{
    /// <inheritdoc />
    public partial class init7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModbusConfigs_HumitureDevices_HumitureDevicesId",
                table: "ModbusConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ModbusConfigs_HumitureDevicesId",
                table: "ModbusConfigs");

            migrationBuilder.DropColumn(
                name: "HumitureDevicesId",
                table: "ModbusConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HumitureDevicesId",
                table: "ModbusConfigs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModbusConfigs_HumitureDevicesId",
                table: "ModbusConfigs",
                column: "HumitureDevicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModbusConfigs_HumitureDevices_HumitureDevicesId",
                table: "ModbusConfigs",
                column: "HumitureDevicesId",
                principalTable: "HumitureDevices",
                principalColumn: "Id");
        }
    }
}
