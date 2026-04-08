using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
  /// <inheritdoc />
  public partial class RemoveDescription : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
        name: "Description",
        table: "Todos");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
        name: "Description",
        table: "Todos",
        type: "TEXT",
        maxLength: 1000,
        nullable: true);
    }
  }
}
