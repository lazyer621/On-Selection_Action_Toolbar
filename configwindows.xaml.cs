using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WpfInput = System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Net.Http;
using translation.Models;
using System.Text.Json;
using WpfPoint = System.Windows.Point;
namespace translation
{
    public partial class ConfigWindow : Window
    {
        private Config? _config;
        private string _configPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private bool _isApiKeyVisible;
        public ConfigWindow()
        {
            InitializeComponent();
            MinWidth  = 400;
            MinHeight = 300;
            txtConfigPath.Text = _configPath;
            LoadConfiguration();
            UpdateStatus();
        }
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
            => ToggleMaximize();
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;
        private void BtnClose_Click(object sender, RoutedEventArgs e)
            => Close();
        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        private async void BtnTestApi_Click(object sender, RoutedEventArgs e)
        {
            string baseUrl = txtBaseUrl.Text.Trim();
            string apiKey = _isApiKeyVisible ? txtApiKeyVisible.Text : txtApiKey.Password;
            string model = txtAiModel.Text.Trim();
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
            {
                txtLogOutput.Text = $"[{DateTime.Now:HH:mm:ss}] 错误: Base URL 和 API Key 不能为空。";
                txtLogOutput.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            txtLogOutput.Text = $"[{DateTime.Now:HH:mm:ss}] 正在测试连接...";
            txtLogOutput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A8FC0"));
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                client.Timeout = TimeSpan.FromSeconds(15);
                string endpoint = baseUrl.TrimEnd('/');
                if (!endpoint.EndsWith("/models") && !endpoint.EndsWith("/chat/completions"))
                {
                    endpoint += "/models";
                }
                HttpResponseMessage response;
                if (endpoint.EndsWith("/chat/completions"))
                {
                    var reqBody = new { model = model, messages = new[] { new { role = "user", content = "Hi" } }, max_tokens = 1 };
                    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(reqBody), System.Text.Encoding.UTF8, "application/json");
                    response = await client.PostAsync(endpoint, content);
                }
                else
                {
                    response = await client.GetAsync(endpoint);
                }
                HandleResponse(response);
            }
            catch (Exception ex)
            {
                txtLogOutput.Text = $"[{DateTime.Now:HH:mm:ss}] 测试失败:\n{ex.Message}";
                txtLogOutput.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
        private void HandleResponse(HttpResponseMessage response)
        {
            int statusCode = (int)response.StatusCode;
            string logMsg = $"[{DateTime.Now:HH:mm:ss}] 状态码: {statusCode}\n";
            if (response.IsSuccessStatusCode)
            {
                logMsg += "连接测试成功！API 配置有效。";
                txtLogOutput.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                txtLogOutput.Foreground = new SolidColorBrush(Colors.OrangeRed);
                switch (statusCode)
                {
                    case 401:
                        logMsg += "成因: API Key 不正确或已过期。\n解决: 请检查并更新 API Key。";
                        break;
                    case 403:
                        logMsg += "成因: 权限不足。\n解决: 请确认该地区可用或账号未被封禁。";
                        break;
                    case 404:
                        logMsg += "成因: 请求路径不存在。\n解决: 请确认 Base URL 是否正确。";
                        break;
                    case 429:
                        logMsg += "成因: 额度用尽或请求太频繁。\n解决: 检查账号余额或降低请求频率。";
                        break;
                    case 500:
                    case 502:
                    case 503:
                    case 504:
                        logMsg += "成因: 服务器网关或内部错误。\n解决: 请稍后再试或联系服务商。";
                        break;
                    default:
                        logMsg += "成因: 未知错误。\n解决: 请检查网络或 API 配置。";
                        break;
                }
            }
            txtLogOutput.Text = logMsg;
        }
        private void BtnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "加载配置文件",
                Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                InitialDirectory = Path.GetDirectoryName(_configPath)
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _config = Config.Load(dialog.FileName) ?? new Config();
                    UpdateUIFromConfig();
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "加载失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadConfiguration()
        {
            try
            {
                _config = Config.Load(_configPath) ?? new Config();
            }
            catch
            {
                _config = new Config();
            }
            UpdateUIFromConfig();
        }
        private void UpdateUIFromConfig()
        {
            txtTranslationPrompt.Text = _config.TranslationPrompt;
            txtExplanationPrompt.Text = _config.ExplanationPrompt;
            txtWordQueryPrompt.Text   = _config.WordLookupPrompt;
            txtBaseUrl.Text           = _config.BaseUrl;
            txtApiKey.Password        = _config.ApiKey;
            if (_isApiKeyVisible) txtApiKeyVisible.Text = _config.ApiKey;
            txtAiModel.Text           = _config.AiModel;
        }
        private void OnFieldChanged(object sender, RoutedEventArgs e)
            => UpdateStatus();
        private List<string> GetMissingFields()
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(txtTranslationPrompt.Text)) missing.Add("AI 翻译提示词");
            if (string.IsNullOrWhiteSpace(txtExplanationPrompt.Text)) missing.Add("AI 解释提示词");
            if (string.IsNullOrWhiteSpace(txtWordQueryPrompt.Text))   missing.Add("AI 查询单词提示词");
            if (string.IsNullOrWhiteSpace(txtBaseUrl.Text))           missing.Add("Base URL");
            string apiKey = _isApiKeyVisible ? txtApiKeyVisible.Text : txtApiKey.Password;
            if (string.IsNullOrWhiteSpace(apiKey)) missing.Add("API Key");
            if (string.IsNullOrWhiteSpace(txtAiModel.Text))           missing.Add("AI Model");
            return missing;
        }
        private void UpdateStatus()
        {
            if (statusDot == null || statusText == null) return;
            var missing  = GetMissingFields();
            int total    = 6;
            int done     = total - missing.Count;
            bool complete = missing.Count == 0;
            statusDot.Fill = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(complete ? "#22C55E" : "#EF9F27"));
            statusText.Text = complete
                ? "配置完整 — 所有项已完成"
                : $"配置未完整 — {done} / {total} 项已完成";
            if (progressFill != null)
                progressFill.Width = 144.0 * done / total;
            if (lblProgressCount != null)
                lblProgressCount.Text = $"{done} / {total} 已配置";
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _config ??= new Config();
            _config.TranslationPrompt = txtTranslationPrompt.Text;
            _config.ExplanationPrompt = txtExplanationPrompt.Text;
            _config.WordLookupPrompt   = txtWordQueryPrompt.Text;
            _config.BaseUrl           = txtBaseUrl.Text;
            _config.ApiKey            = _isApiKeyVisible ? txtApiKeyVisible.Text : txtApiKey.Password;
            _config.AiModel           = txtAiModel.Text;
            var missing = GetMissingFields();
            if (missing.Any())
            {
                MessageBox.Show($"以下配置项未完全填写：\n- {string.Join("\n- ", missing)}", 
                                "配置未完整", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                _config.Save(_configPath);
                UpdateStatus();
                MessageBox.Show("配置保存成功！", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            LoadConfiguration();
            UpdateStatus();
        }
        private void BtnToggleApiKeyView_Click(object sender, RoutedEventArgs e)
        {
            _isApiKeyVisible = !_isApiKeyVisible;
            var btn = (System.Windows.Controls.Button)sender;
            if (_isApiKeyVisible)
            {
                txtApiKeyVisible.Text = txtApiKey.Password;
                txtApiKeyVisible.Visibility = Visibility.Visible;
                txtApiKey.Visibility = Visibility.Collapsed;
                btn.Opacity = 0.5;
            }
            else
            {
                txtApiKey.Password = txtApiKeyVisible.Text;
                txtApiKey.Visibility = Visibility.Visible;
                txtApiKeyVisible.Visibility = Visibility.Collapsed;
                btn.Opacity = 1.0;
            }
        }

        private void TxtAiInput_PreviewKeyDown(object sender, WpfInput.KeyEventArgs e)
        {
            if (e.Key == WpfInput.Key.Enter)
            {
                if (WpfInput.Keyboard.Modifiers.HasFlag(WpfInput.ModifierKeys.Shift))
                {
                    
                    return;
                }
                else
                {
                    
                    e.Handled = true;
                    BtnSendAiInput_Click(sender, e);
                }
            }
        }

        private async void BtnSendAiInput_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAiInput.Text)) return;

            string baseUrl = txtBaseUrl.Text.Trim();
            string apiKey = _isApiKeyVisible ? txtApiKeyVisible.Text : txtApiKey.Password;
            string model = txtAiModel.Text.Trim();

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
            {
                txtAiOutput.Text = "Base URL 和 API Key 不能为空。";
                return;
            }

            txtAiOutput.Text = "正在请求 AI...";
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                client.Timeout = TimeSpan.FromSeconds(60);

                string endpoint = baseUrl.TrimEnd('/');
                if (!endpoint.EndsWith("/chat/completions"))
                {
                    endpoint += "/chat/completions";
                }

                var reqBody = new { 
                    model = model, 
                    messages = new[] { new { role = "user", content = txtAiInput.Text } } 
                };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(reqBody), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var aiText = choices[0].GetProperty("message").GetProperty("content").GetString();
                        txtAiOutput.Text = aiText;
                    }
                    else
                    {
                        txtAiOutput.Text = "API 返回成功，但无内容。";
                    }
                }
                else
                {
                    txtAiOutput.Text = $"请求失败 ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                txtAiOutput.Text = $"发生异常: {ex.Message}";
            }
        }

        private void BtnClearAiInput_Click(object sender, RoutedEventArgs e)
        {
            txtAiInput.Text = "";
        }

        private void BtnCopyAiOutput_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtAiOutput.Text) && txtAiOutput.Text != "正在请求 AI...")
            {
                Clipboard.SetText(txtAiOutput.Text);
                MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
