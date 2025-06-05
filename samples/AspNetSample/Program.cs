using AspNetSample;

var host = new WebHostBuilder()
    .UseKestrel()
    .UseContentRoot(Directory.GetCurrentDirectory())
    .UseUrls("http://localhost:5100", "http://localhost:5101", "http://localhost:5102")
    .UseIISIntegration()
    .ConfigureServices(services =>
    {
        services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();
    })
    .ConfigureLogging(logging =>
    {
        //logging.ClearProviders();
        logging.AddConsole();
    })
    .Configure(app =>
    {
        app.Map(
            new PathString("/onboarding"),
            branch => branch.Run(async ctx =>
            {
                await ctx.Response.WriteAsync("Onboarding");
            })
        );

        app.UseMultitenancy<AppTenant>();

        app.Use(async (ctx, next) =>
        {
            if (ctx.GetTenant<AppTenant>().Name == "Default")
            {
                ctx.Response.Redirect("/onboarding");
            }
            else
            {
                await next();
            }
        });

        app.UseMiddleware<LogTenantMiddleware>();
    })
    .Build();

host.Run();