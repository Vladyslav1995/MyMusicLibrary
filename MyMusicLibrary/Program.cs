using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyMusicLibrary.MusicData;
using System.Security.Claims;
using MyMusicLibrary;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));
/*
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Music Library API",
        Version = "v1",
        Description = "API for managing artists, albums and songs."
    });
});
*/
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim(
            ClaimTypes.Name,
            "admin");
    });

    options.AddPolicy("User", policy =>
    {
        policy.RequireClaim(
            ClaimTypes.Name,
            "user");
    });
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

//app.UseSwagger();
//app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<MyMusicLibrary.Hubs.SearchHub>("/searchHub");
app.MapRazorPages()
   .WithStaticAssets();

// ===============================
// Minimal API - Artists
// ===============================

app.MapGet("/api/artists", async (ApplicationDbContext db) =>
{
    var artists = await db.Artists
        .AsNoTracking()
        .OrderBy(a => a.Name)
        .Select(a => new
        {
            a.Id,
            a.Name,
            a.Biography,
            a.ImageURl
        })
        .ToListAsync();

    return Results.Ok(artists);
});

app.MapGet("/api/artists/{id:int}", async (
    int id,
    ApplicationDbContext db) =>
{
    var artist = await db.Artists
        .AsNoTracking()
        .Where(a => a.Id == id)
        .Select(a => new
        {
            a.Id,
            a.Name,
            a.Biography,
            a.ImageURl
        })
        .FirstOrDefaultAsync();

    if (artist == null)
        return Results.NotFound();

    return Results.Ok(artist);
});

app.MapPost("/api/artists", async (
    CreateArtistRequest request,
    ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new
        {
            message = "Artist name is required."
        });

    var artist = new Artist
    {
        Name = request.Name.Trim(),
        Biography = request.Biography,
        ImageURl = request.ImageUrl
    };

    db.Artists.Add(artist);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/artists/{artist.Id}",
        artist);
});
//.RequireAuthorization("AdminOnly");

app.MapPut("/api/artists/{id:int}", async (
    int id,
    UpdateArtistRequest request,
    ApplicationDbContext db) =>
{
    var artist = await db.Artists
        .FirstOrDefaultAsync(a => a.Id == id);

    if (artist == null)
        return Results.NotFound(new
        {
            message = "Artist not found."
        });

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new
        {
            message = "Artist name is required."
        });

    artist.Name = request.Name.Trim();
    artist.Biography = request.Biography;
    artist.ImageURl = request.ImageUrl;

    await db.SaveChangesAsync();

    return Results.Ok(artist);
})
.RequireAuthorization("AdminOnly");

app.MapDelete("/api/artists/{id:int}", async (
    int id,
    ApplicationDbContext db) =>
{
    var artist = await db.Artists
        .FirstOrDefaultAsync(a => a.Id == id);

    if (artist == null)
        return Results.NotFound(new
        {
            message = "Artist not found."
        });

    var hasAlbums = await db.Albums
        .AnyAsync(a => a.ArtistId == id);

    if (hasAlbums)
        return Results.BadRequest(new
        {
            message = "Cannot delete artist because the artist has albums."
        });

    db.Artists.Remove(artist);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
//.RequireAuthorization("AdminOnly")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);


// ===============================
// Minimal API - Albums
// ===============================

app.MapGet("/api/albums", async (ApplicationDbContext db) =>
{
    var albums = await db.Albums
        .AsNoTracking()
        .Include(a => a.Artist)
        .OrderBy(a => a.Name)
        .Select(a => new
        {
            a.Id,
            a.Name,
            a.ReleaseYear,
            a.CoverUrl,
            a.ArtistId,
            ArtistName = a.Artist != null
                ? a.Artist.Name
                : ""
        })
        .ToListAsync();

    return Results.Ok(albums);
});

app.MapGet("/api/albums/{id:int}", async (
    int id,
    ApplicationDbContext db) =>
{
    var album = await db.Albums
        .AsNoTracking()
        .Include(a => a.Artist)
        .Where(a => a.Id == id)
        .Select(a => new
        {
            a.Id,
            a.Name,
            a.ReleaseYear,
            a.CoverUrl,
            a.ArtistId,
            ArtistName = a.Artist != null
                ? a.Artist.Name
                : ""
        })
        .FirstOrDefaultAsync();

    if (album == null)
        return Results.NotFound();

    return Results.Ok(album);
});

app.MapPost("/api/albums", async (
    CreateAlbumRequest request,
    ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new
        {
            message = "Album name is required."
        });

    var artistExists = await db.Artists
        .AnyAsync(a => a.Id == request.ArtistId);

    if (!artistExists)
        return Results.BadRequest(new
        {
            message = "Artist does not exist."
        });

    var album = new Album
    {
        Name = request.Name.Trim(),
        ReleaseYear = request.ReleaseYear,
        CoverUrl = request.CoverUrl,
        ArtistId = request.ArtistId
    };

    db.Albums.Add(album);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/albums/{album.Id}",
        album);
})
.RequireAuthorization("AdminOnly");

app.MapPut("/api/albums/{id:int}", async (
    int id,
    UpdateAlbumRequest request,
    ApplicationDbContext db) =>
{
    var album = await db.Albums
        .FirstOrDefaultAsync(a => a.Id == id);

    if (album == null)
        return Results.NotFound(new
        {
            message = "Album not found."
        });

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new
        {
            message = "Album name is required."
        });

    var artistExists = await db.Artists
        .AnyAsync(a => a.Id == request.ArtistId);

    if (!artistExists)
        return Results.BadRequest(new
        {
            message = "Artist does not exist."
        });

    album.Name = request.Name.Trim();
    album.ReleaseYear = request.ReleaseYear;
    album.CoverUrl = request.CoverUrl;
    album.ArtistId = request.ArtistId;

    await db.SaveChangesAsync();

    return Results.Ok(album);
})
.RequireAuthorization("AdminOnly");

app.MapDelete("/api/albums/{id:int}", async (
    int id,
    ApplicationDbContext db) =>
{
    var album = await db.Albums
        .FirstOrDefaultAsync(a => a.Id == id);

    if (album == null)
        return Results.NotFound(new
        {
            message = "Album not found."
        });

    var hasSongs = await db.Songs
        .AnyAsync(s => s.AlbumId == id);

    if (hasSongs)
        return Results.BadRequest(new
        {
            message = "Cannot delete album because the album has songs."
        });

    db.Albums.Remove(album);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization("AdminOnly");


// ===============================
// Minimal API - Songs
// ===============================

app.MapGet("/api/songs", async (ApplicationDbContext db) =>
{
    var songs = await db.Songs
        .AsNoTracking()
        .Include(s => s.Album)
        .ThenInclude(a => a!.Artist)
        .OrderBy(s => s.Name)
        .Select(s => new
        {
            s.Id,
            s.Name,
            s.DurationSeconds,
            s.AudioUrl,
            s.AlbumId,

            AlbumName = s.Album != null
                ? s.Album.Name
                : "",

            ArtistName = s.Album != null &&
                         s.Album.Artist != null
                ? s.Album.Artist.Name
                : ""
        })
        .ToListAsync();

    return Results.Ok(songs);
});

app.MapGet("/api/songs/{id:int}", async (
    int id,
    ApplicationDbContext db) =>
{
    var song = await db.Songs
        .AsNoTracking()
        .Include(s => s.Album)
        .ThenInclude(a => a!.Artist)
        .Where(s => s.Id == id)
        .Select(s => new
        {
            s.Id,
            s.Name,
            s.DurationSeconds,
            s.AudioUrl,
            s.AlbumId,

            AlbumName = s.Album != null
                ? s.Album.Name
                : "",

            ArtistName = s.Album != null &&
                         s.Album.Artist != null
                ? s.Album.Artist.Name
                : ""
        })
        .FirstOrDefaultAsync();

    if (song == null)
        return Results.NotFound();

    return Results.Ok(song);
});

app.MapPost("/api/songs", async (
    CreateSongRequest request,
    ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new
        {
            message = "Song name is required."
        });

    if (request.DurationSeconds < 0)
        return Results.BadRequest(new
        {
            message = "Duration cannot be negative."
        });

    var albumExists = await db.Albums
        .AnyAsync(a => a.Id == request.AlbumId);

    if (!albumExists)
        return Results.BadRequest(new
        {
            message = "Album does not exist."
        });

    var song = new Song
    {
        Name = request.Name.Trim(),
        DurationSeconds = request.DurationSeconds,
        AlbumId = request.AlbumId,
        AudioUrl = request.AudioUrl
    };

    db.Songs.Add(song);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/songs/{song.Id}",
        song);
})
.RequireAuthorization("AdminOnly");

app.MapPut("/api/songs/{id:int}", async (
    int id,
    UpdateSongRequest request,
    ApplicationDbContext db) =>
{
    var song = await db.Songs
        .FirstOrDefaultAsync(s => s.Id == id);

    if (song == null)
        return Results.NotFound(new
        {
            message = "Song not found."
        });

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new
        {
            message = "Song name is required."
        });

    if (request.DurationSeconds < 0)
        return Results.BadRequest(new
        {
            message = "Duration cannot be negative."
        });

    var albumExists = await db.Albums
        .AnyAsync(a => a.Id == request.AlbumId);

    if (!albumExists)
        return Results.BadRequest(new
        {
            message = "Album does not exist."
        });

    song.Name = request.Name.Trim();
    song.DurationSeconds = request.DurationSeconds;
    song.AlbumId = request.AlbumId;
    song.AudioUrl = request.AudioUrl;

    await db.SaveChangesAsync();

    return Results.Ok(song);
})
.RequireAuthorization("AdminOnly");

app.MapDelete("/api/songs/{id:int}", async (
    int id,
    ApplicationDbContext db) =>
{
    var song = await db.Songs
        .FirstOrDefaultAsync(s => s.Id == id);

    if (song == null)
        return Results.NotFound(new
        {
            message = "Song not found."
        });

    db.Songs.Remove(song);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization("AdminOnly");

app.Run();
