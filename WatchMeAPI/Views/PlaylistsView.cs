using WatchMeAPI.Models;

namespace WatchMeAPI.Views
{
    public class PlaylistsView
    {
        public ICollection<Playlist> Playlists { get; set; }
        public ICollection<MoviesSeries> WatchedMovieSeries { get; set; } = new HashSet<MoviesSeries>();
        public List<string> Friends = new List<string>();
    }
}
