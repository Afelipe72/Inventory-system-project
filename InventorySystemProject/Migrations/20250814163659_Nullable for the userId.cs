using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystemProject.Migrations
{
    /// <inheritdoc />
    public partial class NullablefortheuserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemRequests_AspNetUsers_ApplicationUserId",
                table: "ItemRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ItemRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemRequests_AspNetUsers_ApplicationUserId",
                table: "ItemRequests",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemRequests_AspNetUsers_ApplicationUserId",
                table: "ItemRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ItemRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemRequests_AspNetUsers_ApplicationUserId",
                table: "ItemRequests",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
