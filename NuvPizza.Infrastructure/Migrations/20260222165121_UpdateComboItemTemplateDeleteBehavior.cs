using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuvPizza.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComboItemTemplateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscolhasCombo_ComboTemplates_ComboItemTemplateId",
                table: "EscolhasCombo");

            migrationBuilder.AlterColumn<int>(
                name: "ComboItemTemplateId",
                table: "EscolhasCombo",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_EscolhasCombo_ComboTemplates_ComboItemTemplateId",
                table: "EscolhasCombo",
                column: "ComboItemTemplateId",
                principalTable: "ComboTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscolhasCombo_ComboTemplates_ComboItemTemplateId",
                table: "EscolhasCombo");

            migrationBuilder.AlterColumn<int>(
                name: "ComboItemTemplateId",
                table: "EscolhasCombo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EscolhasCombo_ComboTemplates_ComboItemTemplateId",
                table: "EscolhasCombo",
                column: "ComboItemTemplateId",
                principalTable: "ComboTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
