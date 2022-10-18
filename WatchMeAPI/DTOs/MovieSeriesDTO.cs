using WatchMeAPI.Models;

namespace WatchMeAPI.DTOs
{
    public class MovieSeriesDTO
    {
        public string ImdbID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsSeries { get; set; }
        public int numberOfSeasons { get; set; }
    }
}
