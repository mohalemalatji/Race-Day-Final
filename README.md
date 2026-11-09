# RaceDay

RaceDay is a race event management app with an ASP.NET Core API and web interface.

Image storage: My Azure Blob Storage was blocked by my school subscription’s region policy (`RequestDisallowedByAzure`, target: `mohales`). I used the local SQL Server database to store images instead.

## Roles

- Participant: Browse events and enrol in categories.
- Organiser: Create and manage events and categories.

## Run the project

Requirements: .NET 10 SDK and SQL Server.

Open `RaceDay.sln` in Visual Studio. Configure the local database connection and JWT key, then run the API and web projects.
