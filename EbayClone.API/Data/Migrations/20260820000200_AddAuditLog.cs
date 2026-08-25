using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EbayClone.API.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260820000200_AddAuditLog")]
public partial class AddAuditLog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[AuditLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [AuditLog] (
        [Id] bigint NOT NULL IDENTITY,
        [ActorId] int NULL,
        [Action] nvarchar(100) NOT NULL,
        [Resource] nvarchar(100) NOT NULL,
        [ResourceId] int NULL,
        [Metadata] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id])
    );
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLog");
    }
}
