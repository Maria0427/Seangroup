using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seangroup.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class RewFiix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_UserName",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Reviews",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_UserName",
                table: "Reviews",
                columns: new[] { "ProductId", "UserName" },
                unique: true);
        }
    }
}
