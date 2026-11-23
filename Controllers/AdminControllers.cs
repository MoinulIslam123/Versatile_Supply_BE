using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersatileApi.Data;
using VersatileApi.Models;
using VersatileApi.Services;

namespace VersatileApi.Controllers;

/// <summary>Generic list / get / create / update / delete used by the admin tables.</summary>
[ApiController]
[Authorize]
public abstract class AdminCrudController<T> : ControllerBase where T : class, IEntity
{
    protected readonly AppDbContext Db;
    protected AdminCrudController(AppDbContext db) => Db = db;

    protected virtual IQueryable<T> Query() => Db.Set<T>().AsNoTracking();

    /// <summary>Return an error message to reject the request, or null when it is fine.</summary>
    protected virtual string? Validate(T model) => null;

    /// <summary>Called just before an existing row is overwritten / removed (used to clean up uploaded files).</summary>
    protected virtual void BeforeUpdate(T existing, T model) { }
    protected virtual void BeforeDelete(T existing) { }

    [HttpGet]
    public async Task<ActionResult<List<T>>> GetAll() =>
        await Query().OrderBy(e => EF.Property<int>(e, "SortOrder")).ThenBy(e => e.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<T>> Get(int id)
    {
        var item = await Db.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<T>> Create(T model)
    {
        var error = Validate(model);
        if (error is not null) return BadRequest(new { message = error });

        model.Id = 0;
        Db.Set<T>().Add(model);
        await Db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<T>> Update(int id, T model)
    {
        var error = Validate(model);
        if (error is not null) return BadRequest(new { message = error });

        var existing = await Db.Set<T>().FindAsync(id);
        if (existing is null) return NotFound();

        BeforeUpdate(existing, model);
        model.Id = id;
        Db.Entry(existing).CurrentValues.SetValues(model);
        await Db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await Db.Set<T>().FindAsync(id);
        if (existing is null) return NotFound();

        BeforeDelete(existing);
        Db.Set<T>().Remove(existing);
        await Db.SaveChangesAsync();
        return NoContent();
    }
}

[Route("api/admin/product-categories")]
public class ProductCategoriesController : AdminCrudController<ProductCategory>
{
    public ProductCategoriesController(AppDbContext db) : base(db) { }
}

[Route("api/admin/products")]
public class ProductsController : AdminCrudController<Product>
{
    public ProductsController(AppDbContext db) : base(db) { }

    // GET api/admin/products?categoryId=3
    protected override IQueryable<Product> Query()
    {
        var q = base.Query();
        return int.TryParse(Request.Query["categoryId"], out var categoryId) ? q.Where(p => p.CategoryId == categoryId) : q;
    }
}

/// <summary>All the generic website lists: GET api/admin/items?collection=services</summary>
[Route("api/admin/items")]
public class ContentItemsController : AdminCrudController<ContentItem>
{
    private readonly UploadStore _uploads;

    public ContentItemsController(AppDbContext db, UploadStore uploads) : base(db) => _uploads = uploads;

    // when a gallery photo is replaced or its item deleted, remove the old file from disk
    protected override void BeforeUpdate(ContentItem existing, ContentItem model)
    {
        if (existing.ImageUrl != model.ImageUrl) _uploads.DeleteIfOurs(existing.ImageUrl);
    }

    protected override void BeforeDelete(ContentItem existing) => _uploads.DeleteIfOurs(existing.ImageUrl);

    protected override IQueryable<ContentItem> Query()
    {
        var q = base.Query();
        string? collection = Request.Query["collection"];
        return string.IsNullOrWhiteSpace(collection) ? q : q.Where(i => i.Collection == collection);
    }

    protected override string? Validate(ContentItem model) =>
        Collections.All.Contains(model.Collection) ? null : $"Unknown collection '{model.Collection}'.";
}

public record SettingUpdate(string Value);

[ApiController]
[Authorize]
[Route("api/admin/settings")]
public class SettingsController : ControllerBase
{
    private static readonly string[] AllowedKeys =
        { "name", "tagline", "footerLine", "mobile", "whatsapp", "hours", "interiorsIntro", "paperTubeIntro" };

    private readonly AppDbContext _db;
    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<Dictionary<string, string>>> GetAll() =>
        await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);

    [HttpPut("{key}")]
    public async Task<IActionResult> Put(string key, SettingUpdate body)
    {
        if (!AllowedKeys.Contains(key)) return NotFound();

        var row = await _db.SiteSettings.FindAsync(key);
        if (row is null) _db.SiteSettings.Add(new SiteSetting { Key = key, Value = body.Value ?? "" });
        else row.Value = body.Value ?? "";

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
