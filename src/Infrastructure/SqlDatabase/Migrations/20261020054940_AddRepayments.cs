using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SqlDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddRepayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "repayments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    group_id = table.Column<int>(type: "int", nullable: false),
                    sender_id = table.Column<int>(type: "int", nullable: false),
                    recipient_id = table.Column<int>(type: "int", nullable: false),
                    recorded_by = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "longtext", nullable: false),
                    unique_key = table.Column<Guid>(type: "char(36)", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repayments", x => x.id);
                    table.ForeignKey(
                        name: "FK_repayments_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_repayments_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_repayments_users_recorded_by",
                        column: x => x.recorded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_repayments_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_group_id",
                table: "repayments",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_recipient_id",
                table: "repayments",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_recorded_by",
                table: "repayments",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_sender_id",
                table: "repayments",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_unique_key",
                table: "repayments",
                column: "unique_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repayments");
        }
    }
}
