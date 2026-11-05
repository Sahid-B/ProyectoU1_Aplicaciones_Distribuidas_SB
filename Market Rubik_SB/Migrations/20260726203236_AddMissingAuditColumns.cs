using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ADD COLUMN IF NOT EXISTS \"Details\" character varying(1000) NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ADD COLUMN IF NOT EXISTS \"EntityName\" character varying(100) NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ADD COLUMN IF NOT EXISTS \"Timestamp\" timestamp without time zone NOT NULL DEFAULT '2020-01-01 00:00:00';");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ADD COLUMN IF NOT EXISTS \"UserId\" character varying(100) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" DROP COLUMN IF EXISTS \"Details\";");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" DROP COLUMN IF EXISTS \"EntityName\";");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" DROP COLUMN IF EXISTS \"Timestamp\";");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" DROP COLUMN IF EXISTS \"UserId\";");
        }
    }
}
