using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WatchMeAPI.Auth;
using WatchMeAPI.DTOs;
using WatchMeAPI.Models;

namespace WatchMeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private readonly DataContext _context;

        public PlaylistsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Playlists
        [Authorize(Roles = UserRoles.User)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPlaylists()
        {
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.UserName == "Laider");
            if (user == null)
          {
              return NotFound();
          }

            return await _context.Playlists.Where(x =>x.Users.Contains(user)).ToListAsync();
        }

        [Authorize(Roles = UserRoles.User)]
        [HttpGet("PlaylistsAvailableForMovieSeries/{imdbId}")]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPlaylistsAvailableForMovieSeries(string imdbId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null)
            {
                return NotFound();
            }

            return await _context.Playlists.Where(x => x.Users.Contains(user) && !x.MoviesSeries.Any(x=> x.ImdbID==imdbId)).ToListAsync();
        }

        // GET: api/Playlists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Playlist>> GetPlaylist(int id)
        {
          if (_context.Playlists == null)
          {
              return NotFound();
          }
            var playlist = await _context.Playlists.FindAsync(id);

            if (playlist == null)
            {
                return NotFound();
            }

            return playlist;
        }

        // PUT: api/Playlists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlaylist(int id, Playlist playlist)
        {
            if (id != playlist.Id)
            {
                return BadRequest();
            }

            _context.Entry(playlist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlaylistExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Playlists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Playlist>> PostPlaylist(PlaylistDTO playlistDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);

            if (user == null)
          {
              return Problem("Entity set 'DataContext.Playlists'  is null.");
          }
            Playlist playlist = new Playlist
            {
                Id = playlistDTO.Id,
                Name = playlistDTO.Name,
                IsPrivate = playlistDTO.IsPrivate,
                SendsNotifications = playlistDTO.SendsNotifications,
                Creator = user.UserName
            };
            playlist.Users.Add(user);
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlaylist", new { id = playlist.Id }, playlist);
        }

        // DELETE: api/Playlists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            if (_context.Playlists == null)
            {
                return NotFound();
            }
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }

            _context.Playlists.Remove(playlist);
            ;

            return NoContent();
        }

        [HttpPut("AddMovieToPlaylist/{id}")]
        public async Task<IActionResult> AddMovieToPlaylist(int id, MovieSeriesDTO movieSeriesDTO)
        {
            MoviesSeries movieSeries = await _context.MovieSeries.FirstOrDefaultAsync(x => x.ImdbID == movieSeriesDTO.ImdbID);
            if (movieSeries == null) return NotFound();



            var playlist = await _context.Playlists.FindAsync(id);
            if (_context.Playlists == null)
            {
                return NotFound();
            }
            playlist.MoviesSeries.Add(movieSeries);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        private bool PlaylistExists(int id)
        {
            return (_context.Playlists?.Any(e => e.Id == id)).GetValueOrDefault();
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
            return await _context.ApplicationUsers.Include(x=>x.Playlists)
                .FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);

        }
        private async Task<MoviesSeries> CreateMissingMovieSeriesFromIMDB(MovieSeriesDTO movieSeriesDTO)
        {
            List<Season> seasonsList = new List<Season>();
            for (int i=1; i<=movieSeriesDTO.numberOfSeasons; i++)
            {
                var client = new HttpClient();
                var httpResponse = client.GetAsync("https://imdb-api.com/en/API/SeasonEpisodes/k_msof1d83/"+movieSeriesDTO.ImdbID+"/"+i);
                httpResponse.Wait();
                var data = await httpResponse.Result.Content.ReadAsStringAsync();
                dynamic finalData = JsonConvert.DeserializeObject<ExpandoObject>(data, new ExpandoObjectConverter());
                List<Episode> episodes = new List<Episode>();
                foreach (var episode in finalData.episodes)
                {
                    episodes.Add(new Episode
                    {
                        ImdbID = episode.id,
                        Title = episode.title,
                        EpisodeNumber = Int32.Parse(episode.episodeNumber),
                        ReleaseDate = DateTime.Parse(episode.released)
                    });
                }
                seasonsList.Add(new Season
                {
                    SeasonNumber = i,
                    Episodes = episodes
                });
                
            }
            MoviesSeries movieSeries = new MoviesSeries
            {
                ImdbID = movieSeriesDTO.ImdbID,
                Title = movieSeriesDTO.Title,
                Image = movieSeriesDTO.Image,
                IsSeries = movieSeriesDTO.numberOfSeasons > 0,
                Seasons = seasonsList,
            };
            await _context.MovieSeries.AddAsync(movieSeries);
            return movieSeries;
        }
    }
}
