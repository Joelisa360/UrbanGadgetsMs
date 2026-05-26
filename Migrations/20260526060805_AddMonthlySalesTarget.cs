using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanGadgetsMS.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlySalesTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalesTarget",
                table: "AppSettings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlySalesTarget",
                table: "AppSettings");
        }
    }
}
