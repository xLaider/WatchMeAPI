using System.ComponentModel.DataAnnotations;

namespace WatchMeAPI.Models
{
    public class Comment
    {
        [Key]
        public int ID { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
