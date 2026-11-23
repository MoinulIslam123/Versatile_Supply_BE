namespace VersatileApi.Services;

/// <summary>Saves and deletes uploaded photos in the "uploads" folder (served at /uploads/...).</summary>
public class UploadStore
{
    public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    public const long MaxBytes = 15 * 1024 * 1024; // 15 MB per photo

    public string Folder { get; }

    public UploadStore(IWebHostEnvironment env)
    {
        Folder = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(Folder);
    }

    /// <summary>Saves the file under a random name and returns its address, e.g. /uploads/3f2a....jpg</summary>
    public async Task<string> SaveAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var name = Guid.NewGuid().ToString("N") + extension;

        await using var stream = File.Create(Path.Combine(Folder, name));
        await file.CopyToAsync(stream);
        return "/uploads/" + name;
    }

    /// <summary>Saves compressed image bytes under a random name and returns /uploads/...</summary>
    public async Task<string> SaveBytesAsync(byte[] bytes, string extension, CancellationToken ct = default)
    {
        var normalizedExt = extension.StartsWith('.')
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
        var name = Guid.NewGuid().ToString("N") + normalizedExt;

        await File.WriteAllBytesAsync(Path.Combine(Folder, name), bytes, ct);
        return "/uploads/" + name;
    }

    /// <summary>Deletes the file if the address points into our uploads folder (links to other sites are ignored).</summary>
    public void DeleteIfOurs(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("/uploads/")) return;

        var path = Path.Combine(Folder, Path.GetFileName(url)); // GetFileName strips any ../ tricks
        if (File.Exists(path)) File.Delete(path);
    }
}