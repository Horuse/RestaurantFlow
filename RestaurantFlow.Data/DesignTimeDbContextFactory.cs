using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RestaurantFlow.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RestaurantDbContext>
{
    public RestaurantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
        optionsBuilder.UseSqlite("Data Source=restaurant.db");

        return new RestaurantDbContext(optionsBuilder.Options);
    }
}