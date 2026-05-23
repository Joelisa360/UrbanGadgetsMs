using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Dns.UseIpv6", false);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

// ================= SERVICES =================

builder.Services.AddControllersWithViews();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Cache
builder.Services.AddDistributedMemoryCache();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthorization();

// ================= BUILD APP =================

var app = builder.Build();

// ================= DATABASE INIT =================

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Apply migrations
        db.Database.Migrate();

        // Load settings
        var settings = db.AppSettings.FirstOrDefault();

        int mins = 15;

        if (settings?.AutoLogout == "5 Minutes")
            mins = 5;

        else if (settings?.AutoLogout == "15 Minutes")
            mins = 15;

        else if (settings?.AutoLogout == "30 Minutes")
            mins = 30;

        else if (settings?.AutoLogout == "Never")
            mins = 1440;

        Console.WriteLine($"Auto logout setting: {mins} minutes");

        // Seed Admin User
        var user = db.Users.FirstOrDefault(x => x.Username == "admin");

        if (user == null)
        {
            user = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                IsActive = true
            };

            db.Users.Add(user);
        }
        else
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            user.Role = "Admin";
            user.IsActive = true;
        }

        db.SaveChanges();
    }
}
catch (Exception ex)
{
    Console.WriteLine("Startup error: " + ex.Message);
}

// ================= PIPELINE =================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// ================= ROUTES =================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ================= RUN =================

app.Run();