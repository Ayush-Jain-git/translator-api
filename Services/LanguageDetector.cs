using NTextCat;

namespace TranslatorAPI.Services
{
    public class LanguageDetector
    {
        private readonly RankedLanguageIdentifier _identifier;

        public LanguageDetector()
        {
            var factory = new RankedLanguageIdentifierFactory();
            var path = Path.Combine(AppContext.BaseDirectory, "Core14.profile.xml");
_identifier = factory.Load(path);
            //_identifier = factory.Load("Core14.profile.xml");
        }

        public string Detect(string text)
        {
            var languages = _identifier.Identify(text);
            var best = languages.FirstOrDefault();

             if (best == null)
                 return "unknown";

            return best?.Item1.Iso639_3 ?? "unknown";
        }
    }
}