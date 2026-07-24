using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CogMediHospitalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Admissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Admissions");
        }
    }
}
