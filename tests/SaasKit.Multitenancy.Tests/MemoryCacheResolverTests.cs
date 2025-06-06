﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace SaasKit.Multitenancy.Tests
{
    public class MemoryCacheTenantResolverTests
    {
        private HttpContext CreateContext(string requestPath)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = requestPath;

            return context;
        }

        [Fact]
        public async Task Can_resolve_tenant_context()
        {
            var harness = new TestHarness();
            var context = CreateContext("/apple");

            using var tenantContext = await harness.Resolver.ResolveAsync(context);

            Assert.NotNull(tenantContext);
            Assert.Equal("fruit", tenantContext.Tenant.Id);
        }

        [Fact]
        public async Task Can_retrieve_tenant_context_from_cache()
        {
            var harness = new TestHarness();
            var context = CreateContext("/apple");

            using var tenantContext = await harness.Resolver.ResolveAsync(context);

            Assert.True(harness.Cache.TryGetValue("/apple", out
            TenantContext<TestTenant> cachedTenant));

            Assert.Equal(tenantContext.Tenant.Id, cachedTenant.Tenant.Id);
        }

        [Fact]
        public async Task Can_retrieve_tenant_context_from_cache_using_linked_identifier()
        {
            var harness = new TestHarness();
            var context = CreateContext("/apple");

            using var tenantContext = await harness.Resolver.ResolveAsync(context);

            Assert.True(harness.Cache.TryGetValue("/pear", out
            TenantContext<TestTenant> cachedTenant));

            Assert.Equal(tenantContext.Tenant.Id, cachedTenant.Tenant.Id);
        }

        [Fact]
        public async Task Should_dispose_tenant_on_eviction_from_cache_by_default()
        {
            var harness = new TestHarness(cacheExpirationInSeconds: 1);
            var context = CreateContext("/apple");

            using var tenantContext = await harness.Resolver.ResolveAsync(context);

            Thread.Sleep(1000);

            // force MemoryCache to examine itself cache for pending evictions
            harness.Cache.Get("/foobar");

            // and give it a moment to works its magic
            Thread.Sleep(100);

            // should also expire tenant context by default
            Assert.False(harness.Cache.TryGetValue("/apple", out
            TenantContext<TestTenant> cachedTenant), "Apple Exists");
            Assert.True(tenantContext.Tenant.Disposed);
            Assert.Null(cachedTenant);
        }

        [Fact]
        public async Task Should_evict_all_cache_entries_of_tenant_context_by_default()
        {
            var harness = new TestHarness(cacheExpirationInSeconds: 10);

            // first request for apple
            using var tenantContext = await harness.Resolver.ResolveAsync(CreateContext("/apple"));

            // cache should have all 3 entries
            Assert.NotNull(harness.Cache.Get("/apple"));
            Assert.NotNull(harness.Cache.Get("/pear"));
            Assert.NotNull(harness.Cache.Get("/grape"));

            // expire apple
            harness.Cache.Remove("/apple");

            Thread.Sleep(500);

            // look it up again so it registers
            harness.Cache.TryGetValue("/apple", out
            TenantContext<TestTenant> cachedTenant);

            Thread.Sleep(500);

            // pear is expired - because apple is
            Assert.False(harness.Cache.TryGetValue("/pear", out cachedTenant), "Pear Exists");

            // should also expire tenant context by default
            Assert.True(tenantContext.Tenant.Disposed);
        }

        [Fact]
        public async Task Can_evict_single_cache_entry_of_tenant_context()
        {
            var harness = new TestHarness(cacheExpirationInSeconds: 2, evictAllOnExpiry: false);
            var context = CreateContext("/apple");

            // first request for apple
            await harness.Resolver.ResolveAsync(CreateContext("/apple"));

            // wait 1 second
            Thread.Sleep(1000);

            // second request for pear
            await harness.Resolver.ResolveAsync(CreateContext("/pear"));

            // wait 1 second
            Thread.Sleep(1000);

            // apple is expired
            Assert.False(harness.Cache.TryGetValue("/apple", out TenantContext<TestTenant> cachedTenant), "Apple Exists");

            // pear is not expired
            Assert.True(harness.Cache.TryGetValue("/pear", out cachedTenant), "Pear Does Not Exist");
        }

        [Fact]
        public async Task Can_dispose_on_eviction()
        {
            var harness = new TestHarness(cacheExpirationInSeconds: 1, disposeOnEviction: true);
            var context = CreateContext("/apple");

            using var tenantContext = await harness.Resolver.ResolveAsync(context);

            Thread.Sleep(2 * 1000);
            // access it again so that MemoryCache examines it's cache for pending evictions
            harness.Cache.Get("/foobar");

            Thread.Sleep(1 * 1000);
            // access it again and we should see the eviction
            Assert.True(tenantContext.Tenant.Disposed);
        }

        [Fact]
        public async Task Can_not_dispose_on_eviction()
        {
            var harness = new TestHarness(cacheExpirationInSeconds: 1, disposeOnEviction: false);
            var context = CreateContext("/apple");

            using var tenantContext = await harness.Resolver.ResolveAsync(context);

            Thread.Sleep(1 * 1000);
            // access it again so that MemoryCache examines it's cache for pending evictions
            harness.Cache.Get("/foobar");

            Thread.Sleep(1 * 1000);
            // access it again and even though it's disposed, it should not be evicted
            Assert.False(tenantContext.Tenant.Disposed);
        }

        private class TestTenant : IDisposable
        {
            public bool Disposed { get; set; }

            public string Id { get; set; }

            public List<string> Paths { get; set; }

            public CancellationTokenSource Cts = new();

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Cts.Cancel();
                }

                Disposed = true;
            }
        }

        private class TestTenantMemoryCacheResolver : MemoryCacheTenantResolver<TestTenant>
        {
            private readonly List<TestTenant> tenants =
            [
                new TestTenant { Id = "fruit", Paths = ["/apple","/pear","/grape"]},
                new TestTenant { Id = "vegetable", Paths = ["/lettuce","/carrot","/onion"]}
            ];

            private readonly int cacheExpirationInSeconds;

            public TestTenantMemoryCacheResolver(IMemoryCache cache, ILoggerFactory loggerFactory, MemoryCacheTenantResolverOptions options, int cacheExpirationInSeconds = 10)
                : base(cache, loggerFactory, options)
            {
                this.cacheExpirationInSeconds = cacheExpirationInSeconds;
            }

            protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
            {
                return new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheExpirationInSeconds));
            }

            protected override string GetContextIdentifier(HttpContext context)
            {
                return context.Request.Path;
            }

            protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<TestTenant> context)
            {
                return context?.Tenant?.Paths;
            }

            protected override Task<TenantContext<TestTenant>> ResolveAsync(HttpContext context)
            {
                var tenant = tenants.FirstOrDefault(testTenant => testTenant.Paths.Contains(context.Request.Path));

                var tenantContext = new TenantContext<TestTenant>(tenant);

                tenantContext.Properties.Add("Created", DateTime.UtcNow);

                return Task.FromResult(tenantContext);
            }
        }

        private class TestHarness
        {
            private static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            public IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions()
            {
                // for testing purposes, we'll scan every 100 milliseconds
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(100),
                Clock = new SystemClock()
            });

            public TestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
            {
                var options = new MemoryCacheTenantResolverOptions { DisposeOnEviction = disposeOnEviction, EvictAllEntriesOnExpiry = evictAllOnExpiry };
                Resolver = new TestTenantMemoryCacheResolver(Cache, loggerFactory, options, cacheExpirationInSeconds);
            }

            public ITenantResolver<TestTenant> Resolver { get; }
        }
    }
}