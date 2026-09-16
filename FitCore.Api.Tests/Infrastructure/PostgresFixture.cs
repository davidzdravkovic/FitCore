using FitCore.Api.Data;
using FitCore.Api.Data.Stores.Locking;
using FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;
using FitCore.Api.Data.Stores.OrganizationOwner.ServicesStore;
using FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;
using FitCore.Api.Features.Organizations.Admin.Memberships;
using FitCore.Api.Features.Organizations.Admin.Services;
using FitCore.Api.Features.Organizations.Admin.Visits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FitCore.Api.Tests.Infrastructure;

/// <summary>
/// Prefers <c>FITCORE_TEST_CONNECTION</c>; otherwise starts Testcontainers Postgres (Docker required).
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ServiceProvider? _root;
    private string? _connectionString;

    public string ConnectionString =>
        _connectionString
        ?? throw new InvalidOperationException("Postgres fixture was not initialized.");

    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable("FITCORE_TEST_CONNECTION");

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            try
            {
                _container = new PostgreSqlBuilder("postgres:16-alpine").Build();
                await _container.StartAsync();
                _connectionString = _container.GetConnectionString();
            }
            catch (Exception ex) when (ex.GetType().Name.Contains("Docker", StringComparison.Ordinal)
                                       || ex.Message.Contains("Docker", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Integration tests need Postgres. Start Docker Desktop for Testcontainers, "
                    + "or set FITCORE_TEST_CONNECTION to a dedicated test database "
                    + "(e.g. Host=localhost;Port=5432;Database=fitcore_tests;Username=...;Password=...).",
                    ex);
            }
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(ConnectionString));

        services.AddScoped<IVisitStore, EfVisitStore>();
        services.AddScoped<IMembershipStore, EfMembershipStore>();
        services.AddScoped<IServiceStore, EfServiceStore>();
        services.AddScoped<IOrderedRowLocks, EfOrderedRowLocks>();
        services.AddScoped<VisitService>();
        services.AddScoped<MembershipService>();
        services.AddScoped<GymServiceService>();

        _root = services.BuildServiceProvider();

        await using var scope = _root.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public AsyncServiceScope CreateScope() =>
        (_root ?? throw new InvalidOperationException("Fixture not initialized.")).CreateAsyncScope();

    public async Task DisposeAsync()
    {
        if (_root is not null)
            await _root.DisposeAsync();

        if (_container is not null)
            await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
