using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services;

public sealed record StoredImage(Stream Content, string ContentType);

public sealed class SqlImageStorage(RaceDayDbContext db)
{
    private const long MaxImageLength = 5 * 1024 * 1024;

    public static async Task<bool> IsSupportedImageAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > MaxImageLength)
            return false;

        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = 0;
        while (bytesRead < header.Length)
        {
            var count = await stream.ReadAsync(header.AsMemory(bytesRead), cancellationToken);
            if (count == 0) break;
            bytesRead += count;
        }

        return file.ContentType switch
        {
            "image/jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => bytesRead >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }


    public async Task<string> UploadAsync(IFormFile file, string containerName, CancellationToken cancellationToken)
    {
        var prefix = GetPrefix(containerName);
        var extension = file.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Only JPEG, PNG, or WebP images are supported.")
        };

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var imageKey = $"{prefix}-{Guid.NewGuid():N}{extension}";
        db.ImageAssets.Add(new ImageAsset
        {
            ImageKey = imageKey,
            ContainerName = containerName,
            ContentType = file.ContentType,
            Data = memory.ToArray()
        });
        await db.SaveChangesAsync(cancellationToken);
        return imageKey;
    }


    public async Task<StoredImage?> DownloadAsync(string containerName, string imageKey, CancellationToken cancellationToken)
    {
        if (!IsValidImageKey(containerName, imageKey))
            return null;

        var image = await db.ImageAssets.AsNoTracking()
            .SingleOrDefaultAsync(x => x.ImageKey == imageKey && x.ContainerName == containerName, cancellationToken);
        return image is null ? null : new StoredImage(new MemoryStream(image.Data, writable: false), image.ContentType);
    }


    public async Task DeleteIfExistsAsync(string containerName, string? imageKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageKey) || !IsValidImageKey(containerName, imageKey))
            return;

        var image = await db.ImageAssets.SingleOrDefaultAsync(
            x => x.ImageKey == imageKey && x.ContainerName == containerName, cancellationToken);
        if (image is null)
            return;

        db.ImageAssets.Remove(image);
        await db.SaveChangesAsync(cancellationToken);
    }


    public bool IsValidImageKey(string containerName, string imageKey)
    {
        try
        {
            var prefix = GetPrefix(containerName);
            var fileName = Path.GetFileName(imageKey);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var publicId = Path.GetFileNameWithoutExtension(fileName);
            return fileName == imageKey && (extension is ".jpg" or ".png" or ".webp") &&
                publicId.StartsWith(prefix + "-", StringComparison.Ordinal) &&
                Guid.TryParseExact(publicId[(prefix.Length + 1)..], "N", out _);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }


    private static string GetPrefix(string containerName) => containerName switch
    {
        "event-banners" => "event-banners",
        "profile-pictures" => "profile-pictures",
        _ => throw new ArgumentException("The image type is invalid.", nameof(containerName))
    };
}
