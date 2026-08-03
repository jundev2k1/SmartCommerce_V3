using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEcommerce.User.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPhoneSearchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "phone_reverse",
                table: "user_profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone_search",
                table: "user_profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Backfill existing rows so PhoneSearch/PhoneReverse stay consistent with PhoneNumber for data
            // written before this migration - new rows are already kept in sync by UserProfile itself.
            migrationBuilder.Sql(
                """
                UPDATE user_profiles
                SET phone_search = regexp_replace(phone_number, '\D', '', 'g'),
                    phone_reverse = reverse(regexp_replace(phone_number, '\D', '', 'g'));
                """);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_phone_reverse",
                table: "user_profiles",
                column: "phone_reverse");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_phone_search",
                table: "user_profiles",
                column: "phone_search");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_profiles_phone_reverse",
                table: "user_profiles");

            migrationBuilder.DropIndex(
                name: "ix_user_profiles_phone_search",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "phone_reverse",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "phone_search",
                table: "user_profiles");
        }
    }
}
