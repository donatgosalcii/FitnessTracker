using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Nutrition");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ChatMessages",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "FoodItems",
                schema: "Nutrition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ServingSizeValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ServingUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CaloriesPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ProteinPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CarbohydratesPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FatPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FiberPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SugarPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SodiumPerServing = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodItems_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNutritionGoals",
                schema: "Nutrition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GoalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetCalories = table.Column<int>(type: "int", nullable: false),
                    TargetProteinGrams = table.Column<int>(type: "int", nullable: false),
                    TargetCarbohydratesGrams = table.Column<int>(type: "int", nullable: false),
                    TargetFatGrams = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNutritionGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNutritionGoals_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoggedFoodItems",
                schema: "Nutrition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FoodItemId = table.Column<int>(type: "int", nullable: false),
                    LoggedDate = table.Column<DateTime>(type: "date", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MealContext = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuantityConsumed = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    CalculatedCalories = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CalculatedProtein = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CalculatedCarbohydrates = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CalculatedFat = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggedFoodItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoggedFoodItems_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoggedFoodItems_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalSchema: "Nutrition",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_ApplicationUserId",
                schema: "Nutrition",
                table: "FoodItems",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_Barcode",
                schema: "Nutrition",
                table: "FoodItems",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoggedFoodItems_ApplicationUserId",
                schema: "Nutrition",
                table: "LoggedFoodItems",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoggedFoodItems_FoodItemId",
                schema: "Nutrition",
                table: "LoggedFoodItems",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNutritionGoals_ApplicationUserId",
                schema: "Nutrition",
                table: "UserNutritionGoals",
                column: "ApplicationUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoggedFoodItems",
                schema: "Nutrition");

            migrationBuilder.DropTable(
                name: "UserNutritionGoals",
                schema: "Nutrition");

            migrationBuilder.DropTable(
                name: "FoodItems",
                schema: "Nutrition");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);
        }
    }
}
