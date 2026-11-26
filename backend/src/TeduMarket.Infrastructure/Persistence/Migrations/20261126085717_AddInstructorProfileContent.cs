using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeduMarket.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddInstructorProfileContent : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "avatar_url",
            table: "instructor_profiles",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "biography",
            table: "instructor_profiles",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "display_name",
            table: "instructor_profiles",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "updated_at",
            table: "instructor_profiles",
            type: "timestamptz",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "avatar_url",
            table: "instructor_profiles");

        migrationBuilder.DropColumn(
            name: "biography",
            table: "instructor_profiles");

        migrationBuilder.DropColumn(
            name: "display_name",
            table: "instructor_profiles");

        migrationBuilder.DropColumn(
            name: "updated_at",
            table: "instructor_profiles");
    }
}
