using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxRetrySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "processed_at",
                table: "inbox_messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "inbox_messages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "headers_json",
                table: "inbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "inbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_retry_at",
                table: "inbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_retry_at",
                table: "inbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payload",
                table: "inbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                table: "inbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Every pre-existing row was written only after a successful attempt (the old
            // implementation had no Pending/Retrying concept), so backfill them as Processed
            // rather than the CLR default (Pending) - "" would not parse back into the enum.
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "inbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "Processed");

            migrationBuilder.AddColumn<string>(
                name: "topic",
                table: "inbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_inbox_status_next_retry_at",
                table: "inbox_messages",
                columns: new[] { "status", "next_retry_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_inbox_status_next_retry_at",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "headers_json",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "last_retry_at",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "next_retry_at",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "payload",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "retry_count",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "status",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "topic",
                table: "inbox_messages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "processed_at",
                table: "inbox_messages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
