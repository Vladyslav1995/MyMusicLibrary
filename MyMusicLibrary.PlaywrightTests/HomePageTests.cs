using Azure;
using Microsoft.Playwright;
using MyMusicLibrary.MusicData;
using NUnit.Framework.Legacy;
using System.Text.Json;

namespace MyMusicLibrary.PlaywrightTests
{
    public class HomePageTests : PageTest
    {
        [Test]
        public async Task HomePage_ShouldLoad()
        {
            await Page.GotoAsync("https://localhost:7282");

            await Expect(Page).ToHaveTitleAsync("My Music Library - MyMusicLibrary");

            await Expect(Page.Locator("body")).ToContainTextAsync("MyMusicLibrary");
        }

        [Test]
        public async Task HomePage_ShouldLoad2()
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            await using var browser = await playwright.Webkit.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false
                });

            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://localhost:7282");

            await Expect(page).ToHaveTitleAsync("My Music Library - MyMusicLibrary");
        }

        [Test]
        public async Task SearchForBeatles_ShouldDisplayResults()
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    SlowMo = 500
                });

            var page = await browser.NewPageAsync();

            // Open the website
            await page.GotoAsync("https://localhost:7282");

            // Click Search
            await page.GetByText("Search")
                .ClickAsync();

            // Enter search text
            await page.Locator("//input[@id='searchInput']")
                .FillAsync("Beatles");

            await Expect(page.Locator("//div[@id='searchResults']//div[1]//a//strong")).ToContainTextAsync("Name: The Beatles ");
            // Give the UI time to update
            await page.WaitForTimeoutAsync(3000);
        }

        [Test, Order(1)]
        public async Task GetArtistsAPI()
        {
            using var playwright =
                await Microsoft.Playwright.Playwright.CreateAsync();

            var requestContext =
                await playwright.APIRequest.NewContextAsync(
                    new APIRequestNewContextOptions
                    {
                        BaseURL = "https://localhost:7282",
                        ExtraHTTPHeaders = new Dictionary<string, string>
                        {
                            { "Accept", "application/json" }
                        }
                    });

            // GET /api/artists
            var response =
                await requestContext.GetAsync("/api/artists");

            // Check response
            Assert.That(response.Ok, Is.True);
            Assert.That(response.Status, Is.EqualTo(200));

            // Get body
            var body = await response.TextAsync();

            TestContext.WriteLine("========== RESPONSE BODY ==========");
            TestContext.WriteLine(body);
            TestContext.WriteLine("===================================");

            Assert.That(body, Is.Not.Null.And.Not.Empty);

            // Deserialize
            var artists =
                JsonSerializer.Deserialize<List<Artist>>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            // Check list
            Assert.That(artists, Is.Not.Null);
            Assert.That(artists, Is.Not.Empty);

            // Print artists
            foreach (var artist in artists!)
            {
                TestContext.WriteLine(
                    $"ID: {artist.Id}");

                TestContext.WriteLine(
                    $"Name: [{artist.Name}]");

                TestContext.WriteLine(
                    $"Biography: [{artist.Biography}]");

                TestContext.WriteLine(
                    $"Image URL: [{artist.ImageURl}]");
            }

            // Find The Beatles
            var searchedArtist =
                artists.SingleOrDefault(
                    a => a.Name == "Name: The Beatles");

            // Verify artist exists
            Assert.That(
                searchedArtist,
                Is.Not.Null,
                "The Beatles was not found in the API response.");

            // Verify artist name
            Assert.That(
                searchedArtist!.Name,
                Is.EqualTo("Name: The Beatles"));
        }


        [Test, Order(2)]
        public async Task GetArtistByIdAPI()
        {
            // Create Playwright
            using var playwright =
                await Microsoft.Playwright.Playwright.CreateAsync();

            // Create API context
            var requestContext =
                await playwright.APIRequest.NewContextAsync(
                    new APIRequestNewContextOptions
                    {
                        BaseURL = "https://localhost:7282",
                        ExtraHTTPHeaders = new Dictionary<string, string>
                        {
                            { "Accept", "application/json" }
                        }
                    });

            // GET /api/artists/2
            var response =
                await requestContext.GetAsync("/api/artists/1010");

            // Verify status
            Assert.That(response.Status, Is.EqualTo(200));
            Assert.That(response.Ok, Is.True);

            // Get response body
            var body = await response.TextAsync();

            TestContext.WriteLine("========== RESPONSE ==========");
            TestContext.WriteLine(body);
            TestContext.WriteLine("==============================");

            // Verify body is not empty
            Assert.That(
                body,
                Is.Not.Null.And.Not.Empty,
                "Response body is empty.");

            // Deserialize
            var artist =
                JsonSerializer.Deserialize<Artist>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            // Verify artist exists
            Assert.That(artist, Is.Not.Null);

            // Print artist
            TestContext.WriteLine($"ID: {artist!.Id}");
            TestContext.WriteLine($"Name: {artist.Name}");
            TestContext.WriteLine($"Biography: {artist.Biography}");
            TestContext.WriteLine($"Image URL: {artist.ImageURl}");

            // Verify ID
            Assert.That(artist.Id, Is.EqualTo(1010));

            // Verify artist name
            Assert.That(
                artist.Name,
                Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(3)]
        public async Task CreateArtistAPI()
        {
            using var playwright =
                await Microsoft.Playwright.Playwright.CreateAsync();

            var browser =
                await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    });

            var context =
                await browser.NewContextAsync();

            var page = await context.NewPageAsync();

            // Login
            await page.GotoAsync("https://localhost:7282/Login/Login");

            await page.FillAsync("#UserName", "admin");
            await page.FillAsync("#Password", "password");

            await page.ClickAsync("button[type=submit]");


            var requestBody = new
            {
                name = "Test Artist",
                biography = "Test biography",
                imageUrl = "test.jpg"
            };

            // Use the API request associated with the browser context
            var response =
                await context.APIRequest.PostAsync(
                    "https://localhost:7282/api/artists",
                    new APIRequestContextOptions
                    {
                        DataObject = requestBody,
                        Headers = new Dictionary<string, string>
                        {
                    { "Content-Type", "application/json" },
                    { "Accept", "application/json" }
                        }
                    });

            var body = await response.TextAsync();

            TestContext.WriteLine("========== RESPONSE ==========");
            TestContext.WriteLine($"Status: {response.Status}");
            TestContext.WriteLine($"Body: {body}");
            TestContext.WriteLine("==============================");

            Assert.That(response.Status, Is.EqualTo(201));
            Assert.That(body, Is.Not.Empty);

            await browser.CloseAsync();
        }

        [Test, Order(4)]
        public async Task DeleteArtistAPI()
        {
            using var playwright =
                await Microsoft.Playwright.Playwright.CreateAsync();

            var browser =
                await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    });

            var context =
                await browser.NewContextAsync();

            var page =
                await context.NewPageAsync();

            // =========================
            // LOGIN
            // =========================

            await page.GotoAsync("https://localhost:7282/Login/Login");

            await page.FillAsync("#UserName", "admin");
            await page.FillAsync("#Password", "password");

            await page.ClickAsync("button[type=submit]");

            // =========================
            // DELETE ARTIST
            // =========================

            var response =
                await context.APIRequest.DeleteAsync(
                    "https://localhost:7282/api/artists/1011",
                    new APIRequestContextOptions
                    {
                        Headers = new Dictionary<string, string>
                        {
                             { "Accept", "application/json" }
                        }
                    });

            var body = await response.TextAsync();

            TestContext.WriteLine("========== DELETE ARTIST ==========");
            TestContext.WriteLine($"URL:         {response.Url}");
            TestContext.WriteLine($"Status:      {response.Status}");
            TestContext.WriteLine($"Status text: {response.StatusText}");
            TestContext.WriteLine($"Body:        {body}");
            TestContext.WriteLine("===================================");

            // Expected response
            Assert.That(
                response.Status,
                Is.EqualTo(204),
                $"Expected 204 but received {response.Status}. Body: {body}");

            await browser.CloseAsync();
        }
    }
}
 


