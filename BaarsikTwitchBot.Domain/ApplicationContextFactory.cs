using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BaarsikTwitchBot.Domain
{
    public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Broadcasterkits", "data.db");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            optionsBuilder.UseSqlite($"Filename={path}");
            return new ApplicationContext(optionsBuilder.Options);
        }
    }
}