using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CogMediHospitalManagementSystem.Migrations
{
    public partial class AddPriceToMedicineStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "MedicineStocks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "MedicineStocks");
        }
    }
}
