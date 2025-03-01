using CAPSTONE_TEAM01_2024;
using CAPSTONE_TEAM01_2024.Models; // Make sure to include your model namespace
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Enable Controllers in project
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Session configuration
builder.Services.AddSignalR(); // Register services for SignalR


// Enable Microsoft Authentication
builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["MicrosoftClientID"]!;
    options.ClientSecret = builder.Configuration["MicrosoftSecretID"]!;
});

// Add DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("name=DefaultConnection"));

// Add Identity of user
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Set EPPlus LicenseContext
var licenseContext = builder.Configuration["EPPlus:ExcelPackage:LicenseContext"];
if (!string.IsNullOrEmpty(licenseContext))
{
    ExcelPackage.LicenseContext = licenseContext == "Commercial"
        ? LicenseContext.Commercial
        : LicenseContext.NonCommercial;
}

// Post Configurations
builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
    options =>
    {
        options.LoginPath = "/users/login";
        options.AccessDeniedPath = "/users/login";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapHub<NotificationHub>("/notificationHub"); // Map the hub endpoint
app.UseSession(); // Ensure it's before UseRouting and only called once
app.UseRouting();
// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}").RequireAuthorization();

app.Run();
