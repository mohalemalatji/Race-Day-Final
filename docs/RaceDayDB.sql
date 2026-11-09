-- Part 1 baseline setup only. This script drops and recreates all RaceDay tables.
-- Do not run it against your existing database if you need to keep its data.
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO
DROP TABLE IF EXISTS RESULT;
DROP TABLE IF EXISTS ENROLMENT;
DROP TABLE IF EXISTS EVENTCATEGORY;
DROP TABLE IF EXISTS CATEGORY;
DROP TABLE IF EXISTS EVENT;
DROP TABLE IF EXISTS [USER];
GO
CREATE TABLE [USER] (
    UserID VARCHAR(50) PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('Organiser', 'Participant'))
);
CREATE TABLE EVENT (
    EventID VARCHAR(50) PRIMARY KEY,
    OrganiserID VARCHAR(50) NOT NULL,
    EventName VARCHAR(200) NOT NULL,
    Description VARCHAR(MAX),
    EventDate DATE NOT NULL,
    Location VARCHAR(200) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Upcoming',

    CONSTRAINT FK_Event_Organiser 
        FOREIGN KEY (OrganiserID) REFERENCES [USER](UserID)
);
CREATE TABLE CATEGORY (
    CategoryID VARCHAR(50) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(MAX)
);
CREATE TABLE EVENTCATEGORY (
    EventCategoryID VARCHAR(50) PRIMARY KEY,
    EventID VARCHAR(50) NOT NULL,
    CategoryID VARCHAR(50) NOT NULL,
    EntryFee DECIMAL(10, 2) NOT NULL,
    MaxParticipants INT NOT NULL,

    CONSTRAINT FK_EventCategory_Event 
        FOREIGN KEY (EventID) REFERENCES EVENT(EventID) ON DELETE CASCADE,

    CONSTRAINT FK_EventCategory_Category 
        FOREIGN KEY (CategoryID) REFERENCES CATEGORY(CategoryID) ON DELETE CASCADE
);
CREATE TABLE ENROLMENT (
    EnrolmentID VARCHAR(50) PRIMARY KEY,
    ParticipantID VARCHAR(50) NOT NULL,
    EventCategoryID VARCHAR(50) NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(50) NOT NULL DEFAULT 'Registered',

    CONSTRAINT FK_Enrolment_Participant 
        FOREIGN KEY (ParticipantID) REFERENCES [USER](UserID),

    CONSTRAINT FK_Enrolment_EventCategory 
        FOREIGN KEY (EventCategoryID) REFERENCES EVENTCATEGORY(EventCategoryID)
);
CREATE TABLE RESULT (
    ResultID VARCHAR(50) PRIMARY KEY,
    EnrolmentID VARCHAR(50) NOT NULL UNIQUE,
    FinishTime TIME NULL,
    Position INT NULL,
    ResultStatus VARCHAR(50) NOT NULL DEFAULT 'Completed',

    CONSTRAINT FK_Result_Enrolment 
        FOREIGN KEY (EnrolmentID) REFERENCES ENROLMENT(EnrolmentID) ON DELETE CASCADE
);
INSERT INTO [USER] (UserID, FirstName, LastName, Email, PasswordHash, Role)
VALUES 
    ('U01', 'Sarah', 'Jenkins', 'sarah.j@raceday.co.za', 'hashed123', 'Organiser'),
    ('U02', 'David', 'Naidoo', 'david.n@raceday.co.za', 'hashed456', 'Organiser'),
    ('U03', 'Mike', 'Ross', 'mike.r@gmail.com', 'hashed789', 'Participant'),
    ('U04', 'Rachel', 'Zane', 'rachel.z@gmail.com', 'hashed012', 'Participant');
INSERT INTO EVENT (EventID, OrganiserID, EventName, Description, EventDate, Location, Status)
VALUES 
    ('E01', 'U01', 'Comrades Marathon', 'Annual ultra marathon', '2024-06-09', 'Durban', 'Upcoming'),
    ('E02', 'U01', 'Cape Town Cycle Tour', 'Scenic cycling event', '2024-03-10', 'Cape Town', 'Completed'),
    ('E03', 'U02', 'Soweto Marathon', 'Road running event', '2024-11-03', 'Johannesburg', 'Upcoming');
INSERT INTO CATEGORY (CategoryID, CategoryName, Description)
VALUES 
    ('C01', 'Ultra Marathon', '90km running race'),
    ('C02', '109km Cycling', 'Standard cycling race'),
    ('C03', 'Half Marathon', '21.1km running race');
INSERT INTO EVENTCATEGORY (EventCategoryID, EventID, CategoryID, EntryFee, MaxParticipants)
VALUES 
    ('EC01', 'E01', 'C01', 1200.00, 20000),
    ('EC02', 'E02', 'C02', 850.00, 35000),
    ('EC03', 'E03', 'C03', 450.00, 10000);
INSERT INTO ENROLMENT (EnrolmentID, ParticipantID, EventCategoryID, EnrolmentDate, Status)
VALUES 
    ('EN01', 'U03', 'EC01', '2024-01-15 08:30:00', 'Registered'),
    ('EN02', 'U04', 'EC02', '2023-11-20 14:15:00', 'Completed');
INSERT INTO RESULT (ResultID, EnrolmentID, FinishTime, Position, ResultStatus)
VALUES 
    ('R01', 'EN02', '03:15:42', 154, 'Finished');

GO




