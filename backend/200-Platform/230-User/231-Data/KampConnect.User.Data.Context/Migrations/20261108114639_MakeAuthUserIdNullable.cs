using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KampConnect.User.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class MakeAuthUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "auth_user_id",
                schema: "user",
                table: "usr_user",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "auth_user_id",
                schema: "user",
                table: "usr_user",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
