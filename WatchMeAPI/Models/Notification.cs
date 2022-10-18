namespace WatchMeAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; }
        public string Sender { get; set; } = string.Empty;
        public bool IsSeen { get; set; }
    }
}
