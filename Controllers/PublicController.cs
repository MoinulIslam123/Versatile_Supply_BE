using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersatileApi.Data;
using VersatileApi.Models;

namespace VersatileApi.Controllers;

/// <summary>Read-only endpoints used by the public website (no login needed).</summary>
[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly AppDbContext _db;
    public PublicController(AppDbContext db) => _db = db;

    /// <summary>Everything the website shows, in one request.</summary>
    [HttpGet("site")]
    public async Task<IActionResult> Site()
    {
        var items = await _db.ContentItems.AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .ToListAsync();

        var settings = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
        string S(string key) => settings.TryGetValue(key, out var v) ? v : "";

        IEnumerable<ContentItem> Of(string c) => items.Where(i => i.Collection == c);
        var whatsapp = S("whatsapp");
        var digits = new string(whatsapp.Where(char.IsDigit).ToArray());

        return Ok(new
        {
            site = new
            {
                name = S("name"),
                tagline = S("tagline"),
                footerLine = S("footerLine"),
                mobile = S("mobile"),
                whatsapp,
                whatsappLink = "https://wa.me/" + digits,
                hours = S("hours"),
                emails = Of(Collections.Emails).Select(i => i.Title).ToList(),
                addresses = Of(Collections.Addresses).Select(i => new { label = i.Title, text = i.Text ?? "" }).ToList()
            },
            nav = Of(Collections.Nav).Select(i => new { label = i.Title, path = i.Text ?? "/" }).ToList(),
            businessCategories = Of(Collections.BusinessCategories).Select(i => new { icon = i.Icon ?? "", title = i.Title, text = i.Text ?? "" }).ToList(),
            coreValues = Of(Collections.CoreValues).Select(i => new { title = i.Title, text = i.Text ?? "" }).ToList(),
            products = await ProductList(),
            services = Of(Collections.Services).Select(Card).ToList(),
            interiorServices = Of(Collections.InteriorServices).Select(Card).ToList(),
            tubeServices = Of(Collections.TubeServices).Select(Card).ToList(),
            tubeProducts = Of(Collections.TubeProducts).Select(i => i.Title).ToList(),
            industries = Of(Collections.Industries).Select(i => new { icon = i.Icon ?? "", title = i.Title }).ToList(),
            whyUs = Of(Collections.WhyUs).Select(Card).ToList(),
            process = Of(Collections.Process).Select(i => new { title = i.Title, text = i.Text ?? "" }).ToList(),
            gallery = Of(Collections.Gallery).Select(i => new { title = i.Title, icon = i.Icon ?? "", src = Absolute(i.ImageUrl) }).ToList(),
            faqs = Of(Collections.Faqs).Select(i => new { q = i.Title, a = i.Text ?? "" }).ToList(),
            interiorsIntro = S("interiorsIntro"),
            paperTubeIntro = S("paperTubeIntro")
        });
    }

    // ---- older single-purpose endpoints (still available) ----

    [HttpGet("products")]
    public async Task<IActionResult> Products() => Ok(await ProductList());

    [HttpGet("interiors")]
    public async Task<IActionResult> Interiors()
    {
        var services = await _db.ContentItems.AsNoTracking()
            .Where(i => i.Collection == Collections.InteriorServices && i.IsActive)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .Select(i => new { icon = i.Icon ?? "", title = i.Title, text = i.Text ?? "" })
            .ToListAsync();
        return Ok(new { intro = await Setting("interiorsIntro"), services });
    }

    [HttpGet("paper-tube")]
    public async Task<IActionResult> PaperTube()
    {
        var items = await _db.ContentItems.AsNoTracking()
            .Where(i => (i.Collection == Collections.TubeServices || i.Collection == Collections.TubeProducts) && i.IsActive)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .ToListAsync();

        return Ok(new
        {
            intro = await Setting("paperTubeIntro"),
            services = items.Where(i => i.Collection == Collections.TubeServices).Select(Card).ToList(),
            products = items.Where(i => i.Collection == Collections.TubeProducts).Select(i => i.Title).ToList()
        });
    }

    /// <summary>"/uploads/x.jpg" becomes "http://host/uploads/x.jpg" so the website can show it from any address.</summary>
    private string Absolute(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        return url.StartsWith('/') ? $"{Request.Scheme}://{Request.Host}{url}" : url;
    }

    private static object Card(ContentItem i) => new { icon = i.Icon ?? "", title = i.Title, text = i.Text ?? "" };

    private async Task<string> Setting(string key) =>
        (await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key))?.Value ?? "";

    private async Task<List<object>> ProductList()
    {
        var cats = await _db.ProductCategories.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new
            {
                c.Id,
                c.Letter,
                c.Icon,
                Title = c.Name,
                Items = c.Products.Where(p => p.IsActive).OrderBy(p => p.SortOrder).Select(p => p.Name).ToList()
            })
            .ToListAsync();

        return cats.Select(c => (object)new { id = c.Id, letter = c.Letter, icon = c.Icon, title = c.Title, items = c.Items }).ToList();
    }
}
