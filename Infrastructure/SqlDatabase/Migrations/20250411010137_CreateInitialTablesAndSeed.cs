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
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fist_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    last_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    email = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    phone = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    password = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
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
                    name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    unique_key = table.Column<Guid>(type: "char(36)", nullable: false),
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
                name: "member_of",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    group_id = table.Column<int>(type: "int", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_groups_owner_id",
                table: "groups",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_of_group_id",
                table: "member_of",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_of_user_id_group_id",
                table: "member_of",
                columns: new[] { "user_id", "group_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "member_of");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
