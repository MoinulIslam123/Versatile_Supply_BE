using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace VersatileApi.Services;

public static class ImageCompressor
{
    public const int MaxTargetBytes = 200 * 1024; // 200 KB

    public static async Task<(byte[] Bytes, string Extension)> CompressUnder200KbAsync(
        IFormFile file,
        CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream();
        using var image = await Image.LoadAsync(stream, ct);

        image.Mutate(x => x.AutoOrient());

        // Cap initial dimensions at 1600px max side so web images stay sharp yet lightweight
        const int maxDimension = 1600;
        if (image.Width > maxDimension || image.Height > maxDimension)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDimension, maxDimension)
            }));
        }

        var keepWebp = Path.GetExtension(file.FileName).Equals(".webp", StringComparison.OrdinalIgnoreCase);
        using var working = image.Clone(_ => { });

        while (true)
        {
            for (int quality = 82; quality >= 30; quality -= 8)
            {
                await using var ms = new MemoryStream();
                if (keepWebp)
                    await working.SaveAsWebpAsync(ms, new WebpEncoder { Quality = quality }, ct);
                else
                    await working.SaveAsJpegAsync(ms, new JpegEncoder { Quality = quality }, ct);

                if (ms.Length <= MaxTargetBytes)
                    return (ms.ToArray(), keepWebp ? ".webp" : ".jpg");
            }

            if (working.Width <= 400 && working.Height <= 400)
            {
                int fallbackQuality = 24;
                while (true)
                {
                    await using var fallback = new MemoryStream();
                    if (keepWebp)
                        await working.SaveAsWebpAsync(fallback, new WebpEncoder { Quality = fallbackQuality }, ct);
                    else
                        await working.SaveAsJpegAsync(fallback, new JpegEncoder { Quality = fallbackQuality }, ct);

                    if (fallback.Length <= MaxTargetBytes ||
                        (fallbackQuality <= 10 && working.Width <= 200 && working.Height <= 200))
                    {
                        return (fallback.ToArray(), keepWebp ? ".webp" : ".jpg");
                    }

                    if (fallbackQuality > 10)
                    {
                        fallbackQuality -= 4;
                    }
                    else
                    {
                        working.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(
                                Math.Max(200, (int)(working.Width * 0.8)),
                                Math.Max(200, (int)(working.Height * 0.8)))
                        }));
                    }
                }
            }

            working.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size((int)(working.Width * 0.85), (int)(working.Height * 0.85))
            }));
        }
    }
}