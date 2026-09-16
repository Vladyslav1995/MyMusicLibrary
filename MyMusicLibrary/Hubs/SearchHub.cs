using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Hubs
{
    public class SearchHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public SearchHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await Clients.Caller.SendAsync(
                    "ReceiveResults",
                    new
                    {
                        artists = new List<object>(),
                        albums = new List<object>(),
                        songs = new List<object>()
                    });

                return;
            }

            query = query.Trim();

            // -------------------------
            // Artists
            // -------------------------

            var artists = await _context.Artists
                .Where(a => a.Name.Contains(query))
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    a.Id,
                    a.Name
                })
                .ToListAsync();

            // -------------------------
            // Albums
            // -------------------------

            var albums = await _context.Albums
                .Include(a => a.Artist)
                .Where(a =>
                    a.Name.Contains(query) ||
                    (a.Artist != null &&
                     a.Artist.Name.Contains(query)))
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    a.Id,
                    a.Name,

                    ArtistName = a.Artist != null
                        ? a.Artist.Name
                        : ""
                })
                .ToListAsync();

            // -------------------------
            // Songs
            // -------------------------

            var songs = await _context.Songs
                .Include(s => s.Album)
                .ThenInclude(a => a!.Artist)
                .Where(s =>
                    s.Name.Contains(query) ||

                    (s.Album != null &&
                     s.Album.Name.Contains(query)) ||

                    (s.Album != null &&
                     s.Album.Artist != null &&
                     s.Album.Artist.Name.Contains(query)))
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,

                    AlbumName = s.Album != null
                        ? s.Album.Name
                        : "",

                    ArtistName =
                        s.Album != null &&
                        s.Album.Artist != null
                            ? s.Album.Artist.Name
                            : ""
                })
                .ToListAsync();

            // -------------------------
            // Send results
            // -------------------------

            await Clients.Caller.SendAsync(
                "ReceiveResults",
                new
                {
                    artists,
                    albums,
                    songs
                });
        }
    }
}