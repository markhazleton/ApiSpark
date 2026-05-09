using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebSpark.Recipe.Data;

public class RecipeDbContextFactory : IDesignTimeDbContextFactory<RecipeDbContext>
{
    public RecipeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RecipeDbContext>();
        optionsBuilder.UseSqlite("Data Source=webspark-recipe-design.db",
            b => b.MigrationsAssembly("WebSpark.Recipe"));
        return new RecipeDbContext(optionsBuilder.Options);
    }
}
