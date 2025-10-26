using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mongo2Go;
using PartyJukebox.Api.Configuration;
using PartyJukebox.Api.Infrastructure;
using PartyJukebox.Api.Services;
using Xunit;

namespace PartyJukebox.Api.Tests;

public class TestingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MongoDbRunner? _runner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<MongoDbContext>();
            services.RemoveAll<IRateLimitService>();
            services.RemoveAll<IStreamClient>();

            var initializerDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType == typeof(MongoInitializer));
            if (initializerDescriptor is not null)
            {
                services.Remove(initializerDescriptor);
            }

            if (_runner is null)
            {
                throw new InvalidOperationException("Mongo runner not started");
            }

            services.AddSingleton<IOptions<MongoOptions>>(_ => Options.Create(new MongoOptions
            {
                ConnectionString = _runner.ConnectionString,
                Database = "partyjukebox-tests"
            }));
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<IRateLimitService, PermitAllRateLimitService>();
            services.AddSingleton<IStreamClient, FakeStreamClient>();
        });
    }

    public Task InitializeAsync()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        _runner?.Dispose();
        return Task.CompletedTask;
    }

    private sealed class PermitAllRateLimitService : IRateLimitService
    {
        public Task<bool> TryConsumeAsync(string key, int limitPerMinute, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class FakeStreamClient : IStreamClient
    {
        public Task<StreamMetadata> ResolveAsync(string videoIdOrUrl, CancellationToken cancellationToken)
        {
            return Task.FromResult(new StreamMetadata("video123", "Test Track", "Test Channel", 180000, ""));
        }

        public Task<StreamMetadata?> SearchAsync(string query, CancellationToken cancellationToken)
        {
            return Task.FromResult<StreamMetadata?>(new StreamMetadata("search123", $"{query} result", "Test Channel", 180000, ""));
        }
    }
}
