using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;
using Microsoft.AspNetCore.Authorization;

namespace MyMusicLibrary.Pages.Artists
{
    [Authorize(Policy = "AdminOnly")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Artist? Artist { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Artist == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Artist == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .FindAsync(Artist.Id);

            if (artist == null)
            {
                return NotFound();
            }

            _context.Artists.Remove(artist);

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}