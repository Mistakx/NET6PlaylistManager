using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Services;
using SkyPlaylistManager;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("SkyPlaylistManagerDatabase"));

builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<PlaylistsService>();
builder.Services.AddSingleton<SessionTokensService>();
builder.Services.AddSingleton<GeneralizedResultsService>();
builder.Services.AddSingleton<GeneralizedResultFactory>();
builder.Services.AddSingleton<RecommendationsService>();
builder.Services.AddSingleton<FilesManager>();


builder.Services.AddScoped(_ =>
{
    var generalizedResultFactory = new GeneralizedResultFactory();
    generalizedResultFactory.RegisterType("GenericVideoResult", () => new GeneralizedVideoResult(generalizedResultFactory.Request));
    generalizedResultFactory.RegisterType("GenericTrackResult", () => new GeneralizedTrackResult(generalizedResultFactory.Request));
    generalizedResultFactory.RegisterType("GenericLivestreamResult", () => new GeneralizedLivestreamResult(generalizedResultFactory.Request));
    return generalizedResultFactory;
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
