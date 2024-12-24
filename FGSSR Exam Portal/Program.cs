using EMSYS.Data;
using EMSYS.Models;
using EMSYS.Services;
using EMSYS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<EMSYSdbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<AspNetUsers, AspNetRoles>(o =>
{
    o.User.AllowedUserNameCharacters = null;
    o.User.RequireUniqueEmail = true;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.AllowedForNewUsers = true;

}).AddEntityFrameworkStores<EMSYSdbContext>().AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();//include this for the light/dark mode cookie
builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => { options.UseMemberCasing(); });
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<UserProfilePictureActionFilter>();//set ViewBag.Avatar
});

builder.Services.AddScoped<ErrorLoggingService>();
builder.Services.AddScoped<Util>();

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Register the IStringLocalizerFactory service
builder.Services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();

// Set English as the only culture
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
    options.SupportedCultures = new List<System.Globalization.CultureInfo> { new System.Globalization.CultureInfo("en") };
    options.SupportedUICultures = new List<System.Globalization.CultureInfo> { new System.Globalization.CultureInfo("en") };
});

//lowercase routing
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap["hyphenated"] = typeof(HyphenatedRouteConstraint);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use English localization
app.UseRequestLocalization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapRazorPages();

app.Run();
