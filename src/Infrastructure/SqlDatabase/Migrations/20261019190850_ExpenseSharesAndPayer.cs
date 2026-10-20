using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SqlDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ExpenseSharesAndPayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Beta-only reset approved for package 1.3; pre-existing paid splits cannot be mapped to shares or repayments.
            migrationBuilder.Sql("DELETE FROM expenses;");
            migrationBuilder.Sql("DELETE FROM `groups`;");
            migrationBuilder.Sql("DELETE FROM users;");

            migrationBuilder.DropTable(
                name: "splits");

            migrationBuilder.DropColumn(
                name: "settled",
                table: "expenses");

            migrationBuilder.AddColumn<bool>(
                name: "voided",
                table: "expenses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "payer_id",
                table: "expenses",
                type: "int",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "expense_shares",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    expense_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_shares", x => x.id);
                    table.ForeignKey(
                        name: "FK_expense_shares_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expense_shares_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_payer_id",
                table: "expenses",
                column: "payer_id");

            migrationBuilder.CreateIndex(
                name: "IX_expense_shares_expense_id_user_id",
                table: "expense_shares",
                columns: new[] { "expense_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expense_shares_user_id",
                table: "expense_shares",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_expenses_users_payer_id",
                table: "expenses",
                column: "payer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expenses_users_payer_id",
                table: "expenses");

            migrationBuilder.DropTable(
                name: "expense_shares");

            migrationBuilder.DropIndex(
                name: "IX_expenses_payer_id",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "payer_id",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "voided",
                table: "expenses");

            migrationBuilder.AddColumn<bool>(
                name: "settled",
                table: "expenses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "splits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    expense_id = table.Column<int>(type: "int", nullable: false),
                    group_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    paid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    paid_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_splits", x => x.id);
                    table.ForeignKey(
                        name: "FK_splits_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_splits_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_splits_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_splits_expense_id",
                table: "splits",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "IX_splits_group_id",
                table: "splits",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_splits_user_id_group_id_expense_id",
                table: "splits",
                columns: new[] { "user_id", "group_id", "expense_id" },
                unique: true);
        }
    }
}
