using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KampConnect.Identity.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_invitation",
                schema: "roles",
                columns: table => new
                {
                    tenant_invitation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    sent_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    accepted_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    creation_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modification_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    accepted_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiry_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_invitation", x => x.tenant_invitation_id);
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "roles",
                        principalTable: "sec_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_tenant_snapshot_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "roles",
                        principalTable: "sec_tenant_snapshot",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_user_accepted_by_user_id",
                        column: x => x.accepted_by_user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_user_cancelled_by_user_id",
                        column: x => x.cancelled_by_user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tenant_invitation_sec_user_sent_by_user_id",
                        column: x => x.sent_by_user_id,
                        principalSchema: "users",
                        principalTable: "sec_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_accepted_by_user_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "accepted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_cancelled_by_user_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "cancelled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_role_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_sent_by_user_id",
                schema: "roles",
                table: "tenant_invitation",
                column: "sent_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_tenant_id_email",
                schema: "roles",
                table: "tenant_invitation",
                columns: new[] { "tenant_id", "email" },
                unique: true,
                filter: "[status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitation_token_hash",
                schema: "roles",
                table: "tenant_invitation",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_invitation",
                schema: "roles");
        }
    }
}
