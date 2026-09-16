using MyMusicLibrary.MusicData;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MyMusicLibrary.Tests
{
    public class ArtistTests
    {
        [Test]
        public void Artist_ShouldStoreName()
        {
            // Arrange
            var artist = new Artist
            {
                Name = "Metallica"
            };

            // Act
            var result = artist.Name;

            // Assert
            Assert.That(result, Is.EqualTo("Metallica"));
        }

        [Test]
        public void Artist_Name_ShouldBeRequired()
        {
            // Arrange
            var artist = new Artist
            {
                Name = ""
            };

            var context = new ValidationContext(artist);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(
                artist,
                context,
                results,
                validateAllProperties: true);

            // Assert
            Assert.That(isValid, Is.False);
        }
        [Test]
        public void Artist_Name_ShouldNotExceed100Characters()
        {
            // Arrange
            var artist = new Artist
            {
                Name = new string('A', 101)
            };

            var context = new ValidationContext(artist);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(
                artist,
                context,
                results,
                validateAllProperties: true);

            // Assert
            Assert.That(isValid, Is.False);
        }
    }
}