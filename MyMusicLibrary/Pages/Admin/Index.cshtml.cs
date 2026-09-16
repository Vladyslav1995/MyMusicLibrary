using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<string> ArtistNames { get; set; } = new();
        public List<int> AlbumCounts { get; set; } = new();

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int ArtistCount { get; set; }
        public int AlbumCount { get; set; }
        public int SongCount { get; set; }

        public List<Artist> RecentArtists { get; set; } = new();

        public List<Album> RecentAlbums { get; set; } = new();

        public List<Song> RecentSongs { get; set; } = new();

        public async Task OnGetAsync()
        {
            ArtistCount = await _context.Artists.CountAsync();
            AlbumCount = await _context.Albums.CountAsync();
            SongCount = await _context.Songs.CountAsync();

            RecentArtists = await _context.Artists
                .OrderByDescending(a => a.Id)
                .Take(5)
                .ToListAsync();

            RecentAlbums = await _context.Albums
                .Include(a => a.Artist)
                .OrderByDescending(a => a.Id)
                .Take(5)
                .ToListAsync();

            RecentSongs = await _context.Songs
                .Include(s => s.Album)
                .ThenInclude(a => a!.Artist)
                .OrderByDescending(s => s.Id)
                .Take(5)
                .ToListAsync();


            // ADD THIS PART

            var albumStatistics = await _context.Artists
                .Select(a => new
                {
                    ArtistName = a.Name,
                    AlbumCount = a.Albums!.Count
                })
                .OrderByDescending(x => x.AlbumCount)
                .ThenBy(x => x.ArtistName)
                .Take(10)
                .ToListAsync();

            ArtistNames = albumStatistics
                .Select(x => x.ArtistName)
                .ToList();

            AlbumCounts = albumStatistics
                .Select(x => x.AlbumCount)
                .ToList();
        }
    }
}