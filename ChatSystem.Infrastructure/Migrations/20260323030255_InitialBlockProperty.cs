using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialBlockProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "friendsStatus",
                table: "Friends",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "friendsStatus",
                table: "Friends");
        }
    }
}
