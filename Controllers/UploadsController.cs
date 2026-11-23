using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VersatileApi.Services;

namespace VersatileApi.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/uploads")]
public class UploadsController : ControllerBase
{
    private readonly UploadStore _store;
    public UploadsController(UploadStore store) => _store = store;

    /// <summary>POST a photo up to 5 MB; compressed and stored under 200 KB.</summary>
    [HttpPost]
    [RequestSizeLimit(UploadStore.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file was sent." });

        if (file.Length > UploadStore.MaxBytes)
            return BadRequest(new { message = "The photo is larger than 15 MB." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!UploadStore.AllowedExtensions.Contains(extension) || !(file.ContentType ?? "").StartsWith("image/"))
            return BadRequest(new { message = "Only JPG, PNG, WEBP or GIF photos are allowed." });

        // If already <= 200 KB, save as-is; otherwise compress down to <= 200 KB
        if (file.Length <= ImageCompressor.MaxTargetBytes)
            return Ok(new { url = await _store.SaveAsync(file) });

        var (compressedBytes, outExtension) = await ImageCompressor.CompressUnder200KbAsync(file, ct);
        return Ok(new { url = await _store.SaveBytesAsync(compressedBytes, outExtension, ct) });
    }
}