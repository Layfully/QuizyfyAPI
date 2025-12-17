namespace QuizyfyAPI.Extensions;

internal static class DatabaseExtensions
{
    public static async Task EnsureDatabaseSetup(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using IServiceScope scope = app.Services.CreateScope();
        QuizDbContext db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
        
        try
        {
            await db.Database.MigrateAsync();
            // await DbInitializer.SeedAsync(db); // Uncomment if you add the Seeder
            Console.WriteLine("--> Database migrated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Database migration error: {ex.Message}");
            throw;
        }
    }
}