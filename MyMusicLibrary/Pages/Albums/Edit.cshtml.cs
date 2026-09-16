using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Albums
{
    [Authorize(Policy = "AdminOnly")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Album Album { get; set; } = default!;

        public SelectList Artists { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Album = await _context.Albums.FindAsync(id);

            if (Album == null)
            {
                return NotFound();
            }

            await LoadArtistsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadArtistsAsync();
                return Page();
            }

            _context.Attach(Album).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Albums.AnyAsync(a => a.Id == Album.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("Index");
        }

        private async Task LoadArtistsAsync()
        {
            Artists = new SelectList(
                await _context.Artists
                    .OrderBy(a => a.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                Album.ArtistId);
        }
    }
}