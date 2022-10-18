using WatchMeAPI.Models;

namespace WatchMeAPI.Views
{
    public class MovieView
    {
        public List<Playlist> Playlists { get; set; }
        public ICollection<Episode> WatchedEpisodes { get; set; } = new HashSet<Episode>();
        public ICollection<Season> WatchedSeasons { get; set; } = new HashSet<Season>();
        public ICollection<MoviesSeries> WatchedMovieSeries { get; set; } = new HashSet<MoviesSeries>();
        public MoviesSeries MovieSeries { get; set; } = new MoviesSeries();
        public float? YourRating { get; set; }
    }
}
