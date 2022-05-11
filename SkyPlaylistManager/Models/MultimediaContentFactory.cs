using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models
{
    public class MultimediaContentFactory
    {
        private readonly Dictionary<string, Func<MultimediaContent>> _multimediaTypes;
        public JsonObject _args;

        public MultimediaContentFactory()
        {
            _multimediaTypes = new Dictionary<string, Func<MultimediaContent>>();
            _args = new JsonObject();
        }

        


        public MultimediaContent this[string multimediaType] => CreateMultimediaContentType(multimediaType);

        public MultimediaContent CreateMultimediaContentType(string multimediaType) =>
            _multimediaTypes[multimediaType]();
          

        public string[] RegistredMultimediaTypes => _multimediaTypes.Keys.ToArray();


        public void RegisterType(string multimediaContentType, Func<MultimediaContent> factoryMethod)
        {
            if (string.IsNullOrEmpty(multimediaContentType)) return;
            if (factoryMethod is null) return;

            _multimediaTypes[multimediaContentType] = factoryMethod;
        }


    }
}
