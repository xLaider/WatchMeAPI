using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WatchMeAPI.Models;
using WatchMeAPI.Utility;

namespace WatchMeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        private readonly DataContext _context;
        public FriendsController(DataContext context)
        {
            _context = context;
        }


        [HttpPost("InviteFriends")]
        public async Task<IActionResult> InviteFriends(List<string> names)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            var friends = await _context.ApplicationUsers.Include(x=>x.Notifications).Where(x=>names.Any(n=>n == x.UserName)).ToListAsync();
            if (!friends.Any()) NotFound();
            
            friends.ForEach(friend => friend.Notifications.Add(new Notification
            {
                Content = user.UserName + " has invited you to his friends list!",
                Type = (int)NotificationsEnum.FriendRequest,
                Sender = user.UserName,
                IsSeen = false,
            }));
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("AddFriend/{id}")]
        public async Task<IActionResult> AddFriend(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) NotFound();
            var notification = user.Notifications.FirstOrDefault(x=>x.Id == id);
            if (notification == null) NotFound();
            notification.IsSeen = true;
            var friend = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == notification.Sender);
            if (friend == null) NotFound();
            user.Friends.Add(new Friend
            {
                Name = notification.Sender,
            });
            friend.Friends.Add(new Friend
            {
                Name = user.UserName
            });
            friend.Notifications.Add(new Notification
            {
                Type = (int)NotificationsEnum.FriendResponse,
                Content = user.UserName + " has accepted your friend request!",
                Sender = friend.UserName,
                IsSeen=false,
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetFriends()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            List<string> friendsNames = new List<string>();
            foreach (var friend in user.Friends)
            {
                friendsNames.Add(friend.Name);
            }

            return friendsNames;
        }
        private async Task<ApplicationUser> GetApplicationUserByClaim(ClaimsIdentity claims)
        {
            // Gets list of claims.
            IEnumerable<Claim> claim = claims.Claims;

            // Gets name from claims. Generally it's an email address.
            var usernameClaim = claim
                .Where(x => x.Type == ClaimTypes.Name)
                .FirstOrDefault();
            if (usernameClaim == null) return null;
            // Finds user.
            return await _context.ApplicationUsers.Include(x => x.Friends).Include(x=>x.Notifications)
                .FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);

        }
    }
    
}