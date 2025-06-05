using AspNetStructureMapSample;
using StructureMap;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// Configure services
builder.Services.AddMultitenancy<AppTenant, AppTenantResolver>();
builder.Services.AddLogging(logging => logging.AddConsole());

// Replace the default DI container with StructureMap
builder.Host
    .UseServiceProviderFactory(new StructureMapServiceProviderFactory(null))
    .ConfigureContainer<Registry>(registry =>
    {
        registry.ConfigureTenants<AppTenant>(c =>
        // Tenant Scoped Services
        c.For<IMessageService>().Singleton().Use<MessageService>());
    });

// If you still want to use the specific URL (optional)
builder.WebHost.UseUrls("http://localhost:5400", "http://localhost:5401", "http://localhost:5402");

var app = builder.Build();

// Middleware pipeline
app.UseMultitenancy<AppTenant>();
app.UseTenantContainers<AppTenant>();
app.UseMiddleware<LogTenantMiddleware>();

await app.RunAsync();




//var host = new WebHostBuilder()
//    .UseKestrel()
//    .UseContentRoot(Directory.GetCurrentDirectory())
//    .UseUrls("http://localhost:5100", "http://localhost:5101", "http://localhost:5102")
//    .UseIISIntegration()
//    .ConfigureServices(services => services.AddMultitenancy<AppTenant, AppTenantResolver>())
//    .ConfigureLogging(logging =>
//        //logging.ClearProviders();
//        logging.AddConsole())
//    .Configure(app =>
//    {
//        app.Map(
//            new PathString("/onboarding"),
//            branch => branch.Run(async ctx => await ctx.Response.WriteAsync("Onboarding"))
//        );

//        app.UseMultitenancy<AppTenant>();

//        app.Use(async (ctx, next) =>
//        {
//            if (ctx.GetTenant<AppTenant>().Name == "Default")
//            {
//                ctx.Response.Redirect("/onboarding");
//            }
//            else
//            {
//                await next();
//            }
//        });

//        app.UseMiddleware<LogTenantMiddleware>();
//    })
//    .Build();

//host.Run();