namespace RaceDay.Api.Models;

public sealed class User
{
    public string UserID { get; set; } = Guid.NewGuid().ToString("D");
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public string? ProfilePictureBlobName { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public sealed class Event
{
    public string EventID { get; set; } = Guid.NewGuid().ToString("D");
    public string OrganiserID { get; set; } = "";
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "Run";
    public string Status { get; set; } = "Upcoming";
    public string? BannerBlobName { get; set; }
    public User? Organiser { get; set; }
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}

public sealed class Category
{
    public string CategoryID { get; set; } = Guid.NewGuid().ToString("D");
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}

public sealed class EventCategory
{
    public string EventCategoryID { get; set; } = Guid.NewGuid().ToString("D");
    public string EventID { get; set; } = "";
    public string CategoryID { get; set; } = "";
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; }
    public Event? Event { get; set; }
    public Category? Category { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public sealed class Enrolment
{
    public string EnrolmentID { get; set; } = Guid.NewGuid().ToString("D");
    public string ParticipantID { get; set; } = "";
    public string EventCategoryID { get; set; } = "";
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Registered";
    public User? Participant { get; set; }
    public EventCategory? EventCategory { get; set; }
    public Result? Result { get; set; }
}

public sealed class Result
{
    public string ResultID { get; set; } = Guid.NewGuid().ToString("D");
    public string EnrolmentID { get; set; } = "";
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string ResultStatus { get; set; } = "Completed";
    public Enrolment? Enrolment { get; set; }
}

public sealed class ImageAsset
{
    public string ImageKey { get; set; } = "";
    public string ContainerName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public byte[] Data { get; set; } = [];
}

