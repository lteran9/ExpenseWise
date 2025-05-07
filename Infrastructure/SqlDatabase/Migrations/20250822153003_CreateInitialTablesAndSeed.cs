using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SqlDatabase.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitialTablesAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    settled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: false),
                    currency = table.Column<string>(type: "longtext", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    unique_key = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fist_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    last_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    email = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    phone = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    unique_key = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    owner_id = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    unique_key = table.Column<Guid>(type: "char(36)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_groups_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "passwords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    cipher = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    encrypted = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_passwords_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "member_of",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    group_id = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_of", x => x.Id);
                    table.ForeignKey(
                        name: "FK_member_of_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_member_of_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "splits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    group_id = table.Column<int>(type: "int", nullable: false),
                    expense_id = table.Column<int>(type: "int", nullable: false),
                    paid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    paid_on = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                name: "IX_groups_owner_id",
                table: "groups",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_unique_key",
                table: "groups",
                column: "unique_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_member_of_group_id",
                table: "member_of",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_of_user_id_group_id",
                table: "member_of",
                columns: new[] { "user_id", "group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_passwords_user_id",
                table: "passwords",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_splits_expense_id",
                table: "splits",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "IX_splits_group_id",
                table: "splits",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_splits_user_id",
                table: "splits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_unique_key",
                table: "users",
                column: "unique_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "member_of");

            migrationBuilder.DropTable(
                name: "passwords");

            migrationBuilder.DropTable(
                name: "splits");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
