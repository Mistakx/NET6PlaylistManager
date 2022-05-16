using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Services;
using SkyPlaylistManager;
using SkyPlaylistManager.Models.Database.GenericResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("SkyPlaylistManagerDataBase"));

builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<PlaylistsService>();
builder.Services.AddSingleton<SessionTokensService>();
builder.Services.AddSingleton<GeneralizedResultsService>();
builder.Services.AddSingleton<MultimediaContentFactory>();
builder.Services.AddSingleton<FilesManager>();


builder.Services.AddScoped(_ =>
{
    var multimediaContentFactory = new MultimediaContentFactory();
    multimediaContentFactory.RegisterType("GenericVideoResult", () => new GenericVideoResult(multimediaContentFactory._args));
    multimediaContentFactory.RegisterType("GenericTrackResult", () => new GenericTrackResult(multimediaContentFactory._args));
    multimediaContentFactory.RegisterType("GenericLivestreamResult", () => new GenericLivestreamResult(multimediaContentFactory._args));
    return multimediaContentFactory;
});

builder.Services.AddControllersWithViews();

builder.Services.AddCors();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
app.UseSession();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
