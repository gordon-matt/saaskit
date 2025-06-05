using SaasKit.Multitenancy;

namespace AspNetSample
{
    public class AppTenantResolver : ITenantResolver<AppTenant>
    {
        private readonly Dictionary<string, string> mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "localhost:5100", "Tenant 1"},
            { "localhost:5101", "Tenant 2"},
            { "localhost:5102", "Tenant 3"},
        };

        public Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<AppTenant> tenantContext = null;

            if (mappings.TryGetValue(context.Request.Host.Value, out string tenantName))
            {
                tenantContext = new TenantContext<AppTenant>(
                    new AppTenant { Name = tenantName, Hostnames = new[] { context.Request.Host.Value } });

                tenantContext.Properties.Add("Created", DateTime.UtcNow);
            }

            return Task.FromResult(tenantContext);
        }
    }
}