using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMotoboyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MotoboyId",
                table: "Pedidos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Motoboys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motoboys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_MotoboyId",
                table: "Pedidos",
                column: "MotoboyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Motoboys_MotoboyId",
                table: "Pedidos",
                column: "MotoboyId",
                principalTable: "Motoboys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Motoboys_MotoboyId",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "Motoboys");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_MotoboyId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "MotoboyId",
                table: "Pedidos");
        }
    }
}
