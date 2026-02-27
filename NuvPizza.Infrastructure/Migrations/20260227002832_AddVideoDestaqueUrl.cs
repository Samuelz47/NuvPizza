using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoDestaqueUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoDestaqueUrl",
                table: "Configuracoes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Configuracoes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VideoDestaqueUrl",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoDestaqueUrl",
                table: "Configuracoes");
        }
    }
}
