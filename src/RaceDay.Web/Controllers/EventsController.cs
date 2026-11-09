using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize]
public sealed class EventsController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? search, string? eventType)
    {
        var result = await api.GetAsync<List<EventDto>>("api/events", Token());
        var page = new EventListPage
        {
            Search = search,
            EventType = eventType,
            Error = result.Error,
            Events = result.Value ?? []
        };
        if (result.Succeeded)
        {
            page.Events = page.Events
                .Where(e => string.IsNullOrWhiteSpace(search) ||
                            e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            e.Location.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(e => string.IsNullOrWhiteSpace(eventType) || e.EventType == eventType)
                .ToList();
        }
        return View(page);
    }


    public async Task<IActionResult> Details(string id)
    {
        var result = await api.GetAsync<List<EventDto>>("api/events", Token());
        if (!result.Succeeded)
            return View(new EventDetailsPage { Error = result.Error });
        var item = result.Value?.SingleOrDefault(e => e.EventID == id);
        if (item is null)
            return NotFound();
        return View(new EventDetailsPage { Event = item });
    }


    [HttpPost, Authorize(Roles = "Participant"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(string eventCategoryId, string eventId)
    {
        var result = await api.PostAsync<EnrolmentDto>(
            $"api/event-categories/{Uri.EscapeDataString(eventCategoryId)}/enrol",
            null, Token());
        TempData[result.Succeeded ? "Notice" : "Error"] =
            result.Succeeded ? "Your enrolment was recorded." : result.Error;
        return RedirectToAction(nameof(Details), new { id = eventId });
    }


    private string? Token() => HttpContext.Session.GetString("RaceDayApiToken");
}
