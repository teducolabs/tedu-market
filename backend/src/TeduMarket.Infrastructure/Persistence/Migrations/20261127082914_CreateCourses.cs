using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeduMarket.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateCourses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "courses",
            columns: table => new
            {
                course_id = table.Column<Guid>(type: "uuid", nullable: false),
                instructor_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                price = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_courses", x => x.course_id);
                table.ForeignKey(
                    name: "fk_courses_categories_category_id",
                    column: x => x.category_id,
                    principalTable: "categories",
                    principalColumn: "category_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_courses_category_id",
            table: "courses",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_courses_instructor_profile_id",
            table: "courses",
            column: "instructor_profile_id");

        migrationBuilder.CreateIndex(
            name: "ux_courses_slug",
            table: "courses",
            column: "slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "courses");
    }
}
