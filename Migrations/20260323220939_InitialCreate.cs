using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemResourceMonitorAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Таблиці вже існують в БД - нічого не створюємо
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // нічого
        }
    }
}
