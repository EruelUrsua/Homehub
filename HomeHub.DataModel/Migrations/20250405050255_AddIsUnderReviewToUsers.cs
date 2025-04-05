using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeHub.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUnderReviewToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnderReview",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnderReview",
                table: "AspNetUsers");
        }
    }
}
