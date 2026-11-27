using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeduMarket.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateLessons : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "lessons",
            columns: table => new
            {
                lesson_id = table.Column<Guid>(type: "uuid", nullable: false),
                course_id = table.Column<Guid>(type: "uuid", nullable: false),
                lesson_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                lesson_order = table.Column<int>(type: "integer", nullable: false),
                is_preview = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_lessons", x => x.lesson_id);
                table.ForeignKey(
                    name: "fk_lessons_courses_course_id",
                    column: x => x.course_id,
                    principalTable: "courses",
                    principalColumn: "course_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_lessons_course_id_lesson_order",
            table: "lessons",
            columns: ["course_id", "lesson_order"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "lessons");
    }
}
