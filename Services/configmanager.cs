using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Linq;
using translation.Models;
namespace translation.Services
{
    public static class ConfigManager
    {
        private static readonly string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        public static Config LoadConfig()
        {
            string exeDir = Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string configPath = Path.Combine(exeDir, "config.json");
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true 
            };
            if (File.Exists(configPath))
            {
                try
                {
                    using var stream = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, true);
                    string json = reader.ReadToEnd();
                    var config = JsonSerializer.Deserialize<Config>(json, options);
                    if (config != null) return config;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"读取或解析 config.json 失败，将使用内置默认配置。\n请检查配置文件格式。\n错误详细信息: {ex.Message}", 
                        "配置读取错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            string resourceName = ResourceNames.FirstOrDefault(r => r.EndsWith("config.json"));
            if (!string.IsNullOrEmpty(resourceName))
            {
                try
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string json = reader.ReadToEnd();
                                return JsonSerializer.Deserialize<Config>(json, options);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return new Config
            {
                TranslationPrompt = Config.DefaultTranslationPrompt,
                ExplanationPrompt = Config.DefaultExplanationPrompt,
                WordLookupPrompt = Config.DefaultWordLookupPrompt,
                NoteOrganizePrompt = Config.DefaultNoteOrganizePrompt,
                BaseUrl = "https://gpt.magiczotero.top/v1",
                ApiKey = "sk-xxxx",
                AiModel = "magic-translate-gpt"
            };
        }
    }
}
