namespace MyMusicLibrary
{
        public record CreateSongRequest(
            string Name,
            int DurationSeconds,
            int AlbumId,
            string? AudioUrl
        );

        public record UpdateSongRequest(
            string Name,
            int DurationSeconds,
            int AlbumId,
            string? AudioUrl
        );

        public record CreateAlbumRequest(
            string Name,
            int ReleaseYear,
            string? CoverUrl,
            int ArtistId
        );

        public record UpdateAlbumRequest(
            string Name,
            int ReleaseYear,
            string? CoverUrl,
            int ArtistId
        );

        public record CreateArtistRequest(
            string Name,
            string? Biography,
            string? ImageUrl
        );

        public record UpdateArtistRequest(
            string Name,
            string? Biography,
            string? ImageUrl
        );
    }
    