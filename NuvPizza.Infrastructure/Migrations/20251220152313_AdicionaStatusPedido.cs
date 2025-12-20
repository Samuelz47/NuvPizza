using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaStatusPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusPedido",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusPedido",
                table: "Pedidos");
        }
    }
}
