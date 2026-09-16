using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Songs
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Song> Songs { get; set; } = new List<Song>();

        public async Task OnGetAsync()
        {
            Songs = await _context.Songs
                .Include(s => s.Album)
                .ThenInclude(a => a!.Artist)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}