using Microsoft.EntityFrameworkCore.Migrations;

namespace NetCore_Model_View_Cortol_Created.Migrations
{
    public partial class SeedEmployeesData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "id", "Department", "email", "name" },
                values: new object[] { 1, 1, "Shenoda@gmail.com", "Shenoda" });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "id", "Department", "email", "name" },
                values: new object[] { 2, 2, "Mena@gmail.com", "Mena" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
