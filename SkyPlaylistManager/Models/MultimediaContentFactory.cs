using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models
{
    public class MultimediaContentFactory
    {
        private readonly Dictionary<string, Func<MultimediaContent>> _multimedaTypes;
        public JsonObject _args;

        public MultimediaContentFactory()
        {
            _multimedaTypes = new Dictionary<string, Func<MultimediaContent>>();
            _args = new JsonObject();
        }

        


        public MultimediaContent this[string multimediaType] => CreateMultimediaContentType(multimediaType);

        public MultimediaContent CreateMultimediaContentType(string multimediaType) =>
            _multimedaTypes[multimediaType]();
          

        public string[] RegistredMultimediaTypes => _multimedaTypes.Keys.ToArray();


        public void RegisterType(string multimediaContentType, Func<MultimediaContent> factoryMethod)
        {
            if (string.IsNullOrEmpty(multimediaContentType)) return;
            if (factoryMethod is null) return;

            _multimedaTypes[multimediaContentType] = factoryMethod;
        }


    }
}
