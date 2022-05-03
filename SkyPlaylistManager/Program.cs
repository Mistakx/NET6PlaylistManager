using System.Text.Json.Nodes;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Services;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("SkyPlaylistManagerDataBase"));

builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<PlaylistsService>();
builder.Services.AddSingleton<MultimediaContentsService>();
builder.Services.AddSingleton<MultimediaContentFactory>();

builder.Services.AddScoped<MultimediaContentFactory>(_ =>
{
    MultimediaContentFactory multimediaContentFactory = new MultimediaContentFactory();

    
    multimediaContentFactory.RegisterType("Youtube", () => new VideosContent(multimediaContentFactory._args));
    multimediaContentFactory.RegisterType("Spotify", () => new TracksContent(multimediaContentFactory._args));
    multimediaContentFactory.RegisterType("Twitch", () => new LivestreamsContent(multimediaContentFactory._args));
    return multimediaContentFactory;
});

builder.Services.AddControllersWithViews();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
