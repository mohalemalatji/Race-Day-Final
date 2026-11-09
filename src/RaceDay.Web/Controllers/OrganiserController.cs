using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public sealed class OrganiserController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = await api.GetAsync<List<EventDto>>("api/events", Token());
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(new OrganiserHomePage
        {
            Error = result.Error,
            Events = result.Value?.Where(e => e.OrganiserID == id).ToList() ?? []
        });
    }


    public IActionResult CreateEvent() => View("EventForm", new EventFormPage());


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormPage page)
    {
        if (!ModelState.IsValid)
            return View("EventForm", page);

        var result = await api.PostAsync<EventDto>("api/events", new EventCreateRequest(
            page.EventName, page.Description, page.EventDate, page.Location, page.DistanceKm, page.EventType), Token());
        if (!result.Succeeded)
        {
            ViewData["ApiError"] = result.Error;
            return View("EventForm", page);
        }

        if (page.BannerImage is { Length: > 0 })
        {
            var upload = await api.PostFileAsync<EventDto>(
                $"api/events/{Uri.EscapeDataString(result.Value!.EventID)}/banner", page.BannerImage, Token());
            TempData[upload.Succeeded ? "Notice" : "Error"] = upload.Succeeded
                ? "Event and banner created."
                : $"Event created, but the banner upload failed: {upload.Error}";
        }
        else
        {
            TempData["Notice"] = "Event created.";
        }
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> EditEvent(string id)
    {
        var item = await FindOwnedEventAsync(id);
        if (item is null)
            return NotFound();
        return View("EventForm", new EventFormPage
        {
            EventID = item.EventID,
            EventName = item.EventName,
            Description = item.Description,
            EventDate = item.EventDate,
            Location = item.Location,
            DistanceKm = item.DistanceKm,
            EventType = item.EventType,
            Status = item.Status,
            BannerImageUrl = item.BannerImageUrl
        });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(string id, EventFormPage page)
    {
        page.EventID = id;
        if (!ModelState.IsValid)
        {
            page.BannerImageUrl = (await FindOwnedEventAsync(id))?.BannerImageUrl;
            return View("EventForm", page);
        }

        var result = await api.PutAsync<EventDto>($"api/events/{Uri.EscapeDataString(id)}",
            new EventUpdateRequest(page.EventName, page.Description, page.EventDate, page.Location,
                page.DistanceKm, page.EventType, page.Status), Token());
        if (!result.Succeeded)
        {
            ViewData["ApiError"] = result.Error;
            page.BannerImageUrl = (await FindOwnedEventAsync(id))?.BannerImageUrl;
            return View("EventForm", page);
        }

        if (page.BannerImage is { Length: > 0 })
        {
            var upload = await api.PostFileAsync<EventDto>(
                $"api/events/{Uri.EscapeDataString(id)}/banner", page.BannerImage, Token());
            if (!upload.Succeeded)
            {
                TempData["Error"] = $"Event details were saved, but the banner upload failed: {upload.Error}";
                return RedirectToAction(nameof(EditEvent), new { id });
            }
        }

        TempData["Notice"] = page.BannerImage is { Length: > 0 } ? "Event and banner updated." : "Event updated.";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var result = await api.DeleteAsync($"api/events/{Uri.EscapeDataString(id)}", Token());
        TempData[result.Succeeded ? "Notice" : "Error"] =
            result.Succeeded ? "Event deleted." : result.Error;
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Categories(string id)
    {
        var item = await FindOwnedEventAsync(id);
        if (item is null)
            return NotFound();

        var categories = await api.GetAsync<List<CategoryDto>>("api/categories", Token());
        return View(new CategoryPage { Event = item, Categories = categories.Value ?? [], Error = categories.Error });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string id, CategoryCreatePage page)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a category name.";
            return RedirectToAction(nameof(Categories), new { id });
        }
        var result = await api.PostAsync<CategoryDto>("api/categories",
            new CategoryCreateRequest(page.CategoryName, page.Description), Token());
        TempData[result.Succeeded ? "Notice" : "Error"] =
            result.Succeeded ? "Category created. You can now add it to the event." : result.Error;
        return RedirectToAction(nameof(Categories), new { id });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkCategory(string id, CategoryLinkPage page)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Choose a category and enter a valid fee and capacity.";
            return RedirectToAction(nameof(Categories), new { id });
        }
        var result = await api.PostAsync<EventCategoryDto>(
            $"api/events/{Uri.EscapeDataString(id)}/categories",
            new EventCategoryRequest(page.CategoryID, page.EntryFee, page.MaxParticipants), Token());
        TempData[result.Succeeded ? "Notice" : "Error"] =
            result.Succeeded ? "Category added to the event." : result.Error;
        return RedirectToAction(nameof(Categories), new { id });
    }


    public async Task<IActionResult> Enrolments(string id)
    {
        var item = await FindOwnedEventAsync(id);
        if (item is null)
            return NotFound();

        var result = await api.GetAsync<List<EnrolmentDto>>(
            $"api/events/{Uri.EscapeDataString(id)}/enrolments", Token());
        return View(new EnrolmentsPage
        {
            EventName = item.EventName,
            Enrolments = result.Value ?? [],
            Error = result.Error
        });
    }


    public async Task<IActionResult> RecordResult(string id, string enrolmentId)
    {
        var item = await FindOwnedEventAsync(id);
        if (item is null)
            return NotFound();

        var result = await api.GetAsync<List<EnrolmentDto>>(
            $"api/events/{Uri.EscapeDataString(id)}/enrolments", Token());
        if (!result.Succeeded)
        {
            ViewData["ApiError"] = result.Error;
            return View(new ResultPage { EventID = id });
        }
        var enrolment = result.Value?.SingleOrDefault(x => x.EnrolmentID == enrolmentId);
        if (enrolment is null)
            return NotFound();
        return View(new ResultPage
        {
            EventID = item.EventID,
            EnrolmentID = enrolment.EnrolmentID,
            EventName = enrolment.EventName,
            ParticipantName = enrolment.ParticipantName,
            CategoryName = enrolment.CategoryName
        });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(string id, string enrolmentId, ResultPage page)
    {
        page.EventID = id;
        page.EnrolmentID = enrolmentId;
        if (!ModelState.IsValid)
            return View(page);
        var result = await api.PostAsync<System.Text.Json.JsonElement>(
            $"api/enrolments/{Uri.EscapeDataString(enrolmentId)}/result",
            new ResultRequest(page.FinishTime, page.Position, page.ResultStatus), Token());
        if (!result.Succeeded)
        {
            ViewData["ApiError"] = result.Error;
            return View(page);
        }
        TempData["Notice"] = "Result recorded.";
        return RedirectToAction(nameof(Enrolments), new { id });
    }


    private async Task<EventDto?> FindOwnedEventAsync(string id)
    {
        var result = await api.GetAsync<List<EventDto>>("api/events", Token());
        var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return result.Value?.SingleOrDefault(e => e.EventID == id && e.OrganiserID == organiserId);
    }


    private string? Token() => HttpContext.Session.GetString("RaceDayApiToken");
}
