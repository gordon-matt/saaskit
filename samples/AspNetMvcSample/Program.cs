using AspNetMvcSample;
using AspNetMvcSample.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

configBuilder.AddEnvironmentVariables();
var configuration = configBuilder.Build();

// Services configuration
builder.Services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();
builder.Services.AddEntityFrameworkSqlite().AddDbContext<SqliteApplicationDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<SqliteApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOptions();
builder.Services.AddControllersWithViews();
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
});
builder.Services.Configure<MultitenancyOptions>(configuration.GetSection("Multitenancy"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseDatabaseErrorPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    try
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        serviceScope.ServiceProvider.GetService<SqlServerApplicationDbContext>()?.Database.Migrate();
    }
    catch { }
}

app.UseStaticFiles();
app.UseMultitenancy<AppTenant>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();