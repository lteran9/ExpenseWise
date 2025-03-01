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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
