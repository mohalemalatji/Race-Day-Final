using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Authorize, Route("api")]
public sealed class EnrolmentsController(RaceDayDbContext db) : ControllerBase
{
    [Authorize(Roles = "Participant")]
    [HttpPost("event-categories/{id}/enrol")]
    public async Task<ActionResult<EnrolmentResponse>> Enrol(string id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var category = await db.EventCategories.Include(x => x.Event).SingleOrDefaultAsync(x => x.EventCategoryID == id);
        if (category is null) return NotFound();
        if (category.Event!.Status != "Upcoming" || category.Event.EventDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return Conflict(new { message = "This event is not open for enrolment." });
        var participantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (await db.Enrolments.AnyAsync(x => x.ParticipantID == participantId && x.EventCategoryID == id)) return Conflict(new { message = "You are already enrolled in this category." });
        var count = await db.Enrolments.CountAsync(x => x.EventCategoryID == id);
        if (count >= category.MaxParticipants) return Conflict(new { message = "This event category is full." });
        var enrolment = new Enrolment { ParticipantID = participantId, EventCategoryID = id };
        db.Enrolments.Add(enrolment); await db.SaveChangesAsync(); await transaction.CommitAsync();
        var loaded = await LoadEnrolment(enrolment.EnrolmentID);
        return StatusCode(StatusCodes.Status201Created, ToResponse(loaded!));
    }


    [Authorize(Roles = "Participant")]
    [HttpGet("enrolments/my-enrolments")]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetMine()
    {
        var participantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rows = await db.Enrolments.Include(x => x.Participant).Include(x => x.Result)
            .Include(x => x.EventCategory).ThenInclude(x => x!.Category)
            .Include(x => x.EventCategory).ThenInclude(x => x!.Event)
            .Where(x => x.ParticipantID == participantId).OrderByDescending(x => x.EnrolmentDate).ToListAsync();
        return Ok(rows.Select(ToResponse));
    }


    [Authorize(Roles = "Participant")]
    [HttpGet("results/my-results")]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetMyResults()
    {
        var participantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rows = await db.Enrolments.Include(x => x.Participant).Include(x => x.Result)
            .Include(x => x.EventCategory).ThenInclude(x => x!.Category)
            .Include(x => x.EventCategory).ThenInclude(x => x!.Event)
            .Where(x => x.ParticipantID == participantId && x.Result != null).OrderByDescending(x => x.EventCategory!.Event!.EventDate).ToListAsync();
        return Ok(rows.Select(ToResponse));
    }


    [Authorize(Roles = "Organiser")]
    [HttpPost("enrolments/{id}/result")]
    public async Task<IActionResult> SaveResult(string id, ResultRequest request)
    {
        var enrolment = await db.Enrolments.Include(x => x.EventCategory).ThenInclude(x => x!.Event)
            .Include(x => x.Result).SingleOrDefaultAsync(x => x.EnrolmentID == id);
        if (enrolment is null) return NotFound();
        if (enrolment.EventCategory!.Event!.OrganiserID != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        if (enrolment.Result is not null) return Conflict(new { message = "A result has already been recorded for this enrolment." });
        if (request.FinishTime is { } time && (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))) return BadRequest(new { message = "Finish time must be between 00:00:00 and 23:59:59." });
        db.Results.Add(new Result { EnrolmentID = id, FinishTime = request.FinishTime, Position = request.Position, ResultStatus = request.ResultStatus.Trim() });
        enrolment.Status = "Completed"; await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, new { enrolment.EnrolmentID, request.FinishTime, request.Position, request.ResultStatus });
    }


    private Task<Enrolment?> LoadEnrolment(string id) => db.Enrolments.Include(x => x.Participant).Include(x => x.Result)
        .Include(x => x.EventCategory).ThenInclude(x => x!.Category)
        .Include(x => x.EventCategory).ThenInclude(x => x!.Event).SingleOrDefaultAsync(x => x.EnrolmentID == id);


    internal static EnrolmentResponse ToResponse(Enrolment x) => new(x.EnrolmentID, x.EventCategory!.EventID,
        x.EventCategory.Event?.EventName ?? "", x.EventCategory.Category?.CategoryName ?? "", x.EnrolmentDate,
        x.Status, x.ParticipantID, $"{x.Participant?.FirstName} {x.Participant?.LastName}".Trim(), x.Result?.FinishTime, x.Result?.Position, x.Result?.ResultStatus);
}
