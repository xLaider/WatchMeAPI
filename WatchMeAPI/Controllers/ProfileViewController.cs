using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WatchMeAPI.Models;
using WatchMeAPI.Views;

namespace WatchMeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileViewController : ControllerBase
    {
        private readonly DataContext _context;
        public ProfileViewController(DataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<ProfileView>> GetProfileView()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            List<string> potentialFriends = await _context.ApplicationUsers.Select(x=>x.UserName).ToListAsync();
            potentialFriends.Remove(user.UserName);

            var filteredPotentialFriends = potentialFriends.Where(x => !user.Friends.Any(y => y.Name.ToUpper() == x.ToUpper())).ToList(); 
            var profileView = new ProfileView
            {
                WatchedMoviesSeries = user.WatchedMoviesSeries,
                Friends = user.Friends.Select(u => u.Name).ToList(),
                PotentialFriends = filteredPotentialFriends,
                OldNotifications = user.Notifications.Where(x=>x.IsSeen).ToList(),
                NewNotifications = user.Notifications.Where(x =>!x.IsSeen).ToList(),
            };
            return profileView;
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
            return await _context.ApplicationUsers.Include(x => x.WatchedMoviesSeries).Include(x=>x.Friends).Include(x=>x.Notifications).FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);

        }
    }
}
