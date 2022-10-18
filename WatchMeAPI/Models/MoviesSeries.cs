using System.ComponentModel.DataAnnotations;

namespace WatchMeAPI.Models
{
    public class MoviesSeries
    {
        [Key]
        public string ImdbID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsSeries { get; set; }
        public ICollection<Season> Seasons { get; set; } = new HashSet<Season>();
        public float Rating { get; set; }
        public int AmountOfVotes { get; set; }
        public ICollection<Playlist> Playlists { get; set; } = new HashSet<Playlist>();
        public virtual ICollection<ApplicationUser> UsersThatWatched { get; set; } = new HashSet<ApplicationUser>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
