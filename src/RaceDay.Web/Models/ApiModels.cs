namespace RaceDay.Web.Models;

public sealed record UserDto(string UserID, string FirstName, string LastName, string Email, string Role, string? ProfilePictureUrl = null);


public sealed record AuthDto(string Token, DateTime ExpiresAtUtc, UserDto User);


public sealed record CategoryDto(string CategoryID, string CategoryName, string? Description);


public sealed record EventCategoryDto(string EventCategoryID, string CategoryID, string CategoryName,
    decimal EntryFee, int MaxParticipants, int Enrolled);


public sealed record EventDto(string EventID, string EventName, string? Description, DateOnly EventDate,
    string Location, decimal DistanceKm, string EventType, string Status, string OrganiserID,
    IReadOnlyList<EventCategoryDto> Categories, string? BannerImageUrl = null);


public sealed record EnrolmentDto(string EnrolmentID, string EventID, string EventName, string CategoryName,
    DateTime EnrolmentDateUtc, string Status, string ParticipantID, string ParticipantName,
    TimeSpan? FinishTime = null, int? Position = null, string? ResultStatus = null);


public sealed record LoginRequest(string Email, string Password);


public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password, string Role);


public sealed record ProfileUpdateRequest(string FirstName, string LastName);


public sealed record EventCreateRequest(string EventName, string? Description, DateOnly EventDate,
    string Location, decimal DistanceKm, string EventType);


public sealed record EventUpdateRequest(string EventName, string? Description, DateOnly EventDate,
    string Location, decimal DistanceKm, string EventType, string Status);


public sealed record CategoryCreateRequest(string CategoryName, string? Description);


public sealed record EventCategoryRequest(string CategoryID, decimal EntryFee, int MaxParticipants);


public sealed record ResultRequest(TimeSpan? FinishTime, int? Position, string ResultStatus);
