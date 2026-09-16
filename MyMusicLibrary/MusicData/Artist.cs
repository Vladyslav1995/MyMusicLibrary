using System.ComponentModel.DataAnnotations;

namespace MyMusicLibrary.MusicData
{
    public class Artist
    {
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Name { get; set; }
        public string? Biography { get; set; }
        public string? ImageURl { get; set; }
        public List<Album>? Albums { get; set; } = new();

    }
}
