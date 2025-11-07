using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitorApi.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HumitureDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeviceCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Remark = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumitureDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CollectInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    UpperLimit = table.Column<decimal>(type: "TEXT", nullable: false),
                    LowerLimit = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValidMin = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValidMax = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataRetentionDays = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableAlarm = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlarmDelay = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPoints_HumitureDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "HumitureDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SerialPortConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    BaudRate = table.Column<string>(type: "TEXT", nullable: false),
                    DataBits = table.Column<string>(type: "TEXT", nullable: false),
                    StopBits = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    Parity = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Timeout = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialPortConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialPortConfigs_HumitureDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "HumitureDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataPointRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataPointId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    CollectTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsValid = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAlarm = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPointRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPointRecords_DataPoints_DataPointId",
                        column: x => x.DataPointId,
                        principalTable: "DataPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModbusConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataPointId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceAddress = table.Column<byte>(type: "INTEGER", nullable: false),
                    RegisterStart = table.Column<ushort>(type: "INTEGER", nullable: false),
                    RegisterLength = table.Column<int>(type: "INTEGER", maxLength: 125, nullable: false, defaultValue: 4),
                    FunctionCode = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    DataFormat = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataMultiplier = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Endianness = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HumitureDevicesId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModbusConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModbusConfigs_DataPoints_DataPointId",
                        column: x => x.DataPointId,
                        principalTable: "DataPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModbusConfigs_HumitureDevices_HumitureDevicesId",
                        column: x => x.HumitureDevicesId,
                        principalTable: "HumitureDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "HumitureDevices",
                columns: new[] { "Id", "DeviceCode", "Location", "Name", "Remark", "Status" },
                values: new object[] { 1, "SENSOR-001", "一号车间A区域", "车间温湿度传感器", "用于监测车间环境温湿度", "Normal" });

            migrationBuilder.InsertData(
                table: "DataPoints",
                columns: new[] { "Id", "AlarmDelay", "Code", "CollectInterval", "DataRetentionDays", "DeviceId", "EnableAlarm", "LowerLimit", "Name", "Unit", "UpperLimit", "ValidMax", "ValidMin" },
                values: new object[] { 2, 30, "Temp", 10, 30, 1, true, 10.0m, "温度", "℃", 35.0m, 80.0m, -20.0m });

            migrationBuilder.InsertData(
                table: "SerialPortConfigs",
                columns: new[] { "Id", "BaudRate", "DataBits", "DeviceId", "Parity", "PortName", "StopBits", "Timeout" },
                values: new object[] { 1, "B9600", "Eight", 1, "None", "COM3", "One", 1000 });

            migrationBuilder.InsertData(
                table: "ModbusConfigs",
                columns: new[] { "Id", "DataFormat", "DataMultiplier", "DataPointId", "DeviceAddress", "Endianness", "FunctionCode", "HumitureDevicesId", "RegisterLength", "RegisterStart" },
                values: new object[] { 1, "Float32", 1.0m, 2, (byte)1, "BigEndian", "ReadHoldingRegisters", null, 2, (ushort)0 });

            migrationBuilder.CreateIndex(
                name: "IX_DataPointRecords_DataPointId",
                table: "DataPointRecords",
                column: "DataPointId");

            migrationBuilder.CreateIndex(
                name: "IX_DataPointRecords_IsValid",
                table: "DataPointRecords",
                column: "IsValid",
                filter: "[IsValid] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DataPoints_Code",
                table: "DataPoints",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPoints_DeviceId",
                table: "DataPoints",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_HumitureDevices_DeviceCode",
                table: "HumitureDevices",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModbusConfigs_DataPointId",
                table: "ModbusConfigs",
                column: "DataPointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModbusConfigs_HumitureDevicesId",
                table: "ModbusConfigs",
                column: "HumitureDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialPortConfigs_DeviceId",
                table: "SerialPortConfigs",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialPortConfigs_PortName",
                table: "SerialPortConfigs",
                column: "PortName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataPointRecords");

            migrationBuilder.DropTable(
                name: "ModbusConfigs");

            migrationBuilder.DropTable(
                name: "SerialPortConfigs");

            migrationBuilder.DropTable(
                name: "DataPoints");

            migrationBuilder.DropTable(
                name: "HumitureDevices");
        }
    }
}
