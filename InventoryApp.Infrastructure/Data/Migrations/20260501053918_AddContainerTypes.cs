using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "container_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_container_types", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_container_types_is_active",
                table: "container_types",
                column: "is_active");

            var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                table: "container_types",
                columns: new[] { "id", "name", "description", "is_active", "created_at", "updated_at" },
                values: new object[] { 1, "Общий", "Тип по умолчанию для новых контейнеров", true, seedTime, seedTime });
            migrationBuilder.InsertData(
                table: "container_types",
                columns: new[] { "id", "name", "description", "is_active", "created_at", "updated_at" },
                values: new object[] { 2, "Шкаф", null, true, seedTime, seedTime });
            migrationBuilder.InsertData(
                table: "container_types",
                columns: new[] { "id", "name", "description", "is_active", "created_at", "updated_at" },
                values: new object[] { 3, "Стеллаж", null, true, seedTime, seedTime });
            migrationBuilder.InsertData(
                table: "container_types",
                columns: new[] { "id", "name", "description", "is_active", "created_at", "updated_at" },
                values: new object[] { 4, "Ящик", null, true, seedTime, seedTime });
            migrationBuilder.InsertData(
                table: "container_types",
                columns: new[] { "id", "name", "description", "is_active", "created_at", "updated_at" },
                values: new object[] { 5, "Коробка", null, true, seedTime, seedTime });

            migrationBuilder.CreateIndex(
                name: "idx_containers_container_type_id",
                table: "containers",
                column: "container_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_containers_container_types_container_type_id",
                table: "containers",
                column: "container_type_id",
                principalTable: "container_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_containers_container_types_container_type_id",
                table: "containers");

            migrationBuilder.DropIndex(
                name: "idx_containers_container_type_id",
                table: "containers");

            migrationBuilder.DropTable(
                name: "container_types");
        }
    }
}
