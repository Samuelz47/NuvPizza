using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PopulaBairros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Bairros",
                columns: new[] { "Id", "Ativo", "Nome", "ValorFrete" },
                values: new object[,]
                {
                    { 1, true, "Guarapes", 10.00m },
                    { 2, true, "Lagoa Nova", 12.00m },
                    { 3, true, "Lagoa Seca", 12.00m },
                    { 4, true, "Nossa Senhora de Nazaré", 6.00m },
                    { 5, true, "Neópolis", 15.00m },
                    { 6, true, "Nordeste", 7.00m },
                    { 7, true, "Nova Descoberta", 15.00m },
                    { 8, true, "Nova Parnamirim", 20.00m },
                    { 9, true, "Pitimbu", 15.00m },
                    { 10, true, "Alecrim", 10.00m },
                    { 11, true, "Barro Vermelho", 15.00m },
                    { 12, true, "Bom Pastor", 4.00m },
                    { 13, true, "Candelária", 10.00m },
                    { 14, true, "Capim Macio", 18.00m },
                    { 15, true, "Cidade Nova", 4.00m },
                    { 16, true, "Dix-Sept Rosado", 8.00m },
                    { 17, true, "Cidade da Esperança", 6.00m },
                    { 18, true, "Felipe Camarão", 0.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Bairros",
                keyColumn: "Id",
                keyValue: 18);
        }
    }
}
