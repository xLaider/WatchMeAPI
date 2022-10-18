using System.ComponentModel.DataAnnotations;

namespace WatchMeAPI.Models
{
    public class Vote
    {
        [Key]
        public string ImdbID { get; set; }
        public float Score { get; set; }
    }
}
