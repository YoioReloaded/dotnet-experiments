using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Simple_CRUD.Tests.Integration.endpoints;

public class InMemoryStoreWebApplicationFactory<TStore,TEntryPoint>
 : WebApplicationFactory<TEntryPoint>
  where TStore : class
  where TEntryPoint : class
{
    private readonly TStore _seedData;

    public InMemoryStoreWebApplicationFactory(TStore seedData)
    {
        _seedData = seedData;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the real movie service registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TStore));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<TStore>(_ => (TStore)(object)_seedData);
        });
    }
}
