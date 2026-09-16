using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Songs
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
        public Song Song { get; set; } = default!;

        public SelectList Albums { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Song = await _context.Songs.FindAsync(id);

            if (Song == null)
            {
                return NotFound();
            }

            await LoadAlbumsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAlbumsAsync();
                return Page();
            }

            _context.Attach(Song).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Songs.AnyAsync(s => s.Id == Song.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("Index");
        }

        private async Task LoadAlbumsAsync()
        {
            Albums = new SelectList(
                await _context.Albums
                    .Include(a => a.Artist)
                    .OrderBy(a => a.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                Song.AlbumId);
        }
    }
}