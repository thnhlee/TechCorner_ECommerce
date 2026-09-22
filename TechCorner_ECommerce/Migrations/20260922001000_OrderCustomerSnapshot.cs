using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechCorner_ECommerce.Migrations
{
    /// <inheritdoc />
    public partial class OrderCustomerSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiverEmail",
                table: "Orders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverPhone",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE o
                SET
                    o.ReceiverName = COALESCE(NULLIF(a.ReceiverName, N''), u.FullName, N''),
                    o.ReceiverEmail = COALESCE(NULLIF(u.Email, N''), N''),
                    o.ReceiverPhone = COALESCE(NULLIF(a.Phone, N''), u.PhoneNumber, N''),
                    o.ShippingAddress = COALESCE(NULLIF(a.FullAddress, N''), N'')
                FROM Orders o
                LEFT JOIN Addresses a ON o.AddressId = a.Id
                LEFT JOIN AspNetUsers u ON o.UserId = u.Id
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiverPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");
        }
    }
}
