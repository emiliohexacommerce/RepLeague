using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RepLeague.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tools (migrations) when the API startup project
/// is unavailable (e.g. running in the background during development).
/// Connection string is read from the REPLAGUE_CONN env-var, falling back to the
/// development Azure SQL connection string.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("REPLAGUE_CONN")
            ?? "Data Source=hexacore.database.windows.net;Initial Catalog=RepLeague;Persist Security Info=True;User ID=hexadex;Password=h3xadex1**;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
