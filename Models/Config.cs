using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace translation.Models
{
    public class Config
    {
        public const string DefaultTranslationPrompt = "请以涉猎广泛的翻译家，对用户输入的文本进行翻译";
        public const string DefaultExplanationPrompt = "请以资历丰富的老师身份，对用户输入的文字进行解释";
        public const string DefaultWordLookupPrompt = "请以资历丰富的老师身份，对用户输入的单词进行讲解，讲解单词的含义、用法、例句、句子成分等";
        public const string DefaultNoteOrganizePrompt = "你是一个学习笔记整理助手。请对用户提供的多条笔记内容进行主题聚类并输出整理稿。要求：1）先给出“主题清单”；2）按主题归纳要点（使用项目符号）；3）提炼“关键结论/后续行动”；4）仅基于提供内容，不编造信息；5）使用简体中文，结构清晰、可直接复习。";
        [JsonPropertyName("AI翻译提示词")]
        public string TranslationPrompt { get; set; }
        [JsonPropertyName("AI解释提示词")]
        public string ExplanationPrompt { get; set; }
        [JsonPropertyName("AI查询单词提示词")]
        public string WordLookupPrompt { get; set; }
        [JsonPropertyName("AI多笔记整理提示词")]
        public string NoteOrganizePrompt { get; set; }
        [JsonPropertyName("base_url")]
        public string BaseUrl { get; set; }
        [JsonPropertyName("API_key")]
        public string ApiKey { get; set; }
        [JsonPropertyName("AI_model")]
        public string AiModel { get; set; }
        public static Config Load(string path)
        {
            if (!File.Exists(path))
            {
                var defaultConfig = new Config
                {
                    TranslationPrompt = DefaultTranslationPrompt,
                    ExplanationPrompt = DefaultExplanationPrompt,
                    WordLookupPrompt = DefaultWordLookupPrompt,
                    NoteOrganizePrompt = DefaultNoteOrganizePrompt,
                    BaseUrl = "https://gpt.magiczotero.top/v1",
                    ApiKey = "sk-xxxx",
                    AiModel = "magic-translate-gpt"
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(defaultConfig, options);
                File.WriteAllText(path, json);
                return defaultConfig;
            }
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, true);
            string content = reader.ReadToEnd();
            var readOptions = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            var config = JsonSerializer.Deserialize<Config>(content, readOptions);
            return EnsureDefaults(config);
        }
        private static Config EnsureDefaults(Config? config)
        {
            config ??= new Config();
            if (string.IsNullOrWhiteSpace(config.TranslationPrompt)) config.TranslationPrompt = DefaultTranslationPrompt;
            if (string.IsNullOrWhiteSpace(config.ExplanationPrompt)) config.ExplanationPrompt = DefaultExplanationPrompt;
            if (string.IsNullOrWhiteSpace(config.WordLookupPrompt)) config.WordLookupPrompt = DefaultWordLookupPrompt;
            if (string.IsNullOrWhiteSpace(config.NoteOrganizePrompt)) config.NoteOrganizePrompt = DefaultNoteOrganizePrompt;
            if (string.IsNullOrWhiteSpace(config.BaseUrl)) config.BaseUrl = "https://gpt.magiczotero.top/v1";
            if (string.IsNullOrWhiteSpace(config.AiModel)) config.AiModel = "magic-translate-gpt";
            return config;
        }
        public void Save(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }
    }
}
