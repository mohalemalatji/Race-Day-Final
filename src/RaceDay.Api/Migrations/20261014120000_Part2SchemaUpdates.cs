using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceDay.Api.Migrations;

[DbContext(typeof(RaceDay.Api.Data.RaceDayDbContext))]
[Migration("20261014120000_Part2SchemaUpdates")]
public sealed class Part2SchemaUpdates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.[USER]', N'U') IS NULL
               OR OBJECT_ID(N'dbo.EVENT', N'U') IS NULL
               OR OBJECT_ID(N'dbo.CATEGORY', N'U') IS NULL
               OR OBJECT_ID(N'dbo.EVENTCATEGORY', N'U') IS NULL
               OR OBJECT_ID(N'dbo.ENROLMENT', N'U') IS NULL
               OR OBJECT_ID(N'dbo.RESULT', N'U') IS NULL
            BEGIN
                THROW 51000, 'The Part 1 RaceDayDB tables were not found. Create the Part 1 database before applying this Part 2 migration.', 1;
            END;
            """);

        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.EVENT', N'DistanceKm') IS NULL
                ALTER TABLE dbo.EVENT ADD DistanceKm decimal(8,2) NULL;
            IF COL_LENGTH(N'dbo.EVENT', N'EventType') IS NULL
                ALTER TABLE dbo.EVENT ADD EventType varchar(20) NULL;
            """);

        migrationBuilder.Sql("""
            UPDATE dbo.EVENT SET DistanceKm = CASE EventID
                WHEN 'E01' THEN 90.00 WHEN 'E02' THEN 109.00 WHEN 'E03' THEN 42.20 END
            WHERE DistanceKm IS NULL AND EventID IN ('E01', 'E02', 'E03');
            UPDATE dbo.EVENT SET EventType = CASE EventID
                WHEN 'E01' THEN 'Run' WHEN 'E02' THEN 'Cycle' WHEN 'E03' THEN 'Run' END
            WHERE EventType IS NULL AND EventID IN ('E01', 'E02', 'E03');

            IF EXISTS (SELECT 1 FROM dbo.EVENT WHERE DistanceKm IS NULL OR EventType IS NULL)
            BEGIN
                THROW 51001, 'Set DistanceKm and EventType for every existing event, then rerun the Part 2 database update.', 1;
            END;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE dbo.EVENT ALTER COLUMN DistanceKm decimal(8,2) NOT NULL;
            ALTER TABLE dbo.EVENT ALTER COLUMN EventType varchar(20) NOT NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.EVENT') AND name = N'CK_EVENT_EventType')
                ALTER TABLE dbo.EVENT ADD CONSTRAINT CK_EVENT_EventType CHECK (EventType IN ('Run', 'Walk', 'Cycle'));
            """);

        migrationBuilder.Sql("""
            IF EXISTS (SELECT EventID, CategoryID FROM dbo.EVENTCATEGORY GROUP BY EventID, CategoryID HAVING COUNT(*) > 1)
            BEGIN
                THROW 51002, 'Duplicate event/category links exist. Resolve them before applying the Part 2 update.', 1;
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.EVENTCATEGORY') AND name = N'IX_EVENTCATEGORY_EventID_CategoryID')
                CREATE UNIQUE INDEX IX_EVENTCATEGORY_EventID_CategoryID ON dbo.EVENTCATEGORY(EventID, CategoryID);
            """);

        migrationBuilder.Sql("""
            IF EXISTS (SELECT ParticipantID, EventCategoryID FROM dbo.ENROLMENT GROUP BY ParticipantID, EventCategoryID HAVING COUNT(*) > 1)
            BEGIN
                THROW 51003, 'Duplicate participant enrolments exist. Resolve them before applying the Part 2 update.', 1;
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ENROLMENT') AND name = N'IX_ENROLMENT_ParticipantID_EventCategoryID')
                CREATE UNIQUE INDEX IX_ENROLMENT_ParticipantID_EventCategoryID ON dbo.ENROLMENT(ParticipantID, EventCategoryID);
            """);
    }


    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new InvalidOperationException("This migration upgrades the Part 1 database in place; it cannot be reversed without risking existing data.");
    }
}
