using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxActorColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "actor_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_type",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "system");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actor_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "actor_type",
                table: "outbox_messages");
        }
    }
}
