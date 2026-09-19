using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Product.Migrations
{
    /// <inheritdoc />
    public partial class updateColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookEntityGenreEntity_Genre_GenreId",
                table: "BookEntityGenreEntity");

            migrationBuilder.RenameColumn(
                name: "GenreId",
                table: "BookEntityGenreEntity",
                newName: "GenresId");

            migrationBuilder.RenameIndex(
                name: "IX_BookEntityGenreEntity_GenreId",
                table: "BookEntityGenreEntity",
                newName: "IX_BookEntityGenreEntity_GenresId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookEntityGenreEntity_Genre_GenresId",
                table: "BookEntityGenreEntity",
                column: "GenresId",
                principalTable: "Genre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookEntityGenreEntity_Genre_GenresId",
                table: "BookEntityGenreEntity");

            migrationBuilder.RenameColumn(
                name: "GenresId",
                table: "BookEntityGenreEntity",
                newName: "GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_BookEntityGenreEntity_GenresId",
                table: "BookEntityGenreEntity",
                newName: "IX_BookEntityGenreEntity_GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookEntityGenreEntity_Genre_GenreId",
                table: "BookEntityGenreEntity",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
