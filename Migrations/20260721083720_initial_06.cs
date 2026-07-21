using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CogMediHospitalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class initial_06 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BedNumber",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BedNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Patients");
        }
    }
}
