using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using NTextCat;

namespace TranslatorAPI.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly RankedLanguageIdentifier _identifier;

        private readonly string _sarvamKey = "sk_vl93m86d_A1PwJjHKywWqNfdMss03gUk6";

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
             var factory = new RankedLanguageIdentifierFactory();
            _identifier = factory.Load("Core14.profile.xml");
        }

    public async Task<string> TranslateAsync(string text, string target)
    {   
        List<string> LibreSupported = new List<string>
        { "en", "es", "fr", "de", "it", "pt", "ru", "ar", "zh", "ja" };

        var languages = _identifier.Identify(text).ToList();
        var bestLanguage = languages.FirstOrDefault();

        string detectedLanguage = "unknown";
        double score = 1;

        if (bestLanguage != null)
        {
            score = bestLanguage.Item2;
            detectedLanguage = bestLanguage.Item1.Iso639_3 ?? "unknown";
        }

        bool useSarvam =
            bestLanguage == null ||
            score > 0.21 ||
            !LibreSupported.Contains(detectedLanguage) ||
            !LibreSupported.Contains(target);

        if (useSarvam)
        {
            Console.WriteLine("Using Sarvam");
            return await CallSarvam(text, target);
        }

        Console.WriteLine("Using LibreTranslate");

        var requestBody = new
        {
            q = text,
            source = detectedLanguage,
            target = target,
            format = "text"
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            "https://libretranslate.com/translate",
            content
        );

        var result = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(result);

        if (doc.RootElement.TryGetProperty("translatedText", out var translated))
        {
            return translated.GetString();
        }

        return await CallSarvam(text, target);
    }
   
    private async Task<string> CallSarvam(string text, string target)
    {
        var apiKey = _sarvamKey;

        var requestBody = new
        {
            model = "sarvam-m",
            max_tokens = 750,
            temperature = 0,
            messages = new[]
            {
                new {
                    role = "system",
                    content = $"You are a translator. Never explain. Never think. Only return the translated sentence in {target} language."
                },
                new {
                    role = "user",
                    content = text
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.sarvam.ai/v1/chat/completions");

        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);

    var translation = doc
    .RootElement
    .GetProperty("choices")[0]
    .GetProperty("message")
    .GetProperty("content")
    .GetString();

if (!string.IsNullOrEmpty(translation))
{
    // Remove reasoning block
    if (translation.Contains("</think>"))
    {
        translation = translation.Substring(
            translation.IndexOf("</think>") + "</think>".Length
        ).Trim();
    }

    // Take first non-empty line
    var lines = translation.Split('\n');

    foreach (var line in lines)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            translation = line.Trim();
            break;
        }
    }
}

return translation ?? "";

    }
    }
}