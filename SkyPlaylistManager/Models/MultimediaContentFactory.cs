using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models
{
    public class MultimediaContentFactory
    {
        private readonly Dictionary<string, Func<MultimediaContent>> _multimedaTypes;

        public MultimediaContentFactory()
        {
            _multimedaTypes = new Dictionary<string, Func<MultimediaContent>>();

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


        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddScoped<MultimediaContentFactory>(_ =>
        //    {
        //        MultimediaContentFactory multimediaContentFactory = new MultimediaContentFactory();

        //        multimediaContentFactory.RegisterType("Youtube", () => new VideosContent());
        //        multimediaContentFactory.RegisterType("Spotify", () => new TracksContent());
        //        multimediaContentFactory.RegisterType("Twitch", () => new LivestreamsContent());
        //        return multimediaContentFactory;
        //    });

        //}

    }
}
