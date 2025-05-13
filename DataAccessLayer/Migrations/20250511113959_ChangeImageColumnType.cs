using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeImageColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, create a temporary column
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Cats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Cats");

            // Rename the new column to Image
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Cats",
                newName: "Image");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // First, create a temporary column
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                table: "Cats",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            // Drop the string column
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Cats");

            // Rename the bytes column to Image
            migrationBuilder.RenameColumn(
                name: "ImageBytes",
                table: "Cats",
                newName: "Image");
        }
    }
} 