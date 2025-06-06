﻿using Microsoft.AspNetCore.Http;

namespace SaasKit.Multitenancy.Tests
{
    public class MultitenancyHttpContextExtensionsTests
    {
        [Fact]
        public void Can_get_and_set_tenant_context()
        {
            HttpContext httpContext = new DefaultHttpContext();

            using var tenantContext = new TenantContext<TestTenant>(new TestTenant());
            httpContext.SetTenantContext(tenantContext);

            Assert.Same(tenantContext, httpContext.GetTenantContext<TestTenant>());
        }

        [Fact]
        public void Can_get_tenant_instance()
        {
            HttpContext httpContext = new DefaultHttpContext();

            var tenant = new TestTenant { Id = "1" };
            httpContext.SetTenantContext(new TenantContext<TestTenant>(tenant));

            Assert.Same(tenant, httpContext.GetTenant<TestTenant>());
        }

        private class TestTenant
        {
            public string Id { get; set; }
        }
    }
}