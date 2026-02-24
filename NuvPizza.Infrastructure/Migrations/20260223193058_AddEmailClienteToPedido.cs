using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailClienteToPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailCliente",
                table: "Pedidos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCliente",
                table: "Pedidos");
        }
    }
}
