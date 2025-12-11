using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteEnderecoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnderecoEntrega",
                table: "Pedidos");

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Numero",
                table: "Pedidos",
                type: "INTEGER",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Pedidos");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
