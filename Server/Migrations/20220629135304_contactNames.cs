using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
  public partial class contactNames : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "NameAtUserFrom",
          table: "Contacts",
          type: "TEXT",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "NameAtUserTo",
          table: "Contacts",
          type: "TEXT",
          nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "NameAtUserFrom",
          table: "Contacts");

      migrationBuilder.DropColumn(
          name: "NameAtUserTo",
          table: "Contacts");
    }
  }
}