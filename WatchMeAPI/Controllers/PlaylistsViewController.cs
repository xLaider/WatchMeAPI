using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WatchMeAPI.Models;
using WatchMeAPI.Utility;

namespace WatchMeAPI.Views
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsViewController : ControllerBase
    {
        private readonly DataContext _context;
        public PlaylistsViewController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PlaylistsView>> GetPlaylistsView()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            PlaylistsView playlistsView = new PlaylistsView
            {
                Playlists = user.Playlists,
                WatchedMovieSeries = user.WatchedMoviesSeries,
                Friends = user.Friends.Select(x=>x.Name).ToList()
            };
            return Ok(playlistsView);
        }

        [HttpGet("GetFriendsToAddToPlaylist/{id}")]
        public async Task<ActionResult<List<string>>> GetFriendsToAddToPlaylist(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();

            List<string> friendsToAddToPlaylist = user.Friends.Select(f => f.Name).ToList();
            var playlist = user.Playlists.FirstOrDefault(x => x.Id == id);
            List<string> filteredFriendsToAdd = friendsToAddToPlaylist.Where(f=> !playlist.Users.Any(p=>p.UserName==f)).ToList();
            return Ok(filteredFriendsToAdd);
        }

        [HttpPut("AddFriendToPlaylist/{id}/{name}")]
        public async Task<IActionResult> AddFriendToPlaylist(int Id,string name)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            var playlist = user.Playlists.FirstOrDefault(x => x.Id == Id);
            if (playlist == null) return NotFound();
            var friend = user.Friends.FirstOrDefault(x => x.Name == name);
            if (friend == null) return NotFound();
            if (playlist.Users.Any(x=>x.UserName == name)) return NotFound();
            var friendToAdd = _context.Users.FirstOrDefault(x => x.UserName == name);
            friendToAdd.Notifications.Add(new Notification
            {
                Content = user.UserName + " has added you to shared playlist: " + playlist.Name,
                Type = (int)NotificationsEnum.PlaylistInvitation,
                Sender = user.UserName,
                IsSeen = false,
            });
            playlist.Users.Add(friendToAdd);
            _context.SaveChanges();
            return Ok();
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
            return await _context.ApplicationUsers
                .Include(x=>x.Playlists).ThenInclude(x=>x.MoviesSeries)
                .Include(x=>x.Playlists).ThenInclude(x=>x.Users)
                .Include(x=>x.WatchedMoviesSeries).Include(x=>x.Friends)
                .FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);

        }
    }
}
