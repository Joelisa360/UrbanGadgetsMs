using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UrbanGadgetsMS.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Targets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "StockMovements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Sales",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "SaleItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "RestockReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "RestockItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Expenses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Categories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "AppSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessName = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    OtherName = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    BusinessName = table.Column<string>(type: "text", nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRegistrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_BusinessId",
                table: "Users",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Targets_BusinessId",
                table: "Targets",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BusinessId",
                table: "StockMovements",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BusinessId",
                table: "Sales",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_BusinessId",
                table: "SaleItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RestockReports_BusinessId",
                table: "RestockReports",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RestockItems_BusinessId",
                table: "RestockItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessId",
                table: "Products",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessId",
                table: "Expenses",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BusinessId",
                table: "Categories",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_BusinessId",
                table: "AppSettings",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSettings_Businesses_BusinessId",
                table: "AppSettings",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Businesses_BusinessId",
                table: "Categories",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Businesses_BusinessId",
                table: "Expenses",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Businesses_BusinessId",
                table: "Products",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestockItems_Businesses_BusinessId",
                table: "RestockItems",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestockReports_Businesses_BusinessId",
                table: "RestockReports",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Businesses_BusinessId",
                table: "SaleItems",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Businesses_BusinessId",
                table: "Sales",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Businesses_BusinessId",
                table: "StockMovements",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Targets_Businesses_BusinessId",
                table: "Targets",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Businesses_BusinessId",
                table: "Users",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSettings_Businesses_BusinessId",
                table: "AppSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Businesses_BusinessId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Businesses_BusinessId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Businesses_BusinessId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_RestockItems_Businesses_BusinessId",
                table: "RestockItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RestockReports_Businesses_BusinessId",
                table: "RestockReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Businesses_BusinessId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Businesses_BusinessId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Businesses_BusinessId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Targets_Businesses_BusinessId",
                table: "Targets");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Businesses_BusinessId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropTable(
                name: "PendingRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Users_BusinessId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Targets_BusinessId",
                table: "Targets");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_BusinessId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Sales_BusinessId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_BusinessId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_RestockReports_BusinessId",
                table: "RestockReports");

            migrationBuilder.DropIndex(
                name: "IX_RestockItems_BusinessId",
                table: "RestockItems");

            migrationBuilder.DropIndex(
                name: "IX_Products_BusinessId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BusinessId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BusinessId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_AppSettings_BusinessId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Targets");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "RestockReports");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "RestockItems");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "AppSettings");
        }
    }
}
