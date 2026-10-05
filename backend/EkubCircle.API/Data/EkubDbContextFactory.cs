using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EkubCircle.API.Data;

public class EkubDbContextFactory : IDesignTimeDbContextFactory<EkubDbContext>
{
    public EkubDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EkubDbContext>();
        optionsBuilder.UseSqlite("Data Source=ekubcircle.db");

        return new EkubDbContext(optionsBuilder.Options);
    }
}
