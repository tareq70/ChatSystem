using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFriendStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserStatus",
                table: "Friends",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserStatus",
                table: "Friends");
        }
    }
}
