namespace TranslatorAPI.Models
{
    public class TranslateRequest
    {
        public string Text { get; set; }
        public string TargetLanguage { get; set; }
    }
}