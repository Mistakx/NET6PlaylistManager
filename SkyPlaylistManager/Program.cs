using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Services;
using SkyPlaylistManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("SkyPlaylistManagerDatabase"));

builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<PlaylistsService>();
builder.Services.AddSingleton<SessionTokensService>();
builder.Services.AddSingleton<ContentService>();
builder.Services.AddSingleton<UserRecommendationsService>();
builder.Services.AddSingleton<PlaylistRecommendationsService>();
builder.Services.AddSingleton<ContentRecommendationsService>();
builder.Services.AddSingleton<FilesManager>();
builder.Services.AddSingleton<CommunityService>();
builder.Services.AddSingleton<SignalRService>();

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

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

app.MapHub<SignalRService>("/hub");

app.Run();
