using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectsManager.Migrations
{
    public partial class DatesWithOutTIme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "20fd8055-4dcb-4201-ba8d-fd7026b748d1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                column: "ConcurrencyStamp",
                value: "7ed15213-4adb-4dd4-a7d5-2875f0c6315f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a6c6-9443d048cdb9-8e445865-a24d-4543",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5242a236-d149-4532-9008-e985933f70ca", "AQAAAAEAACcQAAAAEAD1V3uGniDd676DL+zaLatpDYk/0rpByqCSsbraHC8MgULVFHYqm9Nit2lzz2ehyg==", "665e7c4a-156d-4988-9a46-f0a0318f52e2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "fac6dc47-6b6e-4ecf-be96-590847ec198b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                column: "ConcurrencyStamp",
                value: "37d77cbc-c982-4cc1-9c41-c013c51a2350");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a6c6-9443d048cdb9-8e445865-a24d-4543",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fca16436-bca2-42cb-9b86-f754277bfdf3", "AQAAAAEAACcQAAAAEO9YGDUL+3RXOfAnD17VIeA/S9tBDlZ2TabKTln52pHn13cjwHOb/Nh7foEVVYAEKg==", "4a1bf372-ea14-4552-8d0d-ecae6a353d13" });
        }
    }
}
