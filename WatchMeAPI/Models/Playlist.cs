using System.ComponentModel.DataAnnotations;

namespace WatchMeAPI.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<MoviesSeries> MoviesSeries { get; set; } = new HashSet<MoviesSeries>();
        public bool IsPrivate { get; set; }
        public bool SendsNotifications { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();
        public string Creator { get; set; } = string.Empty;
    }
}
