using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FitnessTracker.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        Console.WriteLine($"[DbContextFactory] Current Directory reported by tool: {currentDirectory}");

        string webUIProjectPath = currentDirectory;
        string appSettingsPath = Path.Combine(webUIProjectPath, "appsettings.json");

        Console.WriteLine($"[DbContextFactory] Expecting appsettings.json at: {appSettingsPath}");

        if (!File.Exists(appSettingsPath))
        {
            throw new FileNotFoundException($"[DbContextFactory Error] Could not find appsettings.json at the calculated path: {appSettingsPath}. Please ensure 'appsettings.json' exists in the startup project ('{Path.GetFileName(webUIProjectPath)}') and that 'dotnet ef' commands are run from the solution root directory.");
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(webUIProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("[DbContextFactory Error] Could not find the 'DefaultConnection' connection string entry within appsettings.json.");
        }
        Console.WriteLine($"[DbContextFactory] Found connection string.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        Console.WriteLine($"[DbContextFactory] Returning DbContext instance.");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}