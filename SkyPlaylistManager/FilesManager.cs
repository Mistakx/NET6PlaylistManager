using SkyPlaylistManager.Services;

namespace SkyPlaylistManager
{
    public interface IFileManager
    {
        bool IsValidImage(IFormFile file);
        string InsertInDirectory(IFormFile file, string folder);
        void DeleteFromDirectory(string sessionToken, string folder);
    }

    public class FilesManager : IFileManager
    {
        private readonly UsersService _usersService;

        public FilesManager(UsersService usersService)
        {
            _usersService = usersService;
        }


        public bool IsValidImage(IFormFile file)
        {
            FileInfo fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension == ".jpg" || fileInfo.Extension == ".png" ||
                fileInfo.Extension == ".jpeg") return true;

            else return false;
        }

        public string InsertInDirectory(IFormFile file, string folder)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file.FileName);

                var destinationFolder = Path.Combine("Images", folder);

                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                string generatedFileName = string.Format(@"{0}" + fileInfo.Extension, Guid.NewGuid());
                string directoryFilePath = Path.Combine(destinationFolder, generatedFileName);

                using (var stream = new FileStream(directoryFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return generatedFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ("Error uploading file");
            }
        }


        public void DeleteFromDirectory(string fileToDelete, string folder)
        {
            try
            {
                var fileName = fileToDelete.Replace("GetImage/" + folder +"/", "");
                
                if (fileName == "DefaultUserPhoto.jpeg") return;

                string completeFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Images/", folder, fileName);
                FileInfo completeFilePathInfo = new FileInfo(completeFilePath);
                completeFilePathInfo.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}