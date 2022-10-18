using WatchMeAPI.Models;

namespace WatchMeAPI.Utility
{
    public class PlaylistWithUsers
    {
        public Playlist Playlists { get; set; } = new Playlist();
        public List<string> Users { get; set; } = new List<string>();   
    }
}
