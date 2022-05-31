using Microsoft.AspNetCore.Mvc;

namespace SkyPlaylistManager.Controllers
{
    public class ImageController : ControllerBase
    {
        [HttpGet("GetImage/{folder}/{imageName}")] // https://stackoverflow.com/questions/186062/can-an-asp-net-mvc-controller-return-an-image
        public Task<IActionResult> GetImage(string imageName, string folder)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", folder, imageName);
            return Task.FromResult<IActionResult>(PhysicalFile(path, "image/jpeg"));
        }
    }
}
