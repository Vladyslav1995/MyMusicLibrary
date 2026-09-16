using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;

namespace MyMusicLibrary.Pages.Songs
{
    [Authorize(Policy = "AdminOnly")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Song Song { get; set; } = new();

        public SelectList Albums { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await LoadAlbumsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAlbumsAsync();
                return Page();
            }

            _context.Songs.Add(Song);
            await _context.SaveChangesAsync();

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
                "Name");
        }
    }
}