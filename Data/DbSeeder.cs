using Microsoft.AspNetCore.Identity;
using VersatileApi.Models;

namespace VersatileApi.Data;

/// <summary>
/// Fills an EMPTY database with the website content (generated from site-data.ts) and creates the first admin.
/// After that, all content is edited from the admin panel.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db, IConfiguration config)
    {
        // ---- Admin user ----
        if (!db.AdminUsers.Any())
        {
            var user = new AdminUser { Username = config["Admin:Username"] ?? "admin" };
            user.PasswordHash = new PasswordHasher<AdminUser>().HashPassword(user, config["Admin:Password"] ?? "Admin@12345");
            db.AdminUsers.Add(user);
        }

        // ---- Single settings ----
        if (!db.SiteSettings.Any())
        {
            db.SiteSettings.AddRange(
                new SiteSetting { Key = "name", Value = "Versatile Support & Supply" },
                new SiteSetting { Key = "tagline", Value = "Your Trusted Partner for Quality & Creativity" },
                new SiteSetting { Key = "footerLine", Value = "Customer Satisfaction Is Our Priority & Your Trust, Our Responsibility" },
                new SiteSetting { Key = "mobile", Value = "+88 01617358383" },
                new SiteSetting { Key = "whatsapp", Value = "+88 01727358383" },
                new SiteSetting { Key = "hours", Value = "" },
                new SiteSetting { Key = "interiorsIntro", Value = "We provide creative, functional, and modern interior design and decoration solutions for residential, commercial, exhibition, and corporate spaces — transforming ordinary spaces into beautiful, comfortable, and productive environments that reflect our clients’ vision and lifestyle." },
                new SiteSetting { Key = "paperTubeIntro", Value = "We offer paper tube products and customized paper tube packaging for different business and product requirements. Discuss your preferred sizes, specifications, printing, and finishing with our team." }
            );
        }

        // ---- Products (groups + products) ----
        if (!db.ProductCategories.Any())
        {
            var cats = new (string Name, string Letter, string Icon, string[] Items)[]
            {
                ("Printing Products", "A", "printer", new string[] {  }),
                ("Packaging Products", "B", "package", new string[] { "Customized Packaging Boxes", "Paper Shopping Bags", "Gift Boxes", "Product Packaging", "Corporate Gift Packaging", "Food Packaging", "Promotional Packaging", "Printed Cartons", "Paper Boxes", "Customized Labels & Tags", "Non-Woven Bag" }),
                ("Paper Tube Products", "C", "scroll", new string[] { "Kraft Paper Tubes", "Customized Paper Tubes", "Printed Paper Tubes", "Cylindrical Packaging Tubes", "Paper Cores & Industrial Tubes (subject to product availability and specifications)" }),
                ("Corporate & Promotional Gift Items", "D", "gift", new string[] { "Corporate Gift Sets", "Customized Pens", "Promotional T-Shirts & Jerseys", "Customized Mugs", "Keychains", "Notebooks & Diaries", "Water Bottles", "Executive Gift Sets", "Office Accessories", "Desk Organizers", "Promotional Bags", "Caps", "Awards & Crests", "Customized Merchandise" }),
                ("Business & Office Accessories", "E", "briefcase", new string[] { "Office Stationery", "Desk Accessories", "Document Holders", "Card Holders", "File Folders", "Desktop Organizers", "Promotional Office Supplies", "Corporate Accessories", "Garments Accessories & All Kinds of Supplies" })
            };

            var order = 1;
            foreach (var c in cats)
            {
                db.ProductCategories.Add(new ProductCategory
                {
                    Name = c.Name,
                    Letter = c.Letter,
                    Icon = c.Icon,
                    SortOrder = order++,
                    Products = c.Items.Select((n, i) => new Product { Name = n, SortOrder = i + 1 }).ToList()
                });
            }
        }

        // ---- Every other list ----
        if (!db.ContentItems.Any())
        {
            void Items(string collection, params (string Title, string? Text, string? Icon, string? Image)[] rows)
            {
                for (var i = 0; i < rows.Length; i++)
                    db.ContentItems.Add(new ContentItem
                    {
                        Collection = collection,
                        Title = rows[i].Title,
                        Text = rows[i].Text,
                        Icon = rows[i].Icon,
                        ImageUrl = rows[i].Image,
                        SortOrder = i + 1
                    });
            }

        Items(Collections.Emails,
            ("mahid.bd@outlook.com", null, null, null),
            ("Versatilebd2000@gmail.com", null, null, null)
        );
        Items(Collections.Addresses,
            ("Office", "House # 568 (G.F), Road # 9, Baitul Aman Housing Society, Adabor, Shyamoli, Dhaka-1207, Bangladesh", null, null),
            ("Office (Narinda)", "30/3/A (1st Floor), Shah Saheb Lane, Narinda, Dhaka-1000, Bangladesh", null, null)
        );
        Items(Collections.Nav,
            ("Home", "/", null, null),
            ("About Us", "/about", null, null),
            ("Products", "/products", null, null),
            ("Services", "/services", null, null),
            ("Industries We Serve", "/industries", null, null),
            ("Why Choose Us", "/why-choose-us", null, null),
            ("Our Work Process", "/work-process", null, null),
            ("Gallery", "/gallery", null, null),
            ("FAQ", "/faq", null, null),
            ("Contact Us", "/contact", null, null)
        );
        Items(Collections.BusinessCategories,
            ("Printing Solutions", "Stationery, marketing materials, catalogues, reports and more.", "printer", null),
            ("Packaging Solutions", "Custom boxes, bags, cartons, labels and gift packaging.", "package", null),
            ("Corporate & Promotional Gifts", "Branded merchandise and executive gift sets.", "gift", null),
            ("Business & Office Accessories", "Stationery, desk accessories and general supplies.", "briefcase", null),
            ("Interior Design & Decoration", "Residential, corporate, retail and exhibition spaces.", "sofa", null),
            ("Paper Tube Solutions", "Kraft, printed and custom paper tubes and cores.", "scroll", null)
        );
        Items(Collections.CoreValues,
            ("Quality", "Consistent standards across every product and service we deliver.", null, null),
            ("Integrity", "Building business relationships through honesty and transparency.", null, null),
            ("Customer Satisfaction", "Understanding and fulfilling customer requirements.", null, null),
            ("Innovation", "Embracing creative ideas and modern solutions.", null, null),
            ("Reliability", "Earning trust through responsible, dependable practice.", null, null),
            ("Professionalism", "Maintaining professional standards in every interaction.", null, null)
        );
        Items(Collections.Services,
            ("Commercial Printing Services", "Printing solutions for businesses, offices, institutions, and individuals.", "printer", null),
            ("Customized Packaging Solutions", "Packaging products designed to support product presentation, branding, and business requirements.", "package", null),
            ("Corporate Gift Solutions", "Gift products suitable for corporate events, employee appreciation, business meetings, and special occasions.", "gift", null),
            ("Promotional Branding Solutions", "Promotional merchandise and customized products to help businesses create brand awareness.", "megaphone", null),
            ("Wholesale Supply", "Product supply solutions for businesses, retailers, resellers, and bulk purchasers.", "factory", null),
            ("Retail Supply", "Products and solutions for individual customers and small-quantity requirements.", "shopping-bag", null),
            ("Customized Order Management", "We welcome inquiries for customized products based on customer specifications, subject to product availability and order requirements.", "clipboard-list", null)
        );
        Items(Collections.InteriorServices,
            ("Exhibition Stall Design & Construction", "Creative stall concepts and display spaces for exhibitions and trade events.", "presentation", null),
            ("Residential Interior Design", "Interior planning and decoration for residential spaces.", "house", null),
            ("Office & Corporate Interior", "Functional and professional work environments for offices and corporate spaces.", "building-2", null),
            ("Commercial Space Decoration", "Interior solutions for commercial environments.", "store", null),
            ("Showroom & Retail Interior", "Attractive and practical spaces for product display and customer experience.", "shopping-bag", null),
            ("Furniture Design & Solutions", "Furniture planning and solutions based on project requirements.", "armchair", null),
            ("Space Planning & Layout", "Organized layouts designed around space use and functionality.", "ruler", null),
            ("Interior Renovation & Fit-Out", "Renovation and fit-out solutions for different interior projects.", "hammer", null),
            ("Customized Interior Solutions", "Interior services tailored to clients’ needs and project specifications.", "sparkles", null)
        );
        Items(Collections.TubeServices,
            ("Customized Paper Tube Products", "Paper tube options for different packaging and business requirements.", "scroll", null),
            ("Paper Tube Printing & Finishing", "Customization options subject to product specifications and availability.", "paintbrush", null)
        );
        Items(Collections.TubeProducts,
            ("Kraft Paper Tubes", null, null, null),
            ("Customized Paper Tubes", null, null, null),
            ("Printed Paper Tubes", null, null, null),
            ("Cylindrical Packaging Tubes", null, null, null),
            ("Paper Cores & Industrial Tubes (subject to availability)", null, null, null),
            ("Customized Paper Tube Packaging", null, null, null)
        );
        Items(Collections.Industries,
            ("Corporate Offices & Organizations", null, "building-2", null),
            ("Banking & Financial Institutions", null, "landmark", null),
            ("Educational Institutions", null, "graduation-cap", null),
            ("Hospitals & Healthcare Organizations", null, "stethoscope", null),
            ("Garments & Textile Industries", null, "shirt", null),
            ("Manufacturing Companies", null, "factory", null),
            ("Retail & E-commerce Businesses", null, "shopping-cart", null),
            ("Event Management Companies", null, "calendar-days", null),
            ("Advertising & Marketing Agencies", null, "trending-up", null),
            ("NGOs & Development Organizations", null, "handshake", null),
            ("Small & Medium Enterprises (SMEs)", null, "store", null),
            ("Individual Customers", null, "user", null)
        );
        Items(Collections.WhyUs,
            ("Wide Product & Service Range", "Multiple categories and services from a single, accountable partner.", "layers", null),
            ("Quality Commitment", "Every order is checked against your expectations and specifications.", "badge-check", null),
            ("Competitive Pricing", "Solutions to suit different budgets and order volumes.", "wallet", null),
            ("Wholesale & Retail", "Serving bulk buyers and individual customers alike.", "shopping-cart", null),
            ("Customized Solutions", "Products and services built around your exact requirements.", "palette", null),
            ("Customer Support", "Professional assistance at every stage of the enquiry and order process.", "headphones", null),
            ("Business Partnership", "We value long-term relationships with customers and business partners.", "handshake", null)
        );
        Items(Collections.Process,
            ("Contact Us", "Share your product or project requirements with our team.", null, null),
            ("Discuss Your Requirements", "Provide product details, project scope, specifications, quantity, and customization preferences.", null, null),
            ("Receive a Quotation", "We will discuss pricing and order or project details based on your requirements.", null, null),
            ("Confirm Your Order", "Once the specifications and quotation are approved, the order or project can be confirmed.", null, null),
            ("Order Processing", "Your confirmed order or project will be processed according to the agreed specifications.", null, null),
            ("Delivery / Project Handover", "Products will be delivered or projects handed over according to the agreed arrangements.", null, null)
        );
        Items(Collections.Gallery,
            ("Printing Collection", null, "printer", null),
            ("Packaging Collection", null, "package", null),
            ("Paper Tube Collection", null, "scroll", null),
            ("Corporate Gift Collection", null, "gift", null),
            ("Promotional Merchandise", null, "shirt", null),
            ("Interior Design Projects", null, "sofa", null),
            ("Exhibition Stall Projects", null, "presentation", null),
            ("Customized Products", null, "sparkles", null),
            ("Corporate Branding", null, "tag", null)
        );
        Items(Collections.Faqs,
            ("What products and services does Versatile Support & Supply offer?", "We offer printing products, packaging solutions, corporate gifts, promotional merchandise, business accessories, paper tube products, and interior design and decoration services.", null, null),
            ("Do you provide both wholesale and retail services?", "Yes. We aim to serve both wholesale buyers and individual retail customers, depending on product availability and order requirements.", null, null),
            ("Do you accept customized orders?", "Yes. We welcome inquiries for customized printing, packaging, paper tube, and promotional gift products.", null, null),
            ("Can corporate organizations place bulk orders?", "Yes. Corporate organizations and businesses can contact us to discuss bulk purchasing requirements and quotations.", null, null),
            ("Do you provide interior design and decoration services?", "Yes. Our interior services include exhibition stalls, residential interiors, office and corporate interiors, commercial spaces, showrooms, space planning, furniture solutions, and renovation and fit-out services.", null, null),
            ("Do you offer customized paper tube products?", "We welcome inquiries for paper tube products and customized packaging requirements. Please contact us with your preferred size, specifications, and quantity.", null, null),
            ("How can I get a quotation?", "You can contact us with your product or project details, quantity, specifications, and customization requirements to request a quotation.", null, null),
            ("Where is your office located?", "Our office is located at 30/3/A (1st Floor), Shah Saheb Lane, Narinda, Dhaka-1000, Bangladesh.", null, null),
            ("How can I place an order?", "Contact us through our website or available communication channels to discuss your requirements and confirm your order.", null, null)
        );
        }

        db.SaveChanges();
    }
}
