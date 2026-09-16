using System.ComponentModel.DataAnnotations;

namespace MyMusicLibrary.MusicData
{
    public class Song
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int DurationSeconds { get; set; }

        public int AlbumId { get; set; }

        public Album? Album { get; set; }

        public string? AudioUrl { get; set; }
    }
}
