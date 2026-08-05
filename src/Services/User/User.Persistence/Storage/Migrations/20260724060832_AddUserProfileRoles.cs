using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.User.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "roles",
                table: "user_profiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_roles",
                table: "user_profiles",
                column: "roles")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_profiles_roles",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "roles",
                table: "user_profiles");
        }
    }
}
