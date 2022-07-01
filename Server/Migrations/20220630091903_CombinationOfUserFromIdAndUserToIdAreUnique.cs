using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
  public partial class CombinationOfUserFromIdAndUserToIdAreUnique : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_Contacts_UserFromId",
          table: "Contacts");

      migrationBuilder.CreateIndex(
          name: "IX_Contacts_UserFromId_UserToId",
          table: "Contacts",
          columns: new[] { "UserFromId", "UserToId" },
          unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_Contacts_UserFromId_UserToId",
          table: "Contacts");

      migrationBuilder.CreateIndex(
          name: "IX_Contacts_UserFromId",
          table: "Contacts",
          column: "UserFromId");
    }
  }
}