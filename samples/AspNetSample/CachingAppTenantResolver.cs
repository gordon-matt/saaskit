using Microsoft.Extensions.Caching.Memory;
using SaasKit.Multitenancy;

namespace AspNetSample
{
    public class CachingAppTenantResolver : MemoryCacheTenantResolver<AppTenant>
    {
        private readonly Dictionary<string, string> mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "localhost:5100", "Tenant 1"},
            { "localhost:5101", "Tenant 2"},
            { "localhost:5102", "Tenant 3"},
        };

        public CachingAppTenantResolver(IMemoryCache cache, ILoggerFactory loggerFactory)
            : base(cache, loggerFactory)
        {
        }

        protected override string GetContextIdentifier(HttpContext context)
        {
            return context.Request.Host.Value.ToLower();
        }

        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<AppTenant> context)
        {
            return context.Tenant.Hostnames;
        }

        protected override Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<AppTenant> tenantContext = null;

            if (!mappings.TryGetValue(context.Request.Host.Value, out string tenantName))
            {
                // Make sure we always have a default tenant
                tenantName = "Default";
            }

            tenantContext = new TenantContext<AppTenant>(
                    new AppTenant { Name = tenantName, Hostnames = new[] { context.Request.Host.Value } });

            tenantContext.Properties.Add("Created", DateTime.UtcNow);

            return Task.FromResult(tenantContext);
        }
    }
}