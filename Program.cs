using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Dns.UseIpv6", false);

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// ================= SERVICES =================

builder.Services.AddControllersWithViews();

// Authentication (IMPORTANT FIX HERE)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";   // FIXED (you are using AuthController)
        options.AccessDeniedPath = "/Auth/Login";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ================= DATABASE INIT =================

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.Migrate();

        // ================= SEED ADMIN (SAFE VERSION) =================
        var superAdmin = db.Users.FirstOrDefault(x => x.Username == "superadmin");

        if (superAdmin == null)
        {
            superAdmin = new User
            {
                Username = "superadmin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"),
                Role = "SuperAdmin",
                IsActive = true,
                BusinessId = null// IMPORTANT: not tied to any business
            };

            db.Users.Add(superAdmin);
        }
        else
        {
            superAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123");
            superAdmin.Role = "SuperAdmin";
            superAdmin.IsActive = true;
            superAdmin.BusinessId = null;
        }


        var user = db.Users.FirstOrDefault(x => x.Username == "admin");

        if (user == null)
        {
            user = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                IsActive = true,
                BusinessId = 1
            };

            db.Users.Add(user);
        }
        else
        {
            user.Role = "Admin";
            user.IsActive = true;

            // ONLY set BusinessId if missing (prevents overwriting real data)
            if (user.BusinessId == 0)
            {
                user.BusinessId = 1;
            }

            // DO NOT reset password every startup (this was breaking login logic)
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
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();