using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public sealed class LoginPage
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public sealed class RegisterPage
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(100)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = "";

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "Participant";
}

public sealed class EventFormPage
{
    public string? EventID { get; set; }

    [Required, StringLength(200)]
    public string EventName { get; set; } = "";

    public string? Description { get; set; }

    [Required]
    public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, StringLength(200)]
    public string Location { get; set; } = "";

    [Range(0.01, 1000)]
    public decimal DistanceKm { get; set; }

    [Required]
    public string EventType { get; set; } = "Run";

    public string Status { get; set; } = "Upcoming";
    public string? BannerImageUrl { get; set; }
    public IFormFile? BannerImage { get; set; }
}

public sealed class CategoryPage
{
    public EventDto Event { get; set; } = default!;
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];
    public string? Error { get; set; }
}

public sealed class CategoryCreatePage
{
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = "";

    public string? Description { get; set; }
}

public sealed class CategoryLinkPage
{
    [Required]
    public string CategoryID { get; set; } = "";

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    [Range(1, 1000000)]
    public int MaxParticipants { get; set; }
}

public sealed class ResultPage
{
    public string EventID { get; set; } = "";
    public string EnrolmentID { get; set; } = "";
    public string EventName { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public TimeSpan? FinishTime { get; set; }

    [Range(1, 1000000)]
    public int? Position { get; set; }

    [Required, StringLength(50)]
    public string ResultStatus { get; set; } = "Completed";
}

public sealed class ProfilePage
{
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
    public IFormFile? ProfilePicture { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(100)]
    public string LastName { get; set; } = "";
}

public sealed class EventListPage
{
    public IReadOnlyList<EventDto> Events { get; set; } = [];
    public string? Search { get; set; }
    public string? EventType { get; set; }
    public string? Error { get; set; }
}

public sealed class EventDetailsPage
{
    public EventDto? Event { get; set; }
    public string? Error { get; set; }
}

public sealed class OrganiserHomePage
{
    public IReadOnlyList<EventDto> Events { get; set; } = [];
    public string? Error { get; set; }
    public int TotalEnrolments => Events.Sum(e => e.Categories.Sum(c => c.Enrolled));
}

public sealed class EnrolmentsPage
{
    public IReadOnlyList<EnrolmentDto> Enrolments { get; set; } = [];
    public string? EventName { get; set; }
    public string? Error { get; set; }
}
