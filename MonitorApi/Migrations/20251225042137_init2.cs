using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitorApi.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPoints_HumitureDevices_HumitureDevicesId",
                table: "DataPoints");

            migrationBuilder.DropColumn(
                name: "HumitureDevicesId",
                table: "DataPoints");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HumitureDevicesId",
                table: "DataPoints",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoints_HumitureDevices_HumitureDevicesId",
                table: "DataPoints",
                column: "HumitureDevicesId",
                principalTable: "HumitureDevices",
                principalColumn: "Id");
        }
    }
}
