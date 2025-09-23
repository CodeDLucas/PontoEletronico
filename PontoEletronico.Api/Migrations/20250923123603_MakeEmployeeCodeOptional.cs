using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontoEletronico.Migrations
{
    public partial class MakeEmployeeCodeOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the existing unique index
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmployeeCode",
                table: "AspNetUsers");

            // Alter column to allow NULL
            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            // Create new unique index with filter for non-null values
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmployeeCode",
                table: "AspNetUsers",
                column: "EmployeeCode",
                unique: true,
                filter: "[EmployeeCode] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the filtered unique index
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmployeeCode",
                table: "AspNetUsers");

            // Alter column to be required again
            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            // Create the original unique index
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmployeeCode",
                table: "AspNetUsers",
                column: "EmployeeCode",
                unique: true);
        }
    }
}
