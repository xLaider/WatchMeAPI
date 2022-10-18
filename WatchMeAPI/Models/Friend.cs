using System.ComponentModel.DataAnnotations;

namespace WatchMeAPI.Models
{
    public class Friend
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();
    }
}
