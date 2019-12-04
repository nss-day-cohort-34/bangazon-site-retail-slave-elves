using Microsoft.EntityFrameworkCore.Migrations;

namespace Bangazon.Migrations
{
    public partial class dbcontxtupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "98eac729-bf89-44dd-ab7b-d697f0285428", "AQAAAAEAACcQAAAAEN4U4yeSAFjRdT4Cq0McZ4JPu8F11mnSeyAnVLa5I2f+GIdaCG0Pv0DaY2/EX0+zOQ==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-ffff-ffff-ffff-ffffffffffff",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "47cac9df-fd92-428d-bc7b-a9c8422fc94f", "AQAAAAEAACcQAAAAEEmyLGxG7vW4bdYaqcXuinEqEeR85mIPF0sWqrCU9OQ9TnzE9xCYIIoYTnTDqMV22Q==" });
        }
    }
}
