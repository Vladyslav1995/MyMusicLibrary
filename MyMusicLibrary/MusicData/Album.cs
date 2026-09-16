using System.ComponentModel.DataAnnotations;

namespace MyMusicLibrary.MusicData
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int ReleaseYear { get; set; }

        public string? CoverUrl { get; set; }

        public int ArtistId { get; set; }

        public Artist? Artist { get; set; }

        public List<Song>? Songs { get; set; } = new();
    }
}
