using Microsoft.AspNetCore.Mvc.Razor;

namespace AspNetMvcSample
{
    public class TenantViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "theme", TENANT_KEY = "tenant";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values[THEME_KEY]
                = context.ActionContext.HttpContext.GetTenant<AppTenant>()?.Theme;

            context.Values[TENANT_KEY]
                = context.ActionContext.HttpContext.GetTenant<AppTenant>()?.Name.Replace(" ", "-");
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                IEnumerable<string> themeLocations = new[]
                {
                    $"/Themes/{theme}/{{1}}/{{0}}.cshtml",
                    $"/Themes/{theme}/Shared/{{0}}.cshtml"
                };

                if (context.Values.TryGetValue(TENANT_KEY, out string tenant))
                {
                    themeLocations = ExpandTenantLocations(tenant, themeLocations);
                }

                viewLocations = themeLocations.Concat(viewLocations);
            }

            return viewLocations;
        }

        private IEnumerable<string> ExpandTenantLocations(string tenant, IEnumerable<string> defaultLocations)
        {
            foreach (string location in defaultLocations)
            {
                yield return location.Replace("{0}", $"{{0}}_{tenant}");
                yield return location;
            }
        }
    }
}