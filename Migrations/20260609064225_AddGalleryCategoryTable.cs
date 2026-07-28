using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotographyCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "GalleryItems");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "GalleryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GalleryCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GalleryCategories",
                columns: new[] { "Id", "Description", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Discover standout work that has earned recognition and acclaim.", "Award-winning Gallery", "award-winning" },
                    { 2, "Explore work that has been recognized across international platforms.", "Internationally Qualified", "internationally-qualified" },
                    { 3, "View the latest projects and the newest additions to the portfolio.", "Recent Work", "recent-work" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_CategoryId",
                table: "GalleryItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryItems_GalleryCategories_CategoryId",
                table: "GalleryItems",
                column: "CategoryId",
                principalTable: "GalleryCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GalleryItems_GalleryCategories_CategoryId",
                table: "GalleryItems");

            migrationBuilder.DropTable(
                name: "GalleryCategories");

            migrationBuilder.DropIndex(
                name: "IX_GalleryItems_CategoryId",
                table: "GalleryItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "GalleryItems");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "GalleryItems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
