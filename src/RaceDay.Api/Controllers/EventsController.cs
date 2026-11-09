using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Authorize, Route("api/events")]
public sealed class EventsController(RaceDayDbContext db, SqlImageStorage storage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll()
    {
        var query = db.Events.AsQueryable();
        if (User.IsInRole("Organiser"))
            query = query.Where(e => e.OrganiserID == ActorId());
        else
            query = query.Where(e => e.Status == "Upcoming" && e.EventDate >= DateOnly.FromDateTime(DateTime.UtcNow));

        var events = await query
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Enrolments)
            .OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(ToResponse));
    }


    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(EventCreateRequest request)
    {
        var item = new Event
        {
            OrganiserID = ActorId(), EventName = request.EventName.Trim(), Description = request.Description?.Trim(),
            EventDate = request.EventDate, Location = request.Location.Trim(), DistanceKm = request.DistanceKm,
            EventType = request.EventType
        };
        db.Events.Add(item); await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToResponse(item));
    }


    [Authorize(Roles = "Organiser")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, EventUpdateRequest request)
    {
        var item = await db.Events.FindAsync(id);
        if (item is null) return NotFound();
        if (item.OrganiserID != ActorId()) return Forbid();
        item.EventName = request.EventName.Trim(); item.Description = request.Description?.Trim(); item.EventDate = request.EventDate;
        item.Location = request.Location.Trim(); item.DistanceKm = request.DistanceKm; item.EventType = request.EventType; item.Status = request.Status;
        await db.SaveChangesAsync(); return Ok(ToResponse((await LoadEvent(id))!));
    }


    [Authorize(Roles = "Organiser")]
    [HttpPost("{id}/banner"), Consumes("multipart/form-data"), RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<ActionResult<EventResponse>> UploadBanner(string id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (!await SqlImageStorage.IsSupportedImageAsync(file, cancellationToken))
            return BadRequest(new { message = "Choose a JPEG, PNG, or WebP image up to 5 MB." });

        var item = await db.Events.FindAsync(id);
        if (item is null) return NotFound();
        if (item.OrganiserID != ActorId()) return Forbid();

        var oldBlobName = item.BannerBlobName;
        item.BannerBlobName = await storage.UploadAsync(file!, "event-banners", cancellationToken);
        await db.SaveChangesAsync();
        await storage.DeleteIfExistsAsync("event-banners", oldBlobName, cancellationToken);
        return Ok(ToResponse(await LoadEvent(id) ?? item));
    }


    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await db.Events.FindAsync(id);
        if (item is null) return NotFound();
        if (item.OrganiserID != ActorId()) return Forbid();
        if (await db.Enrolments.AnyAsync(x => x.EventCategory!.EventID == id)) return Conflict(new { message = "An event with enrolments cannot be deleted." });
        db.Events.Remove(item); await db.SaveChangesAsync();
        await storage.DeleteIfExistsAsync("event-banners", item.BannerBlobName, HttpContext.RequestAborted);
        return NoContent();
    }


    [Authorize(Roles = "Organiser")]
    [HttpPost("{id}/categories")]
    public async Task<ActionResult<EventCategoryResponse>> AddCategory(string id, EventCategoryRequest request)
    {
        var item = await db.Events.FindAsync(id);
        if (item is null) return NotFound();
        if (item.OrganiserID != ActorId()) return Forbid();
        var category = await db.Categories.FindAsync(request.CategoryID);
        if (category is null) return BadRequest(new { message = "Category does not exist." });
        if (await db.EventCategories.AnyAsync(x => x.EventID == id && x.CategoryID == request.CategoryID)) return Conflict(new { message = "This category is already linked to the event." });
        var link = new EventCategory { EventID = id, CategoryID = request.CategoryID, EntryFee = request.EntryFee, MaxParticipants = request.MaxParticipants };
        db.EventCategories.Add(link); await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, new EventCategoryResponse(link.EventCategoryID, category.CategoryID, category.CategoryName, link.EntryFee, link.MaxParticipants, 0));
    }


    [Authorize(Roles = "Organiser")]
    [HttpGet("{id}/enrolments")]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetEventEnrolments(string id)
    {
        var item = await db.Events.FindAsync(id);
        if (item is null) return NotFound();
        if (item.OrganiserID != ActorId()) return Forbid();
        var rows = await db.Enrolments.Include(x => x.Participant).Include(x => x.Result)
            .Include(x => x.EventCategory).ThenInclude(x => x!.Category)
            .Include(x => x.EventCategory).ThenInclude(x => x!.Event)
            .Where(x => x.EventCategory!.EventID == id).OrderBy(x => x.EnrolmentDate).ToListAsync();
        return Ok(rows.Select(EnrolmentsController.ToResponse));
    }


    private string ActorId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


    private Task<Event?> LoadEvent(string id) => db.Events.Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
        .Include(e => e.EventCategories).ThenInclude(ec => ec.Enrolments).SingleOrDefaultAsync(e => e.EventID == id);


    private EventResponse ToResponse(Event e)
    {
        var bannerUrl = string.IsNullOrWhiteSpace(e.BannerBlobName)
            ? null
            : $"{Request.Scheme}://{Request.Host}/api/images/event-banners/{Uri.EscapeDataString(e.BannerBlobName)}";
        return new EventResponse(e.EventID, e.EventName, e.Description, e.EventDate, e.Location, e.DistanceKm,
            e.EventType, e.Status, e.OrganiserID, e.EventCategories.Select(ec => new EventCategoryResponse(ec.EventCategoryID, ec.CategoryID,
                ec.Category?.CategoryName ?? "", ec.EntryFee, ec.MaxParticipants, ec.Enrolments.Count)).ToList(), bannerUrl);
    }
}
