using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriaConfiguracoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EstaAberta = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataHoraFechamentoAtual = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HorarioFechamentoPadrao = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Configuracoes",
                columns: new[] { "Id", "DataHoraFechamentoAtual", "EstaAberta", "HorarioFechamentoPadrao" },
                values: new object[] { 1, null, false, new TimeSpan(0, 23, 0, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracoes");
        }
    }
}
