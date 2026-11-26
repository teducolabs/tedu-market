using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeduMarket.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateInstructorProfiles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "instructor_profiles",
            columns: table => new
            {
                instructor_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_instructor_profiles", x => x.instructor_profile_id);
                table.ForeignKey(
                    name: "fk_instructor_profiles_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ux_instructor_profiles_user_id",
            table: "instructor_profiles",
            column: "user_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "instructor_profiles");
    }
}
