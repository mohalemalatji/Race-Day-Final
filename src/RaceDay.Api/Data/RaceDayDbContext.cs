using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public sealed class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<ImageAsset> ImageAssets => Set<ImageAsset>();


    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(e =>
        {
            e.ToTable("USER", t => t.HasCheckConstraint("CK_USER_Role", "[Role] IN ('Organiser', 'Participant')")); e.HasKey(x => x.UserID); e.Property(x => x.UserID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.FirstName).HasMaxLength(100).IsUnicode(false).IsRequired(); e.Property(x => x.LastName).HasMaxLength(100).IsUnicode(false).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsUnicode(false).IsRequired(); e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsUnicode(false).IsRequired(); e.Property(x => x.Role).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.ProfilePictureBlobName).HasMaxLength(100).IsUnicode(false);
        });
        model.Entity<Event>(e =>
        {
            e.ToTable("EVENT", t => t.HasCheckConstraint("CK_EVENT_EventType", "[EventType] IN ('Run', 'Walk', 'Cycle')")); e.HasKey(x => x.EventID); e.Property(x => x.EventID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.EventName).HasMaxLength(200).IsUnicode(false).IsRequired(); e.Property(x => x.Description).HasColumnType("varchar(max)");
            e.Property(x => x.Location).HasMaxLength(200).IsUnicode(false).IsRequired(); e.Property(x => x.Status).HasMaxLength(50).IsUnicode(false).HasDefaultValue("Upcoming");
            e.Property(x => x.EventType).HasMaxLength(20).IsUnicode(false).IsRequired(); e.Property(x => x.DistanceKm).HasPrecision(8, 2);
            e.Property(x => x.BannerBlobName).HasMaxLength(100).IsUnicode(false);
            e.HasOne(x => x.Organiser).WithMany(x => x.Events).HasForeignKey(x => x.OrganiserID).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<Category>(e =>
        {
            e.ToTable("CATEGORY"); e.HasKey(x => x.CategoryID); e.Property(x => x.CategoryID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.CategoryName).HasMaxLength(100).IsUnicode(false).IsRequired(); e.HasIndex(x => x.CategoryName).IsUnique();
            e.Property(x => x.Description).HasColumnType("varchar(max)");
        });
        model.Entity<EventCategory>(e =>
        {
            e.ToTable("EVENTCATEGORY"); e.HasKey(x => x.EventCategoryID); e.Property(x => x.EventCategoryID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.EventID).HasMaxLength(50).IsUnicode(false); e.Property(x => x.CategoryID).HasMaxLength(50).IsUnicode(false); e.Property(x => x.EntryFee).HasPrecision(10, 2);
            e.HasOne(x => x.Event).WithMany(x => x.EventCategories).HasForeignKey(x => x.EventID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(x => x.EventCategories).HasForeignKey(x => x.CategoryID).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.EventID, x.CategoryID }).IsUnique();
        });
        model.Entity<Enrolment>(e =>
        {
            e.ToTable("ENROLMENT"); e.HasKey(x => x.EnrolmentID); e.Property(x => x.EnrolmentID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.ParticipantID).HasMaxLength(50).IsUnicode(false); e.Property(x => x.EventCategoryID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.EnrolmentDate).HasColumnType("datetime"); e.Property(x => x.Status).HasMaxLength(50).IsUnicode(false).HasDefaultValue("Registered");
            e.HasOne(x => x.Participant).WithMany(x => x.Enrolments).HasForeignKey(x => x.ParticipantID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EventCategory).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventCategoryID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.ParticipantID, x.EventCategoryID }).IsUnique();
        });
        model.Entity<Result>(e =>
        {
            e.ToTable("RESULT"); e.HasKey(x => x.ResultID); e.Property(x => x.ResultID).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.EnrolmentID).HasMaxLength(50).IsUnicode(false); e.Property(x => x.ResultStatus).HasMaxLength(50).IsUnicode(false).HasDefaultValue("Completed");
            e.HasIndex(x => x.EnrolmentID).IsUnique();
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentID).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<ImageAsset>(e =>
        {
            e.ToTable("IMAGEASSET"); e.HasKey(x => x.ImageKey); e.Property(x => x.ImageKey).HasMaxLength(100).IsUnicode(false);
            e.Property(x => x.ContainerName).HasMaxLength(50).IsUnicode(false).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsUnicode(false).IsRequired();
            e.Property(x => x.Data).HasColumnType("varbinary(max)").IsRequired();
        });
    }
}
