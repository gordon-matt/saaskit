using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace SaasKit.Multitenancy.Tests
{
    public class TenantPipelineMiddlewareTests
    {
        [Fact]
        public async Task Should_create_middleware_per_tenant()
        {
            using var server = new TestServer(
                new WebHostBuilder().Configure(
                    app =>
                    {
                        app.Use(
                            async (ctx, next) =>
                            {
                                string name = ctx.Request.Path == "/t1" ? "Tenant 1" : "Tenant 2";
                                ctx.SetTenantContext(new TenantContext<AppTenant>(new AppTenant { Name = name }));
                                ctx.Response.StatusCode = StatusCodes.Status200OK; // Set status code early to prevent error
                                await next();
                            });

                        app.UsePerTenant<AppTenant>(
                            (context, builder) =>
                            {
                                builder.UseMiddleware<WriteNameMiddleware>(context.Tenant.Name);
                            });

                        app.Run(
                            async ctx =>
                            {
                                await ctx.Response.WriteAsync(": Test");
                            });
                    }));

            using var client = server.CreateClient();

            string content = await client.GetStringAsync("/t1");
            Assert.Equal("Tenant 1: Test", content);

            content = await client.GetStringAsync("/t2");
            Assert.Equal("Tenant 2: Test", content);
        }

        public class AppTenant
        {
            public string Name { get; set; }
        }

        public class WriteNameMiddleware
        {
            private readonly RequestDelegate next;
            private readonly string name;

            public WriteNameMiddleware(RequestDelegate next, string name)
            {
                this.next = next;
                this.name = name;
            }

            public async Task Invoke(HttpContext context)
            {
                await context.Response.WriteAsync(name);
                await next(context);
            }
        }
    }
}