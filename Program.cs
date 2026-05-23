//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.EntityFrameworkCore;
//using UrbanGadgets.Data;
//using UrbanGadgets.Models;
//using Microsoft.AspNetCore.HttpOverrides;


//var builder = WebApplication.CreateBuilder(args);

//AppContext.SetSwitch("System.Net.Dns.UseIpv6", false);

//var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
//builder.WebHost.UseUrls($"http://*:{port}");

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Account/Login";
//        options.AccessDeniedPath = "/Account/AccessDenied";
//        options.ExpireTimeSpan = TimeSpan.FromHours(8);
//        options.SlidingExpiration = true;
//    });


//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        npgsqlOptions =>
//        {
//            npgsqlOptions.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(10),
//                errorCodesToAdd: null);
//        }));

//builder.Services.AddDistributedMemoryCache();




//builder.WebHost.UseUrls($"http://*:{port}");

//// ================= SETTINGS FROM DATABASE =================
////var tempProvider = builder.Services.BuildServiceProvider();

////using (var scope = tempProvider.CreateScope())
////{
////    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

////    var settings = db.AppSettings.FirstOrDefault();

////    int mins = 15;

////    if (settings?.AutoLogout == "5 Minutes") mins = 5;
////    if (settings?.AutoLogout == "15 Minutes") mins = 15;
////    if (settings?.AutoLogout == "30 Minutes") mins = 30;
////    if (settings?.AutoLogout == "Never") mins = 1440;

////    builder.Services.AddSession(options =>
////{
////    options.IdleTimeout = TimeSpan.FromMinutes(15);
////    options.Cookie.HttpOnly = true;
////    options.Cookie.IsEssential = true;
////});
////}
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(15);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//builder.Services.AddAuthorization();

//var app = builder.Build();

//try
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//        if (db.Database.CanConnect())
//        {
//            if (!db.Users.Any())
//            {
//                db.Users.Add(new User
//                {
//                    Username = "admin",
//                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
//                });

//                db.SaveChanges();
//            }
//        }
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine("Seeding failed: " + ex.Message);
//}

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}


//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
//                       ForwardedHeaders.XForwardedProto
//});

//app.UseHttpsRedirection();
//app.UseStaticFiles();


//app.UseRouting();

//app.UseSession();

//app.UseAuthentication();
//app.UseAuthorization();

//app.UseDeveloperExceptionPage();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Account}/{action=Login}/{id?}"); 
//app.Run();

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Dns.UseIpv6", false);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services
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

var app = builder.Build();

// Seed admin user
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Database.CanConnect())
        {
            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    IsActive = true
                });

                db.SaveChanges();
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Seeding failed: " + ex.Message);
}

// Middleware pipeline
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
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();