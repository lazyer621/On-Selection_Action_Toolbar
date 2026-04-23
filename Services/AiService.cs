using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using translation.Models;
namespace translation.Services
{
    public class AiService
    {
        private readonly HttpClient _httpClient;
        public AiService()
        {
            _httpClient = new HttpClient();
        }
        public async Task<string> TranslateAsync(string text)
        {
            var config = ConfigManager.LoadConfig();
            return await CallApiAsync(config, config.TranslationPrompt, text);
        }
        public async Task<string> ExplainAsync(string text)
        {
            var config = ConfigManager.LoadConfig();
            return await CallApiAsync(config, config.ExplanationPrompt, text);
        }
        public async Task<string> LookupWordAsync(string text)
        {
            var config = ConfigManager.LoadConfig();
            return await CallApiAsync(config, config.WordLookupPrompt, text);
        }
        public async Task<string> GenerateTagsAsync(string text)
        {
            var config = ConfigManager.LoadConfig();
            var systemPrompt = string.IsNullOrWhiteSpace(config.NoteOrganizePrompt)
                ? Config.DefaultNoteOrganizePrompt
                : config.NoteOrganizePrompt;
            var userText = $"以下是待整理的多条笔记，请直接输出整理后的笔记内容：\n\n{text}";
            return await CallApiAsync(config, systemPrompt, userText);
        }
        public async Task<string> AskQuestionAsync(string referenceText, string question)
        {
            var config = ConfigManager.LoadConfig();
            var systemPrompt = "你是一个阅读助手，帮助用户理解和分析文本内容。";
            var userText = $"【参考文本】{referenceText}\n【用户问题】{question}\n请基于参考文本回答问题，如文本不足以回答则说明。";
            return await CallApiAsync(config, systemPrompt, userText);
        }
        public async Task<string> AskWithContextAsync(string allNotesContext, string question)
        {
            var config = ConfigManager.LoadConfig();
            var systemPrompt = $"你是用户的个人知识助手，以下是用户近期保存的所有笔记摘录。\n{allNotesContext}";
            return await CallApiAsync(config, systemPrompt, question);
        }
        private async Task<string> CallApiAsync(Config config, string systemPrompt, string userContent)
        {
            var baseUrl = config.BaseUrl?.Trim().TrimEnd('/');
            if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.EndsWith("/chat/completions"))
            {
                baseUrl += "/chat/completions";
            }
            var apiKey = config.ApiKey?.Trim();
            try
            {
                var requestBody = new ChatRequest
                {
                    Model = config.AiModel,
                    Messages = new List<Message>
                    {
                        new Message { Role = "system", Content = systemPrompt },
                        new Message { Role = "user", Content = userContent }
                    },
                    Temperature = 0
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} ({(int)response.StatusCode}). {errorContent}. URL: {baseUrl}";
                }
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChatResponse>(responseJson);
                if (result?.Choices != null && result.Choices.Count > 0)
                {
                    return result.Choices[0].Message.Content;
                }
                return "No response from AI.";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}. URL: {baseUrl}";
            }
        }
    }
}
