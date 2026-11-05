using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditLogsActionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ADD COLUMN IF NOT EXISTS \"Action\" character varying(50) NOT NULL DEFAULT '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" DROP COLUMN IF EXISTS \"Action\";");
        }
    }
}
