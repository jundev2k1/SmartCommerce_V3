using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.User.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileMiddleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "user_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "user_profiles");
        }
    }
}
