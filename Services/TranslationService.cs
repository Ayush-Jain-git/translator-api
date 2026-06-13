/*
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

       //private readonly string _sarvamKey = "sk_vl93m86d_A1PwJjHKywWqNfdMss03gUk6";
         private readonly string _sarvamKey = Environment.GetEnvironmentVariable("SARVAM_API_KEY") ?? " ";

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
             var factory = new RankedLanguageIdentifierFactory();
            //_identifier = factory.Load("Core14.profile.xml");
            var path = Path.Combine(AppContext.BaseDirectory, "Core14.profile.xml");
_identifier = factory.Load(path);
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

    // 1. Clean the target language string dynamically
    string cleanTarget = target.Replace("roman ", "", StringComparison.OrdinalIgnoreCase).Trim();
    cleanTarget = char.ToUpper(cleanTarget[0]) + cleanTarget.Substring(1);

    // 2. Build the generalized request body
    var requestBody = new
    {
        model = "sarvam-30b",
        max_tokens = 150,
        temperature = 0,
        reasoning_effort = (string)null, // Keeps compute costs low
        messages = new[]
        {
            new {
                role = "system",
                content = $@"You are a translation engine. Translate the input text into {cleanTarget} language, but you MUST write the output script using English characters (Roman script) phonetically so it can be read in a chat application. Never use native language alphabets. 

    Follow this exact formatting style:
    Input: <source_text>How are you?</source_text>
    Output: [Phonetic translation in {cleanTarget} written using English alphabet]

    Input: <source_text>Where are you going?</source_text>
    Output: [Phonetic translation in {cleanTarget} written using English alphabet]"
            },
            new {
                role = "user",
                content = $"<source_text>{text}</source_text>"
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

     if (!response.IsSuccessStatusCode)
    {
    throw new Exception(
        $"Sarvam API Error: {response.StatusCode}\n{result}"
    );
    }

        Console.WriteLine("===== SARVAM RESPONSE =====");
        Console.WriteLine(result);
        Console.WriteLine("===========================");

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
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NTextCat;

namespace TranslatorAPI.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly RankedLanguageIdentifier _identifier;
        private readonly string _sarvamKey = Environment.GetEnvironmentVariable("SARVAM_API_KEY") ?? " ";

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            var factory = new RankedLanguageIdentifierFactory();
            var path = Path.Combine(AppContext.BaseDirectory, "Core14.profile.xml");
            _identifier = factory.Load(path);
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
                "https://libretranslate.com",
                content
            );

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);

            if (doc.RootElement.TryGetProperty("translatedText", out var translated))
            {
                return translated.GetString();
            }

            return await CallSarvam(text, target);
        }
   
        private async Task<string> CallSarvam(string text, string target)
        {
            var apiKey = _sarvamKey;

            // 1. Clean and safely capitalize the target language string dynamically
            string cleanTarget = target.Replace("roman ", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (!string.IsNullOrEmpty(cleanTarget))
            {
                cleanTarget = char.ToUpper(cleanTarget[0]) + cleanTarget.Substring(1).ToLower();
            }
            else
            {
                cleanTarget = "English"; // Safe fallback
            }

            // 2. Build the generalized request body payload
               // 2. Build the generalized request body payload with explicit, real-world pattern matching
       // 2. Build the truly universal request body payload
       // 2. Build the comprehensive request body payload with robust example scenarios
    var requestBody = new
    {
        model = "sarvam-30b",
        max_tokens = 300, 
        temperature = 0,
        reasoning_effort = (string)null, // Completely disables reasoning loops to keep billing low
        messages = new[]
        {
            new {
                role = "system",
                content = $@"You are a universal cross-lingual translation engine. Translate the phrase inside the user's <source_text> XML tags into the {cleanTarget} language.

        CRITICAL RULES:
        1. If the target language is an Indian language (like Telugu, Hindi, Kannada, Tamil, etc.), you MUST write the output using English characters (Roman script) phonetically. Never use native alphabets like ఎక్కడికి or कहाँ.
        2. If the target language is English, output standard, grammatically correct English text.
        3. Output ONLY the raw converted words. Do not repeat the input words, do not add filler, and do not write explanations.
        4. Keep technical English nouns like 'Azure', 'Host', 'Server', 'Message', 'Database', 'API' as they are, but integrate them naturally into the target grammar.

        Follow these formatting styles based on the scenario:

        [Scenario A: Translating English into a Romanized Indian Language]
        Example 1 (Short Conversational):
        Input: <source_text>Where are you going right now?</source_text>
        Target Language: Telugu
        Output: Ippudu nuvvu ekkadiki velthunnavu

        Example 2 (Long / Technical):
        Input: <source_text>I successfully hosted the project application on azure cloud service, and now the output message will come back instantly.</source_text>
        Target Language: Telugu
        Output: Nenu project application ni azure cloud service lo successfully ga host chesaanu, ippudu output message ventane thirigi vasthundhi

        [Scenario B: Translating Code-Mixed / Romanized Input into English]
        Example 3 (Short Conversational):
        Input: <source_text>Naku aa item urgent ga kavali</source_text>
        Target Language: English
        Output: I need that item urgently

        Example 4 (Long Code-Mixed / Technical):
        Input: <source_text>main ye azure pe host krdia hai , and ye message convert hoke aaega</source_text>
        Target Language: English
        Output: I have hosted this on azure, and this message will come back converted

        Current Request:
        Input: <source_text>{text}</source_text>
        Target Language: {cleanTarget}
        Output:"
            },
            new {
                role = "user",
                content = "Process the current request now."
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

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Sarvam API Error: {response.StatusCode}\n{result}");
            }

            Console.WriteLine("===== SARVAM RESPONSE =====");
            Console.WriteLine(result);
            Console.WriteLine("===========================");

            using var doc = JsonDocument.Parse(result);

            var translation = doc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (!string.IsNullOrEmpty(translation))
            {
                // Remove reasoning block if it somehow slips past the system parameter
                if (translation.Contains("</think>"))
                {
                    translation = translation.Substring(translation.IndexOf("</think>") + "</think>".Length).Trim();
                }

                // Split by line and grab the first valid text row
                var lines = translation.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    translation = lines[0].Trim();
                }

                // Clean out any conversational prefixes, labels, or bracket artifacts generated by the model
                translation = Regex.Replace(translation, @"^(output|translation|text):\s*", "", RegexOptions.IgnoreCase);
                translation = translation.Trim('"', '[', ']', ' ', '.', '?');
            }

            return translation ?? "";
        }
    }
}
