using FitCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Composition;

public static class WebApplicationExtensions
{
    public static async Task UseFitCorePipelineAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }

        await PlatformAdminSeeder.SeedAsync(app.Services);

        app.UseExceptionHandler();
        app.UseCors(app.Environment.IsDevelopment() ? "DevCors" : "ProdCors");

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}
