namespace WatchMeAPI.Models
{
    public class Season
    {
        public int Id { get; set; }
        public int SeasonNumber { get; set; }
        public IList<Episode> Episodes { get; set; }
        public virtual ICollection<ApplicationUser> UsersThatWatched { get; set; } = new HashSet<ApplicationUser>();
    }
}
