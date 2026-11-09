using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Contracts;

public sealed record RegisterRequest([Required, StringLength(100)] string FirstName, [Required, StringLength(100)] string LastName,
    [Required, EmailAddress, StringLength(255)] string Email, [Required, MinLength(8)] string Password,
    [Required, RegularExpression("^(Organiser|Participant)$")] string Role);


public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);


public sealed record ProfileUpdateRequest([Required, StringLength(100)] string FirstName, [Required, StringLength(100)] string LastName);


public sealed record CategoryRequest([Required, StringLength(100)] string CategoryName, string? Description);


public sealed record EventCreateRequest([Required, StringLength(200)] string EventName, string? Description, DateOnly EventDate,
    [Required, StringLength(200)] string Location, [Range(0.01, 1000)] decimal DistanceKm,
    [Required, RegularExpression("^(Run|Walk|Cycle)$")] string EventType);


public sealed record EventUpdateRequest([Required, StringLength(200)] string EventName, string? Description, DateOnly EventDate,
    [Required, StringLength(200)] string Location, [Range(0.01, 1000)] decimal DistanceKm,
    [Required, RegularExpression("^(Run|Walk|Cycle)$")] string EventType, [Required, StringLength(50)] string Status);


public sealed record EventCategoryRequest([Required] string CategoryID, [Range(0, 100000)] decimal EntryFee, [Range(1, 1000000)] int MaxParticipants);


public sealed record ResultRequest(TimeSpan? FinishTime, [Range(1, 1000000)] int? Position, [Required, StringLength(50)] string ResultStatus);
