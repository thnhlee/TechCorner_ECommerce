using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechCorner_ECommerce.Migrations
{
    /// <inheritdoc />
    public partial class fixdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkuCode",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "ParentProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderCode",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ParentProducts_Slug_SubCategoryId",
                table: "ParentProducts",
                columns: new[] { "Slug", "SubCategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParentProducts_Slug_SubCategoryId",
                table: "ParentProducts");

            migrationBuilder.DropColumn(
                name: "SkuCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "ParentProducts");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Orders");
        }
    }
}
