using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MeasurementUnits");

            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.CreateTable(
                name: "MeasurementUnit",
                schema: "MeasurementUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductOwner",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOwner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MeasurementUnitId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_MeasurementUnit_MeasurementUnitId",
                        column: x => x.MeasurementUnitId,
                        principalSchema: "MeasurementUnits",
                        principalTable: "MeasurementUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Product_ProductOwner_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "Inventory",
                        principalTable: "ProductOwner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Record",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Record", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Record_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "MeasurementUnits",
                table: "MeasurementUnit",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "шт." },
                    { 2, "м" },
                    { 3, "кг" }
                });

            migrationBuilder.InsertData(
                schema: "Inventory",
                table: "ProductOwner",
                column: "Id",
                value: new Guid("160be924-907f-4d70-d15c-08de2383d454"));

            migrationBuilder.InsertData(
                schema: "Inventory",
                table: "Product",
                columns: new[] { "Id", "CreatedAt", "Description", "LastModifiedAt", "MeasurementUnitId", "Name", "OwnerId" },
                values: new object[] { new Guid("db976302-1ffe-4993-85f7-3ac1397cc5be"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Пара кожанных перчаток с заячьим мехом", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Перчатки", new Guid("160be924-907f-4d70-d15c-08de2383d454") });

            migrationBuilder.InsertData(
                schema: "Inventory",
                table: "Record",
                columns: new[] { "Id", "ProductId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("9fa64946-0b65-44f8-959c-d58360918180"), new Guid("db976302-1ffe-4993-85f7-3ac1397cc5be"), 20m, 93.99m },
                    { new Guid("d848715a-ca18-47b2-be95-a3ca5325848c"), new Guid("db976302-1ffe-4993-85f7-3ac1397cc5be"), 3m, 80.70m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnit_Name",
                schema: "MeasurementUnits",
                table: "MeasurementUnit",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_MeasurementUnitId",
                schema: "Inventory",
                table: "Product",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_OwnerId",
                schema: "Inventory",
                table: "Product",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOwner_Id",
                schema: "Inventory",
                table: "ProductOwner",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Record_ProductId",
                schema: "Inventory",
                table: "Record",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Record",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MeasurementUnit",
                schema: "MeasurementUnits");

            migrationBuilder.DropTable(
                name: "ProductOwner",
                schema: "Inventory");
        }
    }
}
