using Microsoft.AspNetCore.Identity;

namespace WatchMeAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Playlist> Playlists { get; set; } = new HashSet<Playlist>();
        public virtual ICollection<MoviesSeries> WatchedMoviesSeries { get; set; } = new HashSet<MoviesSeries>();
        public virtual ICollection<Episode> WatchedEpisodes { get; set; } = new HashSet<Episode>();
        public virtual ICollection<Season> WatchedSeasons { get; set; } = new HashSet<Season>();
        public virtual ICollection<Friend> Friends { get; set; } = new HashSet<Friend>();
        public virtual List<Notification> Notifications { get; set; } = new List<Notification>();
        public DateTime LastLogin { get; set; }
    }
}
