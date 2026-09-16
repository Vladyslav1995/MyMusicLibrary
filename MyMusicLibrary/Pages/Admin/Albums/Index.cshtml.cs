using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Admin.Albums
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Album> Albums { get; set; } = new List<Album>();

        public async Task OnGetAsync()
        {
            Albums = await _context.Albums
                .Include(a => a.Artist)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
    }
}