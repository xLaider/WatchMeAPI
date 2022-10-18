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
    public class WatchedController : ControllerBase
    {
        private readonly DataContext _context;

        public WatchedController(DataContext context)
        {
            _context = context;
        }

        [HttpPut("AddToWatchedEpisodes/{id}")]
        public async Task<IActionResult> AddToWatchedEpisodes(string id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            var foundSeason = await _context.Seasons.Include(x=>x.Episodes).Where(x=>x.Episodes.Any(x=>x.ImdbID==id)).FirstOrDefaultAsync();
            var foundEpisode = foundSeason.Episodes.FirstOrDefault(x=>x.ImdbID==id);
            user.WatchedEpisodes.Add(foundEpisode);
            bool comparision = true;
            foreach (Episode episode in foundSeason.Episodes)
            {
                if (comparision && !user.WatchedEpisodes.Any(x=>x.ImdbID==episode.ImdbID))
                {
                    comparision = false;
                }
            }
            var series = await _context.MovieSeries.Include(x => x.Seasons).Where(x => x.Seasons.Any(x => x.Id == foundSeason.Id)).FirstOrDefaultAsync();
            if (comparision)
            {
                user.WatchedSeasons.Add(foundSeason);
               
                foreach (Season season in series.Seasons)
                {
                    if (comparision && !user.WatchedSeasons.Any(x => x.Id == season.Id) && season.Episodes != null)
                    {
                        comparision = false;
                    }
                }
                if (comparision)
                {
                    user.WatchedMoviesSeries.Add(series);
                }
            }
            var playlistsContainingEpisode = user.Playlists.Where(x => x.MoviesSeries.Any(m => m.ImdbID == series.ImdbID)).ToList();
            foreach (var playlist in playlistsContainingEpisode)
            {
                foreach (var playlistUser in playlist.Users)
                {
                    if (playlistUser.UserName != user.UserName)
                    {
                        playlistUser.Notifications.Add(new Notification
                        {
                            Content = user.UserName + " has watched episode of "+series.Title+": " + foundEpisode.Title+" from your shared playlist " + playlist.Name,
                            Type = (int)NotificationsEnum.UserFromSharedPlaylistHasWatchedMovieSeries,
                            Sender = user.UserName,
                            IsSeen = false,
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("DeleteFromWatchedEpisodes/{id}")]
        public async Task<IActionResult> DeleteFromWatchedEpisodes(string id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            var foundSeason = await _context.Seasons.Include(x => x.Episodes).Where(x => x.Episodes.Any(x => x.ImdbID == id)).FirstOrDefaultAsync();
            var foundSeries = await _context.MovieSeries.Include(x => x.Seasons).Where(x => x.Seasons.Any(x => x.Id == foundSeason.Id)).FirstOrDefaultAsync();
            user.WatchedMoviesSeries.Remove(foundSeries);
            var foundEpisode = foundSeason.Episodes.FirstOrDefault(x => x.ImdbID == id);
            user.WatchedEpisodes.Remove(foundEpisode);
            bool comparision = true;
            foreach (Season season in user.WatchedSeasons)
            {
                if (season.Id == foundSeason.Id)
                {
                    user.WatchedSeasons.Remove(season);
                }
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("AddMovieToWatched/{id}")]
        public async Task<IActionResult> AddMovieToWatched(string id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();

            var foundMovie = await _context.MovieSeries.FirstOrDefaultAsync(x => x.ImdbID == id);
            user.WatchedMoviesSeries.Add(foundMovie);
            var playlistsContainingMoviesSeries = user.Playlists.Where(x => x.MoviesSeries.Any(m => m.ImdbID == id)).ToList();
            foreach (var playlist in playlistsContainingMoviesSeries)
            {
                foreach (var playlistUser in playlist.Users)
                {
                    if (playlistUser.UserName != user.UserName)
                    {
                        playlistUser.Notifications.Add(new Notification
                        {
                            Content = user.UserName + " has watched " + foundMovie.Title + "  from your shared playlist " + playlist.Name,
                            Type = (int)NotificationsEnum.UserFromSharedPlaylistHasWatchedMovieSeries,
                            Sender = user.UserName,
                            IsSeen = false,
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
            return NoContent();
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
            return await _context.ApplicationUsers.Include(x=>x.WatchedEpisodes).Include(x=>x.WatchedSeasons).Include(x=>x.WatchedMoviesSeries)
                .Include(x=>x.Playlists).ThenInclude(x=>x.Users)
                .Include(x => x.Playlists).ThenInclude(x => x.MoviesSeries)
                .FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);

        }
    }
}
