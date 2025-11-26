using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitorApi.Migrations
{
    /// <inheritdoc />
    public partial class init6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisterLength",
                table: "ModbusConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegisterLength",
                table: "ModbusConfigs",
                type: "INTEGER",
                maxLength: 125,
                nullable: false,
                defaultValue: 4);
        }
    }
}
