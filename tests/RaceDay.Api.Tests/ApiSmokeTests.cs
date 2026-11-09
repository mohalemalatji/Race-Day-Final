using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

public sealed class ApiSmokeTests
{
    [Fact]
    public async Task RegistrationLoginAndProfileRequireValidAuthentication()
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/profile")).StatusCode);

        Assert.Equal(HttpStatusCode.Created, (await Register(client, "runner@example.com", "Participant")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Register(client, "runner@example.com", "Participant")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { email = "runner@example.com", password = "wrong-password" })).StatusCode);
        var token = await Login(client, "runner@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var profile = await client.GetFromJsonAsync<JsonElement>("/api/users/profile");
        Assert.Equal("runner@example.com", profile.GetProperty("email").GetString());
        Assert.Equal("Participant", profile.GetProperty("role").GetString());
        Assert.False(profile.TryGetProperty("passwordHash", out _));
    }


    [Fact]
    public async Task OnlyOrganisersCanCreateEvents()
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();
        await Register(client, "athlete@example.com", "Participant");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login(client, "athlete@example.com"));
        var eventBody = new { eventName = "City 10K", description = "Road race", eventDate = "2027-04-15", location = "Cape Town", distanceKm = 10, eventType = "Run" };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/events", eventBody)).StatusCode);

        await Register(client, "organiser@example.com", "Organiser");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login(client, "organiser@example.com"));
        var created = await client.PostAsJsonAsync("/api/events", eventBody);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var eventJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var eventId = eventJson.RootElement.GetProperty("eventID").GetString();
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/events/{eventId}", new { eventName = "City 10K Updated", description = "Updated race", eventDate = "2027-04-15", location = "Cape Town", distanceKm = 10, eventType = "Run", status = "Upcoming" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/events/{eventId}")).StatusCode);
    }


    [Fact]
    public async Task ParticipantsCanEnrolAndDuplicateOrFullEntriesAreRejected()
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();
        await Register(client, "participant@example.com", "Participant");
        var participantId = await UserId(factory, "participant@example.com");
        const string categoryId = "category-test";
        const string eventCategoryId = "event-category-test";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Users.Add(new User { UserID = "organiser-test", FirstName = "Event", LastName = "Host", Email = "host@example.com", Role = "Organiser", PasswordHash = "unused" });
            db.Categories.Add(new Category { CategoryID = categoryId, CategoryName = "10km" });
            db.Events.Add(new Event { EventID = "event-test", OrganiserID = "organiser-test", EventName = "Test Run", EventDate = new DateOnly(2027, 4, 15), Location = "Durban", DistanceKm = 10, EventType = "Run" });
            db.EventCategories.Add(new EventCategory { EventCategoryID = eventCategoryId, EventID = "event-test", CategoryID = categoryId, MaxParticipants = 1 });
            await db.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login(client, "participant@example.com"));
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsync($"/api/event-categories/{eventCategoryId}/enrol", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/event-categories/{eventCategoryId}/enrol", null)).StatusCode);
        await Register(client, "overflow@example.com", "Participant");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login(client, "overflow@example.com"));
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/event-categories/{eventCategoryId}/enrol", null)).StatusCode);
        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
        Assert.Equal(1, verifyDb.Enrolments.Count(x => x.ParticipantID == participantId));
    }


    private static Task<HttpResponseMessage> Register(HttpClient client, string email, string role) =>
        client.PostAsJsonAsync("/api/auth/register", new { firstName = "Test", lastName = "User", email, password = "RaceDay!2026", role });


    private static async Task<string> Login(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "RaceDay!2026" });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("token").GetString()!;
    }


    private static async Task<string> UserId(TestFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
        return (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync(db.Users, x => x.Email == email)).UserID;
    }
}
