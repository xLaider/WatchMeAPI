using WatchMeAPI.Models;

namespace WatchMeAPI.Views
{
    public class ProfileView
    {
        public ICollection<MoviesSeries> WatchedMoviesSeries { get; set; }
        public List<string> Friends { get; set; } = new List<string>();
        public List<string> PotentialFriends { get; set; } = new List<string>();
        public List<Notification> OldNotifications { get; set; } = new List<Notification>();
        public List<Notification> NewNotifications { get; set; } = new List<Notification>();
    }
}
