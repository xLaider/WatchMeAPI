using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WatchMeAPI.Models;

namespace WatchMeAPI
{
    public class DataContext : IdentityDbContext<ApplicationUser> {
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        {

        }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<MoviesSeries> MovieSeries { get; set; }
        public DbSet<Episode>? Episodes { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Vote>? Votes { get; set; }
        public DbSet<Comment>? Comments { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
