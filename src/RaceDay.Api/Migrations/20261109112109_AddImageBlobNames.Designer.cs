using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RaceDay.Api.Data;

#nullable disable

namespace RaceDay.Api.Migrations
{
    [DbContext(typeof(RaceDayDbContext))]
    [Migration("20261109112109_AddImageBlobNames")]
    partial class AddImageBlobNames
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RaceDay.Api.Models.Category", b =>
                {
                    b.Property<string>("CategoryID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(max)");

                    b.HasKey("CategoryID");

                    b.HasIndex("CategoryName")
                        .IsUnique();

                    b.ToTable("CATEGORY", (string)null);
                });

            modelBuilder.Entity("RaceDay.Api.Models.Enrolment", b =>
                {
                    b.Property<string>("EnrolmentID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("EnrolmentDate")
                        .HasColumnType("datetime");

                    b.Property<string>("EventCategoryID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ParticipantID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("Registered");

                    b.HasKey("EnrolmentID");

                    b.HasIndex("EventCategoryID");

                    b.HasIndex("ParticipantID", "EventCategoryID")
                        .IsUnique();

                    b.ToTable("ENROLMENT", (string)null);
                });

            modelBuilder.Entity("RaceDay.Api.Models.Event", b =>
                {
                    b.Property<string>("EventID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("BannerBlobName")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(max)");

                    b.Property<decimal>("DistanceKm")
                        .HasPrecision(8, 2)
                        .HasColumnType("decimal(8,2)");

                    b.Property<DateOnly>("EventDate")
                        .HasColumnType("date");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("OrganiserID")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("Upcoming");

                    b.HasKey("EventID");

                    b.HasIndex("OrganiserID");

                    b.ToTable("EVENT", null, t =>
                        {
                            t.HasCheckConstraint("CK_EVENT_EventType", "[EventType] IN ('Run', 'Walk', 'Cycle')");
                        });
                });

            modelBuilder.Entity("RaceDay.Api.Models.EventCategory", b =>
                {
                    b.Property<string>("EventCategoryID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("CategoryID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<decimal>("EntryFee")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("EventID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("MaxParticipants")
                        .HasColumnType("int");

                    b.HasKey("EventCategoryID");

                    b.HasIndex("CategoryID");

                    b.HasIndex("EventID", "CategoryID")
                        .IsUnique();

                    b.ToTable("EVENTCATEGORY", (string)null);
                });

            modelBuilder.Entity("RaceDay.Api.Models.Result", b =>
                {
                    b.Property<string>("ResultID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("EnrolmentID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<TimeSpan?>("FinishTime")
                        .HasColumnType("time");

                    b.Property<int?>("Position")
                        .HasColumnType("int");

                    b.Property<string>("ResultStatus")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("Completed");

                    b.HasKey("ResultID");

                    b.HasIndex("EnrolmentID")
                        .IsUnique();

                    b.ToTable("RESULT", (string)null);
                });

            modelBuilder.Entity("RaceDay.Api.Models.User", b =>
                {
                    b.Property<string>("UserID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProfilePictureBlobName")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)");

                    b.HasKey("UserID");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("USER", null, t =>
                        {
                            t.HasCheckConstraint("CK_USER_Role", "[Role] IN ('Organiser', 'Participant')");
                        });
                });

            modelBuilder.Entity("RaceDay.Api.Models.Enrolment", b =>
                {
                    b.HasOne("RaceDay.Api.Models.EventCategory", "EventCategory")
                        .WithMany("Enrolments")
                        .HasForeignKey("EventCategoryID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RaceDay.Api.Models.User", "Participant")
                        .WithMany("Enrolments")
                        .HasForeignKey("ParticipantID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("EventCategory");

                    b.Navigation("Participant");
                });

            modelBuilder.Entity("RaceDay.Api.Models.Event", b =>
                {
                    b.HasOne("RaceDay.Api.Models.User", "Organiser")
                        .WithMany("Events")
                        .HasForeignKey("OrganiserID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Organiser");
                });

            modelBuilder.Entity("RaceDay.Api.Models.EventCategory", b =>
                {
                    b.HasOne("RaceDay.Api.Models.Category", "Category")
                        .WithMany("EventCategories")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RaceDay.Api.Models.Event", "Event")
                        .WithMany("EventCategories")
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Event");
                });

            modelBuilder.Entity("RaceDay.Api.Models.Result", b =>
                {
                    b.HasOne("RaceDay.Api.Models.Enrolment", "Enrolment")
                        .WithOne("Result")
                        .HasForeignKey("RaceDay.Api.Models.Result", "EnrolmentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Enrolment");
                });

            modelBuilder.Entity("RaceDay.Api.Models.Category", b =>
                {
                    b.Navigation("EventCategories");
                });

            modelBuilder.Entity("RaceDay.Api.Models.Enrolment", b =>
                {
                    b.Navigation("Result");
                });

            modelBuilder.Entity("RaceDay.Api.Models.Event", b =>
                {
                    b.Navigation("EventCategories");
                });

            modelBuilder.Entity("RaceDay.Api.Models.EventCategory", b =>
                {
                    b.Navigation("Enrolments");
                });

            modelBuilder.Entity("RaceDay.Api.Models.User", b =>
                {
                    b.Navigation("Enrolments");

                    b.Navigation("Events");
                });
#pragma warning restore 612, 618
        }
    }
}
