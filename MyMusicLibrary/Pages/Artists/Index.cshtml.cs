using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Artists
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Artist> Artists { get; set; } = new List<Artist>();

        public async Task OnGetAsync()
        {
            Artists = await _context.Artists
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
    }
}