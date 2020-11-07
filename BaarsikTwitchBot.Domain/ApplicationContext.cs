using BaarsikTwitchBot.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BaarsikTwitchBot.Domain
{
    public sealed class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<BotUser> Users { get; set; }
        public DbSet<SongInfo> SongInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BotUserStatistics>(model =>
            {
                model.ToTable(nameof(this.Users));
            });
            modelBuilder.Entity<BotUser>(model =>
            {
                model.ToTable(nameof(this.Users));
                model.HasOne(x => x.Statistics)
                    .WithOne()
                    .HasForeignKey<BotUserStatistics>(x => x.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
