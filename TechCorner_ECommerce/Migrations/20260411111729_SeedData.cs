using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechCorner_ECommerce.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Laptop" },
                    { 2, "PC" },
                    { 3, "Main - CPU - VGA" },
                    { 4, "Gaming Gear" }
                });

            migrationBuilder.InsertData(
                table: "SubCategories",
                columns: new[] { "Id", "CategoryId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Acer" },
                    { 2, 1, "Asus" },
                    { 3, 1, "MSI" },
                    { 4, 1, "Lenovo" },
                    { 5, 2, "Gaming PC" },
                    { 6, 2, "Workstation PC" },
                    { 7, 3, "Mainboard" },
                    { 8, 3, "CPU" },
                    { 9, 3, "VGA" },
                    { 10, 4, "Keyboard" },
                    { 11, 4, "Mouse" },
                    { 12, 4, "Headset" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock", "SubCategoryId" },
                values: new object[,]
                {
                    { 1, 1, "Gaming laptop RTX 3050", "acer_nitro5.jpg", "Acer Nitro 5", 1200m, 10, 1 },
                    { 2, 1, "High performance gaming laptop", "asus_rog_g15.jpg", "Asus ROG Strix G15", 1500m, 8, 2 },
                    { 3, 1, "Gaming laptop RTX 3060", "msi_katana.jpg", "MSI Katana GF66", 1400m, 6, 3 },
                    { 4, 2, "Custom build gaming PC", "pc_gaming.jpg", "Gaming PC RTX 4060", 2000m, 5, 5 },
                    { 5, 3, "13th Gen CPU", "cpu_i7.jpg", "Intel Core i7 13700K", 450m, 15, 8 },
                    { 6, 3, "NVIDIA Graphics Card", "rtx4070.jpg", "RTX 4070", 700m, 7, 9 },
                    { 7, 4, "Mechanical gaming keyboard", "keyboard_razer.jpg", "Razer BlackWidow", 150m, 20, 10 },
                    { 8, 4, "Gaming mouse", "mouse_logitech.jpg", "Logitech G Pro X", 120m, 25, 11 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
