using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KampConnect.User.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "avatar_file_id",
                schema: "user",
                table: "usr_user",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "user",
                table: "usr_user",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "usr_family",
                schema: "user",
                columns: table => new
                {
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    invite_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usr_family", x => x.family_id);
                });

            migrationBuilder.CreateTable(
                name: "usr_family_invite",
                schema: "user",
                columns: table => new
                {
                    family_invite_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usr_family_invite", x => x.family_invite_id);
                    table.ForeignKey(
                        name: "FK_usr_family_invite_usr_family_family_id",
                        column: x => x.family_id,
                        principalSchema: "user",
                        principalTable: "usr_family",
                        principalColumn: "family_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usr_family_invite_usr_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalSchema: "user",
                        principalTable: "usr_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usr_family_member",
                schema: "user",
                columns: table => new
                {
                    family_member_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_primary_contact = table.Column<bool>(type: "boolean", nullable: false),
                    invite_id = table.Column<int>(type: "integer", nullable: true),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usr_family_member", x => x.family_member_id);
                    table.ForeignKey(
                        name: "FK_usr_family_member_usr_family_family_id",
                        column: x => x.family_id,
                        principalSchema: "user",
                        principalTable: "usr_family",
                        principalColumn: "family_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usr_family_member_usr_family_invite_invite_id",
                        column: x => x.invite_id,
                        principalSchema: "user",
                        principalTable: "usr_family_invite",
                        principalColumn: "family_invite_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usr_family_member_usr_user_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalSchema: "user",
                        principalTable: "usr_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usr_family_member_usr_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "usr_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_invite_code_hash",
                schema: "user",
                table: "usr_family_invite",
                column: "code_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_invite_created_by_user_id",
                schema: "user",
                table: "usr_family_invite",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_invite_family_id_expires_at",
                schema: "user",
                table: "usr_family_invite",
                columns: new[] { "family_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_member_approved_by_user_id",
                schema: "user",
                table: "usr_family_member",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_member_family_id_user_id",
                schema: "user",
                table: "usr_family_member",
                columns: new[] { "family_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_member_invite_id",
                schema: "user",
                table: "usr_family_member",
                column: "invite_id");

            migrationBuilder.CreateIndex(
                name: "IX_usr_family_member_user_id",
                schema: "user",
                table: "usr_family_member",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usr_family_member",
                schema: "user");

            migrationBuilder.DropTable(
                name: "usr_family_invite",
                schema: "user");

            migrationBuilder.DropTable(
                name: "usr_family",
                schema: "user");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "user",
                table: "usr_user");

            migrationBuilder.AlterColumn<Guid>(
                name: "avatar_file_id",
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
