namespace WatchMeAPI.DTOs
{
    public class PlaylistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public bool SendsNotifications { get; set; }
    }
}
