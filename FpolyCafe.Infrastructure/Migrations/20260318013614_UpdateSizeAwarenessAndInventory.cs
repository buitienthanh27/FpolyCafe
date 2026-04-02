using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpolyCafe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSizeAwarenessAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Recipes",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "Categories",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "IngredientId",
                table: "Toppings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityNeeded",
                table: "Toppings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "Recipes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "BillDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Recipes",
                table: "Recipes",
                columns: new[] { "ProductId", "SizeId", "IngredientId" });

            migrationBuilder.CreateIndex(
                name: "IX_Toppings_IngredientId",
                table: "Toppings",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_SizeId",
                table: "Recipes",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_SizeId",
                table: "BillDetails",
                column: "SizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillDetails_Sizes_SizeId",
                table: "BillDetails",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "SizeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Sizes_SizeId",
                table: "Recipes",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "SizeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Toppings_Ingredients_IngredientId",
                table: "Toppings",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillDetails_Sizes_SizeId",
                table: "BillDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Sizes_SizeId",
                table: "Recipes");

            migrationBuilder.DropForeignKey(
                name: "FK_Toppings_Ingredients_IngredientId",
                table: "Toppings");

            migrationBuilder.DropIndex(
                name: "IX_Toppings_IngredientId",
                table: "Toppings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Recipes",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_SizeId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_BillDetails_SizeId",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "IngredientId",
                table: "Toppings");

            migrationBuilder.DropColumn(
                name: "QuantityNeeded",
                table: "Toppings");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "BillDetails");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "BasePrice");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "CategoryName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Recipes",
                table: "Recipes",
                columns: new[] { "ProductId", "IngredientId" });
        }
    }
}
