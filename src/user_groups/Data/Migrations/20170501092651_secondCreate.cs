using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace user_groups.Data.Migrations
{
    public partial class secondCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupItem",
                columns: table => new
                {
                    GroupItemID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GroupName = table.Column<string>(nullable: true),
                    Host = table.Column<string>(nullable: true),
                    MemberCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupItem", x => x.GroupItemID);
                });

            migrationBuilder.CreateTable(
                name: "GroupUserLink",
                columns: table => new
                {
                    GroupUserLinkID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    ApplicationUserID = table.Column<string>(nullable: true),
                    GroupItemID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUserLink", x => x.GroupUserLinkID);
                    table.ForeignKey(
                        name: "FK_GroupUserLink_GroupItem_GroupItemID",
                        column: x => x.GroupItemID,
                        principalTable: "GroupItem",
                        principalColumn: "GroupItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupUserLink_GroupItemID",
                table: "GroupUserLink",
                column: "GroupItemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupUserLink");

            migrationBuilder.DropTable(
                name: "GroupItem");
        }
    }
}
