using Microsoft.Extensions.Caching.Memory;
using SaasKit.Multitenancy;

namespace AspNetStructureMapSample;

public class AppTenantResolver : MemoryCacheTenantResolver<AppTenant>
{
    private readonly Dictionary<string, string> mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "localhost:5400", "Tenant 1"},
        { "localhost:5401", "Tenant 2"},
        { "localhost:5402", "Tenant 3"},
    };

    public AppTenantResolver(IMemoryCache cache, ILoggerFactory loggerFactory)
        : base(cache, loggerFactory)
    {
    }

    protected override string GetContextIdentifier(HttpContext context) => context.Request.Host.Value.ToLower();

    protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<AppTenant> context) => context.Tenant.Hostnames;

    protected override Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
    {
        TenantContext<AppTenant> tenantContext = null;

        if (mappings.TryGetValue(context.Request.Host.Value, out string tenantName))
        {
            tenantContext = new TenantContext<AppTenant>(
                new AppTenant { Name = tenantName, Hostnames = new[] { context.Request.Host.Value } });
        }

        return Task.FromResult(tenantContext);
    }

    protected override MemoryCacheEntryOptions CreateCacheEntryOptions() => base.CreateCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));
}