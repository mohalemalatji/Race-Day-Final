using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/images")]
public sealed class ImagesController(SqlImageStorage storage) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{container}/{blobName}")]
    public async Task<IActionResult> Get(string container, string blobName, CancellationToken cancellationToken)
    {
        if (!storage.IsValidImageKey(container, blobName))
            return NotFound();

        var image = await storage.DownloadAsync(container, blobName, cancellationToken);
        if (image is null) return NotFound();
        Response.Headers.CacheControl = "public,max-age=3600";
        return File(image.Content, image.ContentType, enableRangeProcessing: true);
    }
}
