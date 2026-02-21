using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteCombosCustomizaveis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComboTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaPermitida = table.Column<int>(type: "INTEGER", nullable: false),
                    TamanhoObrigatorio = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboTemplates_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscolhasCombo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemPedidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComboItemTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoEscolhidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoSecundarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    BordaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscolhasCombo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscolhasCombo_ComboTemplates_ComboItemTemplateId",
                        column: x => x.ComboItemTemplateId,
                        principalTable: "ComboTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscolhasCombo_Items_ItemPedidoId",
                        column: x => x.ItemPedidoId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscolhasCombo_Produtos_BordaId",
                        column: x => x.BordaId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscolhasCombo_Produtos_ProdutoEscolhidoId",
                        column: x => x.ProdutoEscolhidoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscolhasCombo_Produtos_ProdutoSecundarioId",
                        column: x => x.ProdutoSecundarioId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboTemplates_ProdutoId",
                table: "ComboTemplates",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_EscolhasCombo_BordaId",
                table: "EscolhasCombo",
                column: "BordaId");

            migrationBuilder.CreateIndex(
                name: "IX_EscolhasCombo_ComboItemTemplateId",
                table: "EscolhasCombo",
                column: "ComboItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EscolhasCombo_ItemPedidoId",
                table: "EscolhasCombo",
                column: "ItemPedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_EscolhasCombo_ProdutoEscolhidoId",
                table: "EscolhasCombo",
                column: "ProdutoEscolhidoId");

            migrationBuilder.CreateIndex(
                name: "IX_EscolhasCombo_ProdutoSecundarioId",
                table: "EscolhasCombo",
                column: "ProdutoSecundarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscolhasCombo");

            migrationBuilder.DropTable(
                name: "ComboTemplates");
        }
    }
}
