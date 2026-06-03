using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanGadgetsMS.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Remove old global uniqueness
            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryName",
                table: "Categories");

            // 2. Add correct per-business uniqueness
            migrationBuilder.CreateIndex(
                name: "IX_Categories_BusinessId_CategoryName",
                table: "Categories",
                columns: new[] { "BusinessId", "CategoryName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // reverse: remove composite index
            migrationBuilder.DropIndex(
                name: "IX_Categories_BusinessId_CategoryName",
                table: "Categories");

            // restore old (wrong) global constraint
            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryName",
                table: "Categories",
                column: "CategoryName",
                unique: true);
        }
    }
}
