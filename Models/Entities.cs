using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VersatileApi.Models;

public interface IEntity
{
    int Id { get; set; }
}

/// <summary>Names of the content lists that can be edited in the admin panel (one row per card / link / FAQ ...).</summary>
public static class Collections
{
    public const string Nav = "nav";
    public const string Emails = "emails";
    public const string Addresses = "addresses";
    public const string BusinessCategories = "business-categories";
    public const string CoreValues = "core-values";
    public const string Services = "services";
    public const string InteriorServices = "interior-services";
    public const string TubeServices = "tube-services";
    public const string TubeProducts = "tube-products";
    public const string Industries = "industries";
    public const string WhyUs = "why-us";
    public const string Process = "process";
    public const string Gallery = "gallery";
    public const string Faqs = "faqs";

    public static readonly string[] All =
    {
        Nav, Emails, Addresses, BusinessCategories, CoreValues, Services, InteriorServices,
        TubeServices, TubeProducts, Industries, WhyUs, Process, Gallery, Faqs
    };
}

/// <summary>A product group shown as a tab on the Products page (A. Printing Products ...).</summary>
public class ProductCategory : IEntity
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = "";

    [Required, MaxLength(5)]
    public string Letter { get; set; } = "";

    [MaxLength(50)]
    public string Icon { get; set; } = "package";

    public int SortOrder { get; set; }

    [JsonIgnore]
    public List<Product> Products { get; set; } = new();
}

/// <summary>One product line inside a category.</summary>
public class Product : IEntity
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [Required, MaxLength(250)]
    public string Name { get; set; } = "";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ProductCategory? Category { get; set; }
}

/// <summary>
/// One generic row used by every other list on the website.
/// Title = main text, Text = description / answer / page link, Icon = icon name, ImageUrl = gallery photo.
/// </summary>
public class ContentItem : IEntity
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Collection { get; set; } = "";

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    [MaxLength(2000)]
    public string? Text { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>Single values: company name, tagline, phone numbers, footer line ...</summary>
public class SiteSetting
{
    [Key, MaxLength(100)]
    public string Key { get; set; } = "";

    public string Value { get; set; } = "";
}

public class AdminUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";
}
