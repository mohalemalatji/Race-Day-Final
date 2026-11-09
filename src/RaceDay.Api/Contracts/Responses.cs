namespace RaceDay.Api.Contracts;

public sealed record UserResponse(string UserID, string FirstName, string LastName, string Email, string Role, string? ProfilePictureUrl = null);


public sealed record AuthResponse(string Token, DateTime ExpiresAtUtc, UserResponse User);


public sealed record CategoryResponse(string CategoryID, string CategoryName, string? Description);


public sealed record EventCategoryResponse(string EventCategoryID, string CategoryID, string CategoryName, decimal EntryFee, int MaxParticipants, int Enrolled);


public sealed record EventResponse(string EventID, string EventName, string? Description, DateOnly EventDate, string Location,
    decimal DistanceKm, string EventType, string Status, string OrganiserID, IReadOnlyList<EventCategoryResponse> Categories,
    string? BannerImageUrl = null);


public sealed record EnrolmentResponse(string EnrolmentID, string EventID, string EventName, string CategoryName, DateTime EnrolmentDateUtc,
    string Status, string ParticipantID, string ParticipantName, TimeSpan? FinishTime = null, int? Position = null, string? ResultStatus = null);
