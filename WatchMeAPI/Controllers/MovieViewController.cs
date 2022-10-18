using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Security.Claims;
using WatchMeAPI.DTOs;
using WatchMeAPI.Models;
using WatchMeAPI.Views;

namespace WatchMeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieViewController : ControllerBase
    {
        private readonly DataContext _context;
        public MovieViewController(DataContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MovieView>> GetMovieView(string id, MovieSeriesDTO movieSeriesDTO)
        {   
            var movieSeries = await _context.MovieSeries.Include(x=>x.Seasons).ThenInclude(x=>x.Episodes).Include(x=>x.Comments).FirstOrDefaultAsync(x=>x.ImdbID ==id);
            if (movieSeries == null) movieSeries = await CreateMissingMovieSeriesFromIMDB(movieSeriesDTO);
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            var availablePlaylists = new List<Playlist>();
            if (user != null)
            {
                availablePlaylists = await _context.Playlists.Where(x => x.Users.Contains(user) && !x.MoviesSeries.Any(x => x.ImdbID == id)).ToListAsync();
            }

            Vote vote = null;
            if (user != null)
            {
                vote = user.Votes.FirstOrDefault(x=> x.ImdbID == id); 
            }

            MovieView movieView = new MovieView
            {
                Playlists = availablePlaylists,
                WatchedEpisodes = user != null ? user.WatchedEpisodes : new HashSet<Episode>(),
                WatchedSeasons = user != null ? user.WatchedSeasons : new HashSet<Season>(),
                WatchedMovieSeries = user != null ? user.WatchedMoviesSeries : new HashSet<MoviesSeries>(),
                MovieSeries = movieSeries,
                YourRating = vote == null ? null : vote.Score
            };
            await _context.SaveChangesAsync();
            return Ok(movieView);
        }

        [HttpPut("PostCommentForMovieSeries/{id}/{comment}")]
        public async Task<IActionResult> PostCommentForMovieSeries(string id, string comment)
        {
            var movieSeries = await _context.MovieSeries.Include(x => x.Comments).FirstOrDefaultAsync(x => x.ImdbID == id);
            if (movieSeries == null) return NotFound();
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            movieSeries.Comments.Add(new Comment
            {
                Sender = user.UserName,
                Content = comment,
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("PostRatingForMovieSeries/{id}/{rating}")]
        public async Task<IActionResult> PostRatingForMovieSeries(string id, float rating)
        {
            var movieSeries = await _context.MovieSeries.Include(x => x.Comments).FirstOrDefaultAsync(x => x.ImdbID == id);
            if (movieSeries == null) return NotFound();
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var user = await GetApplicationUserByClaim(identity);
            if (user == null) return NotFound();
            if (user.Votes.FirstOrDefault(x => x.ImdbID == id) != null) {
                movieSeries.Rating = (movieSeries.Rating + rating) / movieSeries.AmountOfVotes;
                var voteToUpdate = user.Votes.FirstOrDefault(x => x.ImdbID == id);
                voteToUpdate.Score = rating;
            }
            else
            {
                movieSeries.AmountOfVotes++;
                movieSeries.Rating = (movieSeries.Rating + rating) / movieSeries.AmountOfVotes;
                user.Votes.Add(new Vote
                {
                    Score = rating,
                    ImdbID = id,
                });
            }
        
            await _context.SaveChangesAsync();
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
            if (usernameClaim == null) return null;
            // Finds user.
            return await _context.ApplicationUsers.Include(x=>x.WatchedSeasons).Include(x=>x.WatchedEpisodes).Include(x=>x.WatchedMoviesSeries)
                .Include(x=>x.Votes)
                .FirstOrDefaultAsync(u => u.UserName == usernameClaim.Value);
        }

        private async Task<MoviesSeries> CreateMissingMovieSeriesFromIMDB(MovieSeriesDTO movieSeriesDTO)
        {

            List<Season> seasonsList = new List<Season>();
            for (int i = 1; i <= movieSeriesDTO.numberOfSeasons; i++)
            {
                var client = new HttpClient();
                var httpResponse = client.GetAsync("https://imdb-api.com/en/API/SeasonEpisodes/k_msof1d83/" + movieSeriesDTO.ImdbID + "/" + i);
                httpResponse.Wait();
                var data = await httpResponse.Result.Content.ReadAsStringAsync();
                dynamic finalData = JsonConvert.DeserializeObject<ExpandoObject>(data, new ExpandoObjectConverter());
                List<Episode> episodes = new List<Episode>();
                foreach (var episode in finalData.episodes)
                {
                    if (episode.released.Length > 4)
                    {
                        episodes.Add(new Episode
                        {
                            ImdbID = episode.id,
                            Title = episode.title,
                            EpisodeNumber = Int32.Parse(episode.episodeNumber),
                            ReleaseDate = DateTime.Parse(episode.released)
                        });
                    }
                    
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

