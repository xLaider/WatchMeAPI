using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WatchMeAPI.Models;
using WatchMeAPI.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WatchMeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly DataContext _context;
        public NotificationsController(DataContext context)
        {
            _context = context;
        }


        [HttpGet("GetNotificationsCount")]
        public async Task<ActionResult<int>> GetNotificationsCount()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            if (user.LastLogin.Date < DateTime.UtcNow.Date)
            {
                await NotifyAboutNewEpisodes(user.UserName);
            }
            return user.Notifications.Where(x=>!x.IsSeen).ToList().Count;
        }

        [HttpPut("SetNotificationAsSeen/{id}")]
        public async Task<ActionResult<int>> SetNotificationAsSeen(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            var notification = user.Notifications.FirstOrDefault(x => x.Id == id);
            if (notification == null) return NotFound();
            notification.IsSeen = true;

            await _context.SaveChangesAsync();

            return Ok();
        }


        private async Task NotifyAboutNewEpisodes(string username)
        {
            var user = await _context.ApplicationUsers.Include(x=>x.Playlists)
                .ThenInclude(x=>x.MoviesSeries)
                .ThenInclude(x=>x.Seasons)
                .ThenInclude(x=>x.Episodes)
                .FirstOrDefaultAsync(x=>x.UserName==username);

            foreach (var playlist in user.Playlists)
            {
                foreach (var series in playlist.MoviesSeries.Where(x => x.IsSeries).ToList())
                {
                    foreach (var season in series.Seasons)
                    {
                        foreach (var episode in season.Episodes)
                        {
                            if (user.LastLogin.Date < DateTime.UtcNow.Date && episode.ReleaseDate.Date <= DateTime.UtcNow.Date)
                            {
                                user.Notifications.Add(new Notification
                                {
                                    Content = "New episode of : "+series.Title+" is available: "+episode.Title,
                                    Type = (int)NotificationsEnum.NewEpisodeAvailable,
                                    Sender = "Watch.me",
                                    IsSeen = false
                                });
                            }
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<ApplicationUser> GetApplicationUserByClaim(ClaimsIdentity claims)
        {
            // Gets list of claims.
            IEnumerable<Claim> claim = claims.Claims;

            // Gets name from claims. Generally it's an email address.
            var usernameClaim = claim
                .Where(x => x.Type == ClaimTypes.Name)
                .FirstOrDefault();

            // Finds user.
            return await _context.ApplicationUsers.Include(x=>x.Notifications).Include(x=>x.Friends)
                .FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);

        }
    }
}
