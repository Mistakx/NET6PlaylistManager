using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.GeneralizedResults;

namespace SkyPlaylistManager.Models
{
    public class GeneralizedResultFactory
    {
        private readonly Dictionary<string, Func<GeneralizedResult>> _multimediaTypes;
        public JsonObject Request { get; set; }

        public GeneralizedResultFactory()
        {
            _multimediaTypes = new Dictionary<string, Func<GeneralizedResult>>();
            Request = new JsonObject();
        }


        public GeneralizedResult this[string multimediaType] => CreateMultimediaContentType(multimediaType);

        public GeneralizedResult CreateMultimediaContentType(string multimediaType) =>
            _multimediaTypes[multimediaType]();


        public string[] RegisteredMultimediaTypes => _multimediaTypes.Keys.ToArray();


        public void RegisterType(string multimediaContentType, Func<GeneralizedResult> factoryMethod)
        {
            if (string.IsNullOrEmpty(multimediaContentType)) return;
            if (factoryMethod.Equals( null)) return;

            _multimediaTypes[multimediaContentType] = factoryMethod;
        }
    }
}