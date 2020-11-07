using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BaarsikTwitchBot.Domain
{
    public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=BaarsikTwitchBot;Trusted_Connection=True;Integrated Security=true;");
            return new ApplicationContext(optionsBuilder.Options);
        }
    }
}