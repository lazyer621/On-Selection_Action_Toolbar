using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq;
using translation.Services;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using WpfControls = System.Windows.Controls;
namespace translation.Controls
{
    public partial class FloatingMenu : WpfControls.UserControl
    {
        private string _selectedText;
        private string _sourceWindowTitle;
        private AiService _aiService;
        private NoteService _noteService;
        private string _resultText;
        private int _currentThemeIndex = 0;
        private Stopwatch _stopwatch;
        private DispatcherTimer _timer;
        private double _currentFontSize = 14;
        private List<BitmapFrame> _gifFrames;       
        private List<int> _gifDelays;       
        private int _gifCurrentFrame; 
        private DispatcherTimer _gifTimer;        
        public FloatingMenu()
        {
            InitializeComponent();
            _aiService = new AiService();
            _noteService = new NoteService();
            _stopwatch = new Stopwatch();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;
            Task.Run(() => LoadGifResource());
        }
        public void ReloadGif()
        {
            if (_gifTimer != null)
            {
                _gifTimer.Stop();
            }
            Task.Run(() => LoadGifResource());
        }
        private void LoadGifResource()
        {
            string gifPath = null;
            string customGifConfig = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customgif.cfg");
            if (System.IO.File.Exists(customGifConfig))
            {
                string selectedGifName = System.IO.File.ReadAllText(customGifConfig).Trim();
                if (!string.IsNullOrEmpty(selectedGifName))
                {
                    string customFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomGifs");
                    string potentialPath = System.IO.Path.Combine(customFolder, selectedGifName);
                    if (System.IO.File.Exists(potentialPath))
                    {
                        gifPath = potentialPath;
                    }
                }
            }
            if (gifPath == null)
            {
                try
                {
                    var asm = System.Reflection.Assembly.GetExecutingAssembly();
                    var names = asm.GetManifestResourceNames();
                    var res = names.FirstOrDefault(r => r.ToLower().Contains("pet.gif"));
                    if (res != null)
                    {
                        using var stream = asm.GetManifestResourceStream(res);
                        if (stream != null)
                        {
                            string tmp = System.IO.Path.Combine(
                                System.IO.Path.GetTempPath(), "xiaoliu_pet.gif");
                            using var fs = new System.IO.FileStream(
                                tmp, System.IO.FileMode.Create,
                                System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
                            stream.CopyTo(fs);
                            gifPath = tmp;
                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"[GIF embed] {ex.Message}"); }
                if (gifPath == null)
                {
                    string fs = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "imgs", "pet.gif");
                    if (System.IO.File.Exists(fs)) gifPath = fs;
                }
            }
            if (gifPath == null) return;
            string localPath = gifPath;
            Dispatcher.BeginInvoke(new Action(() => DecodeAndStartGif(localPath)),
                DispatcherPriority.Normal);
        }
        private void DecodeAndStartGif(string path)
        {
            try
            {
                var decoder = new GifBitmapDecoder(
                    new Uri(path, UriKind.Absolute),
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);   
                int frameCount = decoder.Frames.Count;
                if (frameCount == 0) return;
                _gifFrames = new List<BitmapFrame>(frameCount);
                _gifDelays = new List<int>(frameCount);
                for (int i = 0; i < frameCount; i++)
                {
                    var frame = decoder.Frames[i];
                    _gifFrames.Add(frame);
                    _gifDelays.Add(GetFrameDelayMs(frame));
                }
                _gifCurrentFrame = 0;
                if (GifElement != null)
                    GifElement.Source = _gifFrames[0];
                if (frameCount == 1) return;
                _gifTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(_gifDelays[0])
                };
                _gifTimer.Tick += GifTimer_Tick;
                _gifTimer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GIF decode] {ex.Message}");
            }
        }
        private void GifTimer_Tick(object sender, EventArgs e)
        {
            if (_gifFrames == null || _gifFrames.Count == 0) return;
            _gifCurrentFrame = (_gifCurrentFrame + 1) % _gifFrames.Count;
            _gifTimer.Interval = TimeSpan.FromMilliseconds(_gifDelays[_gifCurrentFrame]);
            if (GifElement != null)
                GifElement.Source = _gifFrames[_gifCurrentFrame];
        }
        private static int GetFrameDelayMs(BitmapFrame frame)
        {
            const int defaultDelay = 100;
            try
            {
                if (frame.Metadata is BitmapMetadata meta)
                {
                    const string delayPath = "/grctlext/Delay";
                    if (meta.ContainsQuery(delayPath))
                    {
                        var val = meta.GetQuery(delayPath);
                        if (val is ushort delay100)
                        {
                            int ms = (int)delay100 * 10;
                            return ms < 20 ? 100 : ms;
                        }
                    }
                }
            }
            catch {  }
            return defaultDelay;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (TimerDisplay != null && _stopwatch != null)
                    TimerDisplay.Text = $"⏱ {_stopwatch.Elapsed.TotalSeconds:F2}s";
            }
            catch { }
        }
        private string ExtractValidPath(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string path = input.Trim(' ', '\t', '"', '\'', '\r', '\n', '<', '>', '.');
            path = System.Text.RegularExpressions.Regex.Replace(path, @"\x1B\[[^a-zA-Z]*[a-zA-Z]", "");

          
            if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file:\\\\", StringComparison.OrdinalIgnoreCase))
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri uri) && uri.IsFile)
                    path = uri.LocalPath;
            }

            
            path = path.Replace('/', '\\');

            
            
            bool isNetworkOrLongPath = System.Text.RegularExpressions.Regex.IsMatch(path, @"^\\{2,}");

            
            path = System.Text.RegularExpressions.Regex.Replace(path, @"\\+", "\\");

            
            if (isNetworkOrLongPath)
            {
                path = "\\" + path;
            }

            string TryCheck(string p)
            {
                p = p.Trim();
                try {
                    if (System.IO.File.Exists(p) || System.IO.Directory.Exists(p)) return p;
                } catch { }
                int extIdx = p.Length;
                while ((extIdx = p.LastIndexOfAny(new[] { ':', '(', ' ' }, extIdx - 1)) > 2)
                {
                    string sub = p.Substring(0, extIdx).Trim();
                    try {
                        if (System.IO.File.Exists(sub) || System.IO.Directory.Exists(sub)) return sub;
                    } catch { }
                }
                return null;
            }

            string valid = TryCheck(path);
            if (valid != null) return valid;

            
            var match = System.Text.RegularExpressions.Regex.Match(path, @"([a-zA-Z]:\\[^*?\""<>|\r\n]+)|(\\\\[a-zA-Z0-9_.$~-]+\\[^*?\""<>|\r\n]+)");
            if (match.Success)
            {
                valid = TryCheck(match.Value);
                if (valid != null) return valid;
            }
            return path;
        }
        public void SetSelectedText(string text)
        {
            try
            {
                _selectedText = text;
                _sourceWindowTitle = NativeMethods.GetActiveWindowTitle();
                if (ResultText != null) ResultText.Text = string.Empty;
                ResultContainer.Visibility = Visibility.Collapsed;
                BtnCopyResult.Visibility = Visibility.Collapsed;
                if (AskAiPanel != null) AskAiPanel.Visibility = Visibility.Collapsed;
                if (BtnSaveAiResult != null) BtnSaveAiResult.Visibility = Visibility.Collapsed;
                if (TxtAskAiInput != null) TxtAskAiInput.Text = string.Empty;
                _resultText = string.Empty;
                if (TimerDisplay != null) TimerDisplay.Text = "⏱ 0.00s";
                if (BtnOpenPath != null)
                {
                    try
                    {
                        string validPath = ExtractValidPath(_selectedText);
                        bool isPath = !string.IsNullOrWhiteSpace(validPath) &&
                                      (System.IO.File.Exists(validPath) ||
                                       System.IO.Directory.Exists(validPath));
                        BtnOpenPath.Visibility = isPath ? Visibility.Visible : Visibility.Collapsed;
                    }
                    catch { BtnOpenPath.Visibility = Visibility.Collapsed; }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"SetSelectedText error: {ex.Message}"); }
        }
        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null && e.LeftButton == MouseButtonState.Pressed)
                parentWindow.DragMove();
        }

        public bool IsPinned { get; private set; } = false;

        private void BtnPin_Click(object sender, RoutedEventArgs e)
        {
            IsPinned = !IsPinned;
            if (TxtPinIcon != null) TxtPinIcon.Text = IsPinned ? "📍" : "📌";
            if (TxtPinText != null) TxtPinText.Text = IsPinned ? "取消" : "置顶";
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
                parentWindow.Topmost = true; // explicitly ensure it stays topmost if that's desired, or just use logic in Deactivated
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e) => PerformSystemCopy();
        private void BtnCut_Click(object sender, RoutedEventArgs e) => PerformSystemCut();
        public async void PerformSystemCut()
        {
            if (ResultText != null && ResultText.SelectionLength > 0 && !ResultText.IsReadOnly)
            { try { ResultText.Cut(); } catch { } return; }
            CloseMenu();
            await Task.Delay(100);
            Services.NativeMethods.keybd_event(Services.NativeMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);
            Services.NativeMethods.keybd_event(Services.NativeMethods.VK_X, 0, 0, UIntPtr.Zero);
            Services.NativeMethods.keybd_event(Services.NativeMethods.VK_X, 0, Services.NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            Services.NativeMethods.keybd_event(Services.NativeMethods.VK_CONTROL, 0, Services.NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
        public void PerformSystemCopy()
        {
            if (ResultText != null && ResultText.SelectionLength > 0)
            { try { ResultText.Copy(); } catch { } return; }
            if (!string.IsNullOrEmpty(_selectedText))
            { try { Clipboard.SetText(_selectedText); } catch { } CloseMenu(); }
        }
        private async void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {
            BtnTranslate.IsEnabled = false;
            ResultContainer.Visibility = Visibility.Visible;
            BtnCopyResult.Visibility = Visibility.Collapsed;
            if (AskAiPanel != null) AskAiPanel.Visibility = Visibility.Collapsed;
            if (BtnSaveAiResult != null) BtnSaveAiResult.Visibility = Visibility.Collapsed;
            if (TxtAskAiInput != null) TxtAskAiInput.Text = string.Empty;
            StartTimer();
            try
            {
                var result = await _aiService.TranslateAsync(_selectedText);
                StopTimer(); ShowResult(result);
            }
            catch (Exception ex) { StopTimer(); ShowResult($"翻译失败: {ex.Message}"); }
            finally { BtnTranslate.IsEnabled = true; }
        }
        private async void ExplainMenuItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (ResultContainer == null) return;
            if (sender is ComboBoxItem selectedItem && selectedItem.Tag is string tag)
            {
                CmbExplainMenu.IsDropDownOpen = false;
                e.Handled = true;
                ResultContainer.Visibility = Visibility.Visible;
                BtnCopyResult.Visibility = Visibility.Collapsed;
                if (AskAiPanel != null) AskAiPanel.Visibility = Visibility.Collapsed;
                if (BtnSaveAiResult != null) BtnSaveAiResult.Visibility = Visibility.Collapsed;
                if (TxtAskAiInput != null) TxtAskAiInput.Text = string.Empty;
                StartTimer();
                try
                {
                    string result = tag switch
                    {
                        "Explain" => await _aiService.ExplainAsync(_selectedText),
                        "Lookup" => await _aiService.LookupWordAsync(_selectedText),
                        _ => null
                    };
                    if (result == null) return;
                    if (string.IsNullOrEmpty(result))
                        result = "返回了空结果，请检查 API 配置或网络连接。";
                    StopTimer(); ShowResult(result);
                }
                catch (Exception ex) { StopTimer(); ShowResult($"发生错误: {ex.Message}"); }
                finally { CmbExplainMenu.SelectedIndex = 0; }
            }
        }
        private void StartTimer()
        {
            _stopwatch.Restart(); _timer.Start();
            TimerDisplay.Text = "⏱ 0.00s";
        }
        private void StopTimer()
        {
            _stopwatch.Stop(); _timer.Stop();
            TimerDisplay.Text = $"⏱ {_stopwatch.Elapsed.TotalSeconds:F2}s";
        }
        private void ShowResult(string text)
        {
            _resultText = text;
            ResultText.Text = text;
            ResultContainer.Visibility = Visibility.Visible;
            BtnCopyResult.Visibility = Visibility.Visible;
            if (BtnSaveAiResult != null) BtnSaveAiResult.Visibility = Visibility.Visible;
        }
        private async void BtnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedText)) return;
            try
            {
                var popup = new Window
                {
                    Title = "保存笔记 - 添加标签",
                    Width = 320, Height = 140,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.ToolWindow, Topmost = true,
                    ResizeMode = ResizeMode.NoResize
                };
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    popup.Owner = parentWindow;
                    popup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                var grid = new WpfControls.Grid { Margin = new Thickness(15) };
                grid.RowDefinitions.Add(new WpfControls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new WpfControls.RowDefinition { Height = GridLength.Auto });
                var inputTb = new WpfControls.TextBox { VerticalAlignment = VerticalAlignment.Center, FontSize = 13, Padding = new Thickness(5) };
                WpfControls.Grid.SetRow(inputTb, 0);
                grid.Children.Add(inputTb);
                var hint = new WpfControls.TextBlock
                {
                    Text = "自定义标签(逗号分隔)，留空则无标签", Foreground = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(7, 0, 0, 0), IsHitTestVisible = false
                };
                WpfControls.Grid.SetRow(hint, 0);
                inputTb.TextChanged += (s, ev) => hint.Visibility = string.IsNullOrEmpty(inputTb.Text) ? Visibility.Visible : Visibility.Collapsed;
                grid.Children.Add(hint);
                var sp = new WpfControls.StackPanel { Orientation = WpfControls.Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
                WpfControls.Grid.SetRow(sp, 1);
                var btnAdd = new WpfControls.Button { Content = "添加", Width = 70, Height = 26, Margin = new Thickness(0, 0, 10, 0) };
                var btnCancel = new WpfControls.Button { Content = "取消", Width = 70, Height = 26 };
                bool isOk = false;
                btnAdd.Click += (s, ev) => { isOk = true; popup.Close(); };
                btnCancel.Click += (s, ev) => { isOk = false; popup.Close(); };
                sp.Children.Add(btnAdd);
                sp.Children.Add(btnCancel);
                grid.Children.Add(sp);
                popup.Content = grid;
                popup.ShowDialog();
                if (!isOk) return;
                var tagList = inputTb.Text.Split(new[] { ',', '，', ' ', '、' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(t => t.Trim()).ToList();
                string safelyFormattedContent = _selectedText;
                if (_selectedText.Contains('\n') || _selectedText.Contains('<') || _selectedText.Contains('{') || _selectedText.Contains('='))
                {
                    safelyFormattedContent = $"```text\n{_selectedText}\n```";
                }
                var note = new Models.NoteItem
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = safelyFormattedContent,
                    Source = _sourceWindowTitle,
                    CreatedAt = DateTime.Now,
                    Tags = tagList
                };
                await _noteService.SaveNoteAsync(note);
                ShowResult("✅ 已保存笔记");
                ResultContainer.Visibility = Visibility.Visible;
                AskAiPanel.Visibility = Visibility.Collapsed;
                BtnSaveAiResult.Visibility = Visibility.Collapsed;
                BtnCopyResult.Visibility = Visibility.Collapsed;
                await Task.Delay(1500);
                CloseMenu();
            }
            catch (Exception ex)
            {
                ShowResult($"保存失败: {ex.Message}");
            }
        }
        private void BtnAskAiInline_Click(object sender, RoutedEventArgs e)
        {
            ResultContainer.Visibility = Visibility.Visible;
            AskAiPanel.Visibility = Visibility.Visible;
            BtnSaveAiResult.Visibility = Visibility.Visible;
            BtnCopyResult.Visibility = Visibility.Collapsed;
            ResultText.Text = "输入问题...";
            TxtAskAiInput.Focus();
        }
        private async void BtnSubmitAskAi_Click(object sender, RoutedEventArgs e)
        {
            await SubmitAskAi();
        }
        private async void TxtAskAiInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (BtnSubmitAskAi.IsEnabled)
                {
                    await SubmitAskAi();
                }
            }
        }
        private async Task SubmitAskAi()
        {
            var question = TxtAskAiInput.Text?.Trim();
            if (string.IsNullOrEmpty(question)) return;
            BtnSubmitAskAi.IsEnabled = false;
            StartTimer();
            try
            {
                var result = await _aiService.AskQuestionAsync(_selectedText, question);
                StopTimer();
                ShowResult(result);
            }
            catch (Exception ex)
            {
                StopTimer();
                ShowResult($"问答失败: {ex.Message}");
            }
            finally
            {
                BtnSubmitAskAi.IsEnabled = true;
            }
        }
        private async void BtnSaveAiResult_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_resultText) || string.IsNullOrWhiteSpace(_selectedText)) return;
            try
            {
                var popup = new Window
                {
                    Title = "保存笔记 - 添加标签",
                    Width = 320, Height = 140,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.ToolWindow, Topmost = true,
                    ResizeMode = ResizeMode.NoResize
                };
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    popup.Owner = parentWindow;
                    popup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                var grid = new WpfControls.Grid { Margin = new Thickness(15) };
                grid.RowDefinitions.Add(new WpfControls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new WpfControls.RowDefinition { Height = GridLength.Auto });
                var inputTb = new WpfControls.TextBox { VerticalAlignment = VerticalAlignment.Center, FontSize = 13, Padding = new Thickness(5) };
                WpfControls.Grid.SetRow(inputTb, 0);
                grid.Children.Add(inputTb);
                var hint = new WpfControls.TextBlock
                {
                    Text = "自定义标签(逗号分隔)，留空则无标签", Foreground = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(7, 0, 0, 0), IsHitTestVisible = false
                };
                WpfControls.Grid.SetRow(hint, 0);
                inputTb.TextChanged += (s, ev) => hint.Visibility = string.IsNullOrEmpty(inputTb.Text) ? Visibility.Visible : Visibility.Collapsed;
                grid.Children.Add(hint);
                var sp = new WpfControls.StackPanel { Orientation = WpfControls.Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
                WpfControls.Grid.SetRow(sp, 1);
                var btnAdd = new WpfControls.Button { Content = "添加", Width = 70, Height = 26, Margin = new Thickness(0, 0, 10, 0) };
                var btnCancel = new WpfControls.Button { Content = "取消", Width = 70, Height = 26 };
                bool isOk = false;
                btnAdd.Click += (s, ev) => { isOk = true; popup.Close(); };
                btnCancel.Click += (s, ev) => { isOk = false; popup.Close(); };
                sp.Children.Add(btnAdd);
                sp.Children.Add(btnCancel);
                grid.Children.Add(sp);
                popup.Content = grid;
                popup.ShowDialog();
                if (!isOk) return;
                var tagList = inputTb.Text.Split(new[] { ',', '，', ' ', '、' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(t => t.Trim()).ToList();

                string safelyFormattedContent = _selectedText;
                if (_selectedText.Contains('\n') || _selectedText.Contains('<') || _selectedText.Contains('{') || _selectedText.Contains('='))
                {
                    safelyFormattedContent = $"\n```text\n{_selectedText}\n```\n";
                }
                var note = new Models.NoteItem
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = $"**【来源：{_sourceWindowTitle}】**\n{safelyFormattedContent}",
                    Source = _sourceWindowTitle + " (AI Q&A)",
                    CreatedAt = DateTime.Now,
                    Tags = new List<string> { "AI问答" }
                };
                foreach (var tag in tagList)
                {
                    if (!note.Tags.Contains(tag)) note.Tags.Add(tag);
                }
                string question = TxtAskAiInput.Text?.Trim();
                string qAndA = string.IsNullOrWhiteSpace(question) 
                    ? _resultText 
                    : $"**Q:** {question}\n\n**A:** {_resultText}";
                note.Content += $"\n\n======AI补充======\n{qAndA}";
                await _noteService.SaveNoteAsync(note);
                ShowResult("✅ AI结果已成功保存至笔记");
                AskAiPanel.Visibility = Visibility.Collapsed;
                BtnSaveAiResult.Visibility = Visibility.Collapsed;
                BtnCopyResult.Visibility = Visibility.Collapsed;
                await Task.Delay(1500);
                CloseMenu();
            }
            catch (Exception ex)
            {
                ShowResult($"保存失败: {ex.Message}");
            }
        }
        private void BtnOpenUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedText)) return;
                string url = _selectedText.Trim();
                var match = System.Text.RegularExpressions.Regex.Match(
                    url, @"([a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(/\S*)?)");
                if (match.Success) url = match.Value;
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    url = "https://" + url;
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); CloseMenu(); }
                else
                    MessageBox.Show("所选文本不包含有效的网址。", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            { MessageBox.Show($"无法打开网址：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void BtnOpenPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedText)) return;
                string path = ExtractValidPath(_selectedText);
                if (System.IO.File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{path}\"",
                        UseShellExecute = true
                    });
                    CloseMenu();
                }
                else if (System.IO.Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true
                    });
                    CloseMenu();
                }
            }
            catch (Exception ex)
            { MessageBox.Show($"无法打开路径：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedText)) return;
            string engine = "Baidu";
            if (CmbSearchEngine.SelectedItem is ComboBoxItem si && si.Tag is string t) engine = t;
            string enc = Uri.EscapeDataString(_selectedText);
            string url = engine switch
            {
                "Google" => $"https://www.google.com/search?q={enc}",
                "Bing" => $"https://www.bing.com/search?q={enc}",
                "Metaso" => $"https://metaso.cn/?q={enc}",
                _ => $"https://www.baidu.com/s?wd={enc}"
            };
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); CloseMenu(); }
            catch (Exception ex)
            { MessageBox.Show($"无法打开搜索：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void BtnCopyResult_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_resultText)) { try { Clipboard.SetText(_resultText); } catch { } CloseMenu(); }
        }
        private void BtnIncreaseFont_Click(object sender, RoutedEventArgs e)
        { if (_currentFontSize < 32) { _currentFontSize += 2; ResultText.FontSize = _currentFontSize; } }
        private void BtnDecreaseFont_Click(object sender, RoutedEventArgs e)
        { if (_currentFontSize > 8) { _currentFontSize -= 2; ResultText.FontSize = _currentFontSize; } }
        private void CmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ResultText == null || CmbFontFamily == null) return;
                if (CmbFontFamily.SelectedItem is ComboBoxItem si && si.Tag is string fontName)
                    ResultText.FontFamily = new FontFamily(fontName);
            }
            catch (Exception ex) { Debug.WriteLine($"CmbFontFamily error: {ex.Message}"); }
        }
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPopup != null)
            {
                SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
            }
        }
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_themes?.Count > 0)
                {
                    _currentThemeIndex = (_currentThemeIndex + 1) % _themes.Count;
                    ApplyTheme(_themes[_currentThemeIndex]);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"BtnTheme_Click error: {ex.Message}"); }
        }
        private void ApplyTheme(AppTheme theme)
        {
            try
            {
                var res = this.Resources;
                Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
                res["AccentBrush"] = new SolidColorBrush(C(theme.AccentColor));
                res["ForegroundBrush"] = new SolidColorBrush(C(theme.ForegroundColor));
                res["SubForeground"] = new SolidColorBrush(C(theme.SubForeground));
                res["ButtonBgBrush"] = new SolidColorBrush(C(theme.ButtonBg));
                res["ButtonBorderBrush"] = new SolidColorBrush(C(theme.ButtonBorder));
                res["ButtonFgBrush"] = new SolidColorBrush(C(theme.ButtonFg));
                res["ResultBgBrush"] = new SolidColorBrush(C(theme.ResultBg));
                res["ComboBoxBgBrush"] = new SolidColorBrush(C(theme.ComboBoxBg));
                res["ComboBoxHover"] = new SolidColorBrush(C(theme.ComboBoxHover));
                res["ComboBoxSelected"] = new SolidColorBrush(C(theme.ComboBoxSelected));
                res["SeparatorBrush"] = new SolidColorBrush(C(theme.SeparatorColor));
                res["PrimaryBtnFg"] = new SolidColorBrush(C(theme.PrimaryBtnFg));
                var modGrad = new LinearGradientBrush
                { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
                modGrad.GradientStops.Add(new GradientStop(C(theme.BgGradientStart), 0));
                modGrad.GradientStops.Add(new GradientStop(C(theme.BgGradientEnd), 1));
                res["ModernGradient"] = modGrad;
                var primGrad = new LinearGradientBrush
                { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
                primGrad.GradientStops.Add(new GradientStop(C(theme.PrimaryBtnStart), 0));
                primGrad.GradientStops.Add(new GradientStop(C(theme.PrimaryBtnEnd), 1));
                res["PrimaryGradient"] = primGrad;
                Color accent = C(theme.AccentColor);
                bool isLight = theme.Name is "晨雾白" or "暖沙漠" or "玫瑰石英";
                if (MainBorder?.Effect is System.Windows.Media.Effects.DropShadowEffect sh)
                {
                    MainBorder.BorderBrush = new SolidColorBrush(accent);
                    sh.Color = isLight ? accent : Colors.Black;
                    sh.Opacity = isLight ? 0.15 : 0.5;
                }
                if (ResultShadow != null) ResultShadow.Color = accent;
            }
            catch (Exception ex) { Debug.WriteLine($"ApplyTheme failed: {ex.Message}"); }
        }
        public event EventHandler RequestClose;
        private void CloseMenu() => RequestClose?.Invoke(this, EventArgs.Empty);
        public double GetMinimumRequiredHeight() => 52;
        public void UpdateMaxHeight(double maxScreenHeight) { }
        private readonly List<AppTheme> _themes = new List<AppTheme>
        {
            new AppTheme { Name="默认",     BgGradientStart="#46A3FF", BgGradientEnd="#46A3FF", ResultBg="#84C1FF",  AccentColor="#667eea", ForegroundColor="#e0e0e0", SubForeground="#8a8a9a", ButtonBg="#C4E1FF",  ButtonBorder="#C4E1FF",  ButtonFg="#333333", PrimaryBtnStart="#667eea", PrimaryBtnEnd="#764ba2", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#C4E1FF",  ComboBoxHover="#C4E1FF",  ComboBoxSelected="#667eea", SeparatorColor="#C4E1FF"   },
            new AppTheme { Name="晨雾白",   BgGradientStart="#f5f6fa", BgGradientEnd="#ffffff",  ResultBg="#eef0fb",  AccentColor="#5c6bc0", ForegroundColor="#3c4255", SubForeground="#7a7f9a", ButtonBg="#ffffff",   ButtonBorder="#c5cae9",  ButtonFg="#3c4255", PrimaryBtnStart="#5c6bc0", PrimaryBtnEnd="#7986cb", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#ffffff",   ComboBoxHover="#e8eaf6",  ComboBoxSelected="#5c6bc0", SeparatorColor="#c5cae9"   },
            new AppTheme { Name="赛博霓虹", BgGradientStart="#f0fffe", BgGradientEnd="#f5f0ff",  ResultBg="#edfffe",  AccentColor="#00b8b2", ForegroundColor="#0a3d3c", SubForeground="#5a9a98", ButtonBg="#f5f0ff",   ButtonBorder="#d0f5f4",  ButtonFg="#0a3d3c", PrimaryBtnStart="#00b8b2", PrimaryBtnEnd="#8b00cc", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#f5f0ff",   ComboBoxHover="#e0d5f5",  ComboBoxSelected="#00b8b2", SeparatorColor="#d0f5f4"   },
            new AppTheme { Name="深空极夜", BgGradientStart="#f2f3fb", BgGradientEnd="#eef0ff",  ResultBg="#eef0ff",  AccentColor="#5566dd", ForegroundColor="#2a2f6e", SubForeground="#7075aa", ButtonBg="#f2f3fb",   ButtonBorder="#d0d4f5",  ButtonFg="#2a2f6e", PrimaryBtnStart="#5566dd", PrimaryBtnEnd="#7a44c0", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#f2f3fb",   ComboBoxHover="#e2e5f8",  ComboBoxSelected="#5566dd", SeparatorColor="#d0d4f5"   },
            new AppTheme { Name="暖沙漠",   BgGradientStart="#fdf6ec", BgGradientEnd="#fff8f0",  ResultBg="#fef2e4",  AccentColor="#e07b2a", ForegroundColor="#5c3a00", SubForeground="#a07040", ButtonBg="#fff8f0",   ButtonBorder="#f0c98a",  ButtonFg="#5c3a00", PrimaryBtnStart="#e07b2a", PrimaryBtnEnd="#f0a855", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#fff8f0",   ComboBoxHover="#fce8c8",  ComboBoxSelected="#e07b2a", SeparatorColor="#f0c98a"   },
            new AppTheme { Name="墨绿沉静", BgGradientStart="#f0faf6", BgGradientEnd="#e8f7f1",  ResultBg="#e8f7f1",  AccentColor="#1d9460", ForegroundColor="#0d3324", SubForeground="#4a8a6c", ButtonBg="#f0faf6",   ButtonBorder="#b8e4d0",  ButtonFg="#0d3324", PrimaryBtnStart="#1d9460", PrimaryBtnEnd="#157a4e", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#f0faf6",   ComboBoxHover="#d8f0e6",  ComboBoxSelected="#1d9460", SeparatorColor="#b8e4d0"   },
            new AppTheme { Name="hack终端", BgGradientStart="#0a0f0a", BgGradientEnd="#050805",  ResultBg="#050805",  AccentColor="#00ff41", ForegroundColor="#00cc33", SubForeground="#007a1f", ButtonBg="#0a0f0a",   ButtonBorder="#00ff4133", ButtonFg="#00cc33", PrimaryBtnStart="#00ff41", PrimaryBtnEnd="#00cc33", PrimaryBtnFg="#050805", ComboBoxBg="#0a0f0a",   ComboBoxHover="#93FF93",  ComboBoxSelected="#00ff41", SeparatorColor="#00ff4122" },
            new AppTheme { Name="code入侵", BgGradientStart="#0c0c14", BgGradientEnd="#080810",  ResultBg="#080810",  AccentColor="#ff6600", ForegroundColor="#ff9944", SubForeground="#994422", ButtonBg="#0c0c14",   ButtonBorder="#ff660033", ButtonFg="#ff9944", PrimaryBtnStart="#ff6600", PrimaryBtnEnd="#cc4400", PrimaryBtnFg="#ffffff", ComboBoxBg="#0c0c14",   ComboBoxHover="#FF5151",  ComboBoxSelected="#ff6600", SeparatorColor="#ff660022" },
            new AppTheme { Name="玫瑰石英", BgGradientStart="#fdf0f3", BgGradientEnd="#fff5f7",  ResultBg="#fde8ee",  AccentColor="#e0507a", ForegroundColor="#5c2035", SubForeground="#a06070", ButtonBg="#fff5f7",   ButtonBorder="#f5c0ce",  ButtonFg="#5c2035", PrimaryBtnStart="#e0507a", PrimaryBtnEnd="#c0306a", PrimaryBtnFg="#FFFFFF", ComboBoxBg="#fff5f7",   ComboBoxHover="#f9d6e0",  ComboBoxSelected="#e0507a", SeparatorColor="#f5c0ce"   },
        };
    }
    public class AppTheme
    {
        public string Name { get; set; }
        public string BgGradientStart { get; set; }
        public string BgGradientEnd { get; set; }
        public string ResultBg { get; set; }
        public string AccentColor { get; set; }
        public string ForegroundColor { get; set; }
        public string SubForeground { get; set; }
        public string ButtonBg { get; set; }
        public string ButtonBorder { get; set; }
        public string ButtonFg { get; set; }
        public string PrimaryBtnStart { get; set; }
        public string PrimaryBtnEnd { get; set; }
        public string PrimaryBtnFg { get; set; }
        public string ComboBoxBg { get; set; }
        public string ComboBoxHover { get; set; }
        public string ComboBoxSelected { get; set; }
        public string SeparatorColor { get; set; }
    }
}
