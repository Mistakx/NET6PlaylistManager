using SkyPlaylistManager.Services;

namespace SkyPlaylistManager
{

    public interface IFileManager
    {
        bool IsValidImage(IFormFile file);
        string InsertInDiretory(IFormFile file);
        void DeleteFromDiretory();
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

        public string InsertInDiretory(IFormFile file)
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


        public async void DeleteFromDiretory()
        {
            try
            {
                var oldPhoto = await _usersService.GetUserProfilePhoto("6261707eff67ad3d4f51d38b"); // TODO: Mudar para o Id da sessão
                string oldPhotopath = Path.Combine(Directory.GetCurrentDirectory(), "Images", (string)oldPhoto["profilePhotoPath"]);
                FileInfo oldPhotoFileInfo = new FileInfo(oldPhotopath);
                oldPhotoFileInfo.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

       
    }






}
