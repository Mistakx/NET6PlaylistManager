using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.Database.GenericResults;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models
{
    public class MultimediaContentFactory
    {
        private readonly Dictionary<string, Func<GenericResult>> _multimediaTypes;
        public JsonObject _args;

        public MultimediaContentFactory()
        {
            _multimediaTypes = new Dictionary<string, Func<GenericResult>>();
            _args = new JsonObject();
        }

        


        public GenericResult this[string multimediaType] => CreateMultimediaContentType(multimediaType);

        public GenericResult CreateMultimediaContentType(string multimediaType) =>
            _multimediaTypes[multimediaType]();
          

        public string[] RegistredMultimediaTypes => _multimediaTypes.Keys.ToArray();


        public void RegisterType(string multimediaContentType, Func<GenericResult> factoryMethod)
        {
            if (string.IsNullOrEmpty(multimediaContentType)) return;
            if (factoryMethod is null) return;

            _multimediaTypes[multimediaContentType] = factoryMethod;
        }


    }
}
