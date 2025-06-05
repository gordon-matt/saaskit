using SaasKit.Multitenancy;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Multitenant extensions for <see cref="HttpContext"/>.
    /// </summary>
    public static class MultitenancyHttpContextExtensions
    {
        private const string TenantContextKey = "saaskit.TenantContext";

        public static void SetTenantContext<TTenant>(this HttpContext context, TenantContext<TTenant> tenantContext)
        {
            Ensure.Argument.NotNull(context, nameof(context));
            Ensure.Argument.NotNull(tenantContext, nameof(tenantContext));

            context.Items[TenantContextKey] = tenantContext;
        }

        public static TenantContext<TTenant> GetTenantContext<TTenant>(this HttpContext context)
        {
            Ensure.Argument.NotNull(context, nameof(context));

            return context.Items.TryGetValue(TenantContextKey, out object tenantContext) ? tenantContext as TenantContext<TTenant> : null;
        }

        public static TTenant GetTenant<TTenant>(this HttpContext context)
        {
            Ensure.Argument.NotNull(context, nameof(context));

            var tenantContext = GetTenantContext<TTenant>(context);

            return tenantContext != null ? tenantContext.Tenant : default;
        }
    }
}