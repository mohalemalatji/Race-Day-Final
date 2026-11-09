using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Authorize, Route("api/categories")]
public sealed class CategoriesController(RaceDayDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll() =>
        Ok(await db.Categories.OrderBy(c => c.CategoryName).Select(c => new CategoryResponse(c.CategoryID, c.CategoryName, c.Description)).ToListAsync());


    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request)
    {
        if (await db.Categories.AnyAsync(c => c.CategoryName == request.CategoryName.Trim())) return Conflict(new { message = "That category already exists." });
        var category = new Category { CategoryName = request.CategoryName.Trim(), Description = request.Description?.Trim() };
        db.Categories.Add(category); await db.SaveChangesAsync();
        var response = new CategoryResponse(category.CategoryID, category.CategoryName, category.Description);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
