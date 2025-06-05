using AspNetMvcAuthSample;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Services configuration
builder.Services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();
builder.Services.AddControllersWithViews(); // Replaces AddMvc()
builder.Services.Configure<MultitenancyOptions>(builder.Configuration.GetSection("Multitenancy"));

// Authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/forbidden";
    // Default cookie name (will be overridden per tenant)
    options.Cookie.Name = "default.AspNet.Cookies";
})
.AddGoogle(options =>
{
    // Placeholder values (will be overridden per tenant)
    options.ClientId = "temp_client_id";
    options.ClientSecret = "temp_client_secret";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseMultitenancy<AppTenant>();

// Per-tenant authentication configuration
app.UsePerTenant<AppTenant>((context, builder) =>
{
    // 1. Configure cookie name per tenant
    var cookieOptions = app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
    cookieOptions.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name = $"{context.Tenant.Id}.AspNet.Cookies";

    // 2. Configure Google auth per tenant
    string googleClientId = app.Configuration[$"{context.Tenant.Id}:GoogleClientId"];
    string googleClientSecret = app.Configuration[$"{context.Tenant.Id}:GoogleClientSecret"];

    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
    {
        // Get the Google options and update them
        var googleOptions = app.Services.GetRequiredService<IOptionsMonitor<GoogleOptions>>();
        googleOptions.Get(GoogleDefaults.AuthenticationScheme).ClientId = googleClientId;
        googleOptions.Get(GoogleDefaults.AuthenticationScheme).ClientSecret = googleClientSecret;
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();