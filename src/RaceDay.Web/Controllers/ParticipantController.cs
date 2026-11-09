using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public sealed class ParticipantController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Enrolments()
    {
        var result = await api.GetAsync<List<EnrolmentDto>>("api/enrolments/my-enrolments", Token());
        return View(new EnrolmentsPage { Enrolments = result.Value ?? [], Error = result.Error });
    }


    public async Task<IActionResult> Results()
    {
        var result = await api.GetAsync<List<EnrolmentDto>>("api/results/my-results", Token());
        return View(new EnrolmentsPage { Enrolments = result.Value ?? [], Error = result.Error });
    }


    private string? Token() => HttpContext.Session.GetString("RaceDayApiToken");
}
