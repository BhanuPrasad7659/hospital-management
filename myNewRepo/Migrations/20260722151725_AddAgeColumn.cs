using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CogMediHospitalManagementSystem.Migrations
{
    public partial class AddAgeColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Admissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Admissions");
        }
    }
}
