using AspNetStructureMapSample;

public class LogTenantMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger log;

    public LogTenantMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        this.next = next;
        this.log = loggerFactory.CreateLogger<LogTenantMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        var tenant = context.GetTenant<AppTenant>();

        if (tenant != null)
        {
            var messageService = context.RequestServices.GetRequiredService<IMessageService>();
            await context.Response.WriteAsync(
                messageService.Format($"Tenant \"{tenant.Name}\"."));
        }
        else
        {
            await context.Response.WriteAsync("No matching tenant found.");
        }
    }
}