

using System.ComponentModel.DataAnnotations;

namespace WatchMeAPI.Models
{
    public class Episode
    {
        [Key]
        public string ImdbID { get; set; }
        public string Title { get; set; }
        public int EpisodeNumber { get; set; }
        public DateTime ReleaseDate { get; set; }
        public virtual ICollection<ApplicationUser> UsersThatWatched { get; set; } = new HashSet<ApplicationUser>();
    }
}
