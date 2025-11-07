using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MonitorApi.Migrations
{
    /// <inheritdoc />
    public partial class lii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialPortConfigs_HumitureDevices_DeviceId",
                table: "SerialPortConfigs");

            migrationBuilder.DropIndex(
                name: "IX_SerialPortConfigs_DeviceId",
                table: "SerialPortConfigs");

            migrationBuilder.DeleteData(
                table: "ModbusConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ModbusConfigs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ModbusConfigs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SerialPortConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SerialPortConfigs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DataPoints",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DataPoints",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "HumitureDevices",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "HumitureDevices",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "SerialPortConfigId",
                table: "HumitureDevices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HumitureDevices_SerialPortConfigId",
                table: "HumitureDevices",
                column: "SerialPortConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_HumitureDevices_SerialPortConfigs_SerialPortConfigId",
                table: "HumitureDevices",
                column: "SerialPortConfigId",
                principalTable: "SerialPortConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumitureDevices_SerialPortConfigs_SerialPortConfigId",
                table: "HumitureDevices");

            migrationBuilder.DropIndex(
                name: "IX_HumitureDevices_SerialPortConfigId",
                table: "HumitureDevices");

            migrationBuilder.DropColumn(
                name: "SerialPortConfigId",
                table: "HumitureDevices");

            migrationBuilder.InsertData(
                table: "HumitureDevices",
                columns: new[] { "Id", "DeviceCode", "Location", "Name", "Remark", "Status" },
                values: new object[,]
                {
                    { 1, "SENSOR-001", "一号车间A区域", "车间温湿度传感器", "用于监测车间环境温湿度", "Normal" },
                    { 2, "SENSOR-002", "一号车间A区域", "车间温湿度传感器", "用于监测车间环境温湿度", "Normal" }
                });

            migrationBuilder.InsertData(
                table: "DataPoints",
                columns: new[] { "Id", "AlarmDelay", "Code", "CollectInterval", "DataRetentionDays", "DeviceId", "EnableAlarm", "LowerLimit", "Name", "Unit", "UpperLimit", "ValidMax", "ValidMin" },
                values: new object[,]
                {
                    { 2, 30, "Temp", 10, 30, 1, true, 10.0m, "温度", "℃", 35.0m, 80.0m, -20.0m },
                    { 3, 30, "Temp", 10, 30, 2, true, 10.0m, "温度", "℃", 35.0m, 80.0m, -20.0m }
                });

            migrationBuilder.InsertData(
                table: "SerialPortConfigs",
                columns: new[] { "Id", "BaudRate", "DataBits", "DeviceId", "Parity", "PortName", "StopBits", "Timeout" },
                values: new object[,]
                {
                    { 1, "B9600", "Eight", 1, "None", "COM3", "One", 1000 },
                    { 2, "B9600", "Eight", 2, "None", "COM2", "One", 1000 }
                });

            migrationBuilder.InsertData(
                table: "ModbusConfigs",
                columns: new[] { "Id", "DataFormat", "DataMultiplier", "DataPointId", "DeviceAddress", "Endianness", "FunctionCode", "HumitureDevicesId", "RegisterLength", "RegisterStart" },
                values: new object[,]
                {
                    { 1, "Float32", 1.0m, 2, (byte)1, "BigEndian", "ReadHoldingRegisters", null, 2, (ushort)0 },
                    { 2, "Float32", 1.0m, 2, (byte)1, "BigEndian", "ReadHoldingRegisters", null, 2, (ushort)0 },
                    { 3, "Float32", 1.0m, 3, (byte)1, "BigEndian", "ReadHoldingRegisters", null, 2, (ushort)0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialPortConfigs_DeviceId",
                table: "SerialPortConfigs",
                column: "DeviceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SerialPortConfigs_HumitureDevices_DeviceId",
                table: "SerialPortConfigs",
                column: "DeviceId",
                principalTable: "HumitureDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
