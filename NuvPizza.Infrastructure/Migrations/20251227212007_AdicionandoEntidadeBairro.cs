using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoEntidadeBairro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Pedidos");

            migrationBuilder.AddColumn<int>(
                name: "BairroId",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BairroNome",
                table: "Pedidos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorFrete",
                table: "Pedidos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Bairros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ValorFrete = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bairros", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_BairroId",
                table: "Pedidos",
                column: "BairroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Bairros_BairroId",
                table: "Pedidos",
                column: "BairroId",
                principalTable: "Bairros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Bairros_BairroId",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "Bairros");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_BairroId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "BairroId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "BairroNome",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ValorFrete",
                table: "Pedidos");

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
