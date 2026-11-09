using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceDay.Api.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @tableCount int =
                (CASE WHEN OBJECT_ID(N'dbo.[USER]', N'U') IS NULL THEN 0 ELSE 1 END) +
                (CASE WHEN OBJECT_ID(N'dbo.EVENT', N'U') IS NULL THEN 0 ELSE 1 END) +
                (CASE WHEN OBJECT_ID(N'dbo.CATEGORY', N'U') IS NULL THEN 0 ELSE 1 END) +
                (CASE WHEN OBJECT_ID(N'dbo.EVENTCATEGORY', N'U') IS NULL THEN 0 ELSE 1 END) +
                (CASE WHEN OBJECT_ID(N'dbo.ENROLMENT', N'U') IS NULL THEN 0 ELSE 1 END) +
                (CASE WHEN OBJECT_ID(N'dbo.RESULT', N'U') IS NULL THEN 0 ELSE 1 END);

            IF @tableCount BETWEEN 1 AND 5
                THROW 51004, 'The RaceDay database has only some Part 1 tables. Restore the complete Part 1 schema before applying migrations.', 1;
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.[USER]', N'U') IS NULL
                CREATE TABLE dbo.[USER] (
                    UserID varchar(50) NOT NULL PRIMARY KEY,
                    FirstName varchar(100) NOT NULL,
                    LastName varchar(100) NOT NULL,
                    Email varchar(255) NOT NULL UNIQUE,
                    PasswordHash varchar(255) NOT NULL,
                    Role varchar(20) NOT NULL,
                    CONSTRAINT CK_USER_Role CHECK (Role IN ('Organiser', 'Participant'))
                );
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.EVENT', N'U') IS NULL
                CREATE TABLE dbo.EVENT (
                    EventID varchar(50) NOT NULL PRIMARY KEY,
                    OrganiserID varchar(50) NOT NULL,
                    EventName varchar(200) NOT NULL,
                    Description varchar(max) NULL,
                    EventDate date NOT NULL,
                    Location varchar(200) NOT NULL,
                    Status varchar(50) NOT NULL CONSTRAINT DF_EVENT_Status DEFAULT 'Upcoming',
                    CONSTRAINT FK_Event_Organiser FOREIGN KEY (OrganiserID) REFERENCES dbo.[USER](UserID)
                );
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.CATEGORY', N'U') IS NULL
                CREATE TABLE dbo.CATEGORY (
                    CategoryID varchar(50) NOT NULL PRIMARY KEY,
                    CategoryName varchar(100) NOT NULL UNIQUE,
                    Description varchar(max) NULL
                );
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.EVENTCATEGORY', N'U') IS NULL
                CREATE TABLE dbo.EVENTCATEGORY (
                    EventCategoryID varchar(50) NOT NULL PRIMARY KEY,
                    EventID varchar(50) NOT NULL,
                    CategoryID varchar(50) NOT NULL,
                    EntryFee decimal(10,2) NOT NULL,
                    MaxParticipants int NOT NULL,
                    CONSTRAINT FK_EventCategory_Event FOREIGN KEY (EventID) REFERENCES dbo.EVENT(EventID) ON DELETE CASCADE,
                    CONSTRAINT FK_EventCategory_Category FOREIGN KEY (CategoryID) REFERENCES dbo.CATEGORY(CategoryID) ON DELETE CASCADE
                );
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.ENROLMENT', N'U') IS NULL
                CREATE TABLE dbo.ENROLMENT (
                    EnrolmentID varchar(50) NOT NULL PRIMARY KEY,
                    ParticipantID varchar(50) NOT NULL,
                    EventCategoryID varchar(50) NOT NULL,
                    EnrolmentDate datetime NOT NULL CONSTRAINT DF_ENROLMENT_Date DEFAULT GETDATE(),
                    Status varchar(50) NOT NULL CONSTRAINT DF_ENROLMENT_Status DEFAULT 'Registered',
                    CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (ParticipantID) REFERENCES dbo.[USER](UserID),
                    CONSTRAINT FK_Enrolment_EventCategory FOREIGN KEY (EventCategoryID) REFERENCES dbo.EVENTCATEGORY(EventCategoryID)
                );
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.RESULT', N'U') IS NULL
                CREATE TABLE dbo.RESULT (
                    ResultID varchar(50) NOT NULL PRIMARY KEY,
                    EnrolmentID varchar(50) NOT NULL UNIQUE,
                    FinishTime time NULL,
                    Position int NULL,
                    ResultStatus varchar(50) NOT NULL CONSTRAINT DF_RESULT_Status DEFAULT 'Completed',
                    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES dbo.ENROLMENT(EnrolmentID) ON DELETE CASCADE
                );
            """);
    }


    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new InvalidOperationException("The Part 1 baseline cannot be reversed because its tables and data predate this migration.");
    }
}
