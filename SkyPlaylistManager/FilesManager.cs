using SkyPlaylistManager.Services;

namespace SkyPlaylistManager
{

    public interface IFileManager
    {
        bool IsValidImage(IFormFile file);
        string InsertInDirectory(IFormFile file);
        void DeleteFromDirectory(string sessionToken);
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

        public string InsertInDirectory(IFormFile file)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file.FileName);
           
                if (!Directory.Exists("Images"))
                    Directory.CreateDirectory("Images");

                string generatedFileName = string.Format(@"{0}" + fileInfo.Extension, Guid.NewGuid());
                string directoryFilePath = Path.Combine("Images", generatedFileName);
                
                using (var stream = new FileStream(directoryFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return generatedFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ("Erro ao inserir ao fazer upload do ficheiro.");
            }
        }


        public async void DeleteFromDirectory(string sessionToken)
        {
            try
            {
                var oldPhoto = await _usersService.GetUserProfilePhoto(sessionToken);
                string oldUserPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "Images/", (string)oldPhoto["profilePhotoUrl"]);
                oldUserPhotoPath = oldUserPhotoPath.Replace("User/GetImage/", "");
                FileInfo oldPhotoFileInfo = new FileInfo(oldUserPhotoPath);
                oldPhotoFileInfo.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

       
    }






}
