using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using translation.Models;
using translation.Services;
using System.Diagnostics;
using System.Windows.Threading;
namespace translation.Controls
{
    public partial class NotebookWindow : Window
    {
        private NoteService _noteService;
        private AiService _aiService;
        private List<NoteItem> _allNotes;
        private bool _isEditing = false;
        private bool _isReloading = false;
        private Stopwatch _stopwatch;
        private DispatcherTimer _timer;
        public NotebookWindow()
        {
            InitializeComponent();
            _noteService = new NoteService();
            _aiService = new AiService();
            _allNotes = new List<NoteItem>();
            _stopwatch = new Stopwatch();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;
            this.Loaded += NotebookWindow_Loaded;
            this.Closed += NotebookWindow_Closed;
            NoteService.OnNoteChanged += NoteService_OnNoteChanged;
        }
        private void NotebookWindow_Closed(object sender, EventArgs e)
        {
            NoteService.OnNoteChanged -= NoteService_OnNoteChanged;
        }
        private void NoteService_OnNoteChanged()
        {
            Dispatcher.InvokeAsync(async () =>
            {
                await LoadNotes();
            });
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_stopwatch.IsRunning)
            {
                TxtAiResponse.Markdown = $"正在处理中... {_stopwatch.Elapsed.TotalSeconds:F2}s";
            }
        }
        private async void NotebookWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNotes();
        }
        private async Task LoadNotes()
        {
            _allNotes = await _noteService.GetAllNotesAsync() ?? new List<NoteItem>();
            _allNotes = _allNotes.OrderByDescending(n => n.CreatedAt).ToList();
            var allTags = _allNotes.SelectMany(n => n.Tags ?? new List<string>()).Distinct().ToList();
            CmbTags.Items.Clear();
            CmbTags.Items.Add("全部标签");
            foreach (var tag in allTags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    CmbTags.Items.Add(tag);
            }
            CmbTags.SelectedIndex = 0;
            FilterNotes();
        }
        private void FilterNotes()
        {
            if (_allNotes == null) return;
            _isReloading = true;
            var searchText = TxtSearch.Text == "搜索笔记..." ? "" : TxtSearch.Text.Trim();
            var tagText = CmbTags.Text == "全部标签" ? "" : CmbTags.Text.Trim();
            var filtered = _allNotes.AsEnumerable();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(n => (n.Content != null && n.Content.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                                             (n.Source != null && n.Source.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrEmpty(tagText) && tagText != "全部标签")
            {
                filtered = filtered.Where(n => n.Tags != null && n.Tags.Any(t => t.Contains(tagText, StringComparison.OrdinalIgnoreCase)));
            }
            var selectedIds = LstNotes.SelectedItems.Cast<NoteItem>().Select(n => n.Id).ToList();
            LstNotes.ItemsSource = filtered.ToList();
            if (selectedIds.Any())
            {
                var newSelected = filtered.Where(n => selectedIds.Contains(n.Id)).ToList();
                foreach (var item in newSelected)
                {
                    LstNotes.SelectedItems.Add(item);
                }
            }
            UpdateSelectedCount();
            _isReloading = false;
            LstNotes_SelectionChanged(null, null);
        }
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearch.Text == "搜索笔记...")
            {
                TxtSearch.Text = "";
                TxtSearch.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                TxtSearch.Text = "搜索笔记...";
                TxtSearch.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtSearch.Text != "搜索笔记...")
            {
                FilterNotes();
            }
        }
        private void CmbTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTags.SelectedItem != null)
            {
                FilterNotes();
            }
        }
        private void CmbTags_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterNotes();
        }
        private void LstNotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isReloading) return;
            UpdateSelectedCount();
            var firstSelected = LstNotes.SelectedItems.Cast<NoteItem>().FirstOrDefault();
            if (firstSelected != null)
            {
                MarkdownDetailContent.Markdown = firstSelected.Content;
                if (!_isEditing)
                {
                    TxtEditContent.Text = firstSelected.Content;
                    TxtEditSource.Text = firstSelected.Source;
                    TxtEditTags.Text = firstSelected.Tags != null ? string.Join(", ", firstSelected.Tags) : "";
                }
                TxtDetailSource.Text = firstSelected.Source;
                TxtDetailTags.Text = firstSelected.Tags != null ? string.Join(", ", firstSelected.Tags) : "";
            }
            else
            {
                MarkdownDetailContent.Markdown = "";
                if (!_isEditing)
                {
                    TxtEditContent.Text = "";
                    TxtEditSource.Text = "";
                    TxtEditTags.Text = "";
                }
                TxtDetailSource.Text = "";
                TxtDetailTags.Text = "";
            }
        }
        private void UpdateSelectedCount()
        {
            if (LstNotes != null && TxtSelectedCount != null)
            {
                TxtSelectedCount.Text = $"已选择: {LstNotes.SelectedItems.Count} 条笔记，共计 {_allNotes.Count} 条";
            }
        }
        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled) return;
            ScrollViewer scv = sender as ScrollViewer;
            if (scv == null && sender is DependencyObject depObj)
            {
                scv = FindVisualChild<ScrollViewer>(depObj);
            }
            if (scv != null)
            {
                var scrollStep = Math.Max(Math.Abs(e.Delta), 48);
                var offset = e.Delta < 0
                    ? scv.VerticalOffset + scrollStep
                    : scv.VerticalOffset - scrollStep;
                scv.ScrollToVerticalOffset(offset);
                e.Handled = true;
            }
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            var item = LstNotes.SelectedItem as NoteItem;
            if (item != null)
            {
                Clipboard.SetText(item.Content);
            }
        }
        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            if (LstNotes.SelectedItem != null && !_isEditing)
            {
                ToggleEditMode();
            }
        }
        private async void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedNotes = LstNotes.SelectedItems.Cast<NoteItem>().ToList();
            if (!selectedNotes.Any())
            {
                var selected = LstNotes.SelectedItem as NoteItem;
                if (selected != null)
                {
                    selectedNotes.Add(selected);
                }
            }
            if (!selectedNotes.Any()) return;
            var confirm = MessageBox.Show(
                $"确认删除选中的 {selectedNotes.Count} 条笔记吗？",
                "删除确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
            foreach (var note in selectedNotes)
            {
                await _noteService.DeleteNoteAsync(note.Id);
            }
            await LoadNotes();
            MarkdownDetailContent.Markdown = "";
            TxtEditContent.Text = "";
            TxtDetailSource.Text = "";
            TxtEditSource.Text = "";
            TxtDetailTags.Text = "";
            TxtEditTags.Text = "";
        }
        private async void BtnToggleEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                var item = LstNotes.SelectedItem as NoteItem;
                if (item != null)
                {
                    item.Content = TxtEditContent.Text;
                    item.Source = TxtEditSource.Text;
                    item.Tags = TxtEditTags.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(t => t.Trim()).ToList();
                    await _noteService.UpdateNoteAsync(item);
                }
            }
            ToggleEditMode();
            FilterNotes(); 
        }
        private void ToggleEditMode()
        {
            _isEditing = !_isEditing;
            if (_isEditing)
            {
                BtnToggleEdit.Content = "保存";
                MarkdownDetailContent.Visibility = Visibility.Collapsed;
                TxtEditContent.Visibility = Visibility.Visible;
                TxtDetailSource.Visibility = Visibility.Collapsed;
                TxtEditSource.Visibility = Visibility.Visible;
                TxtDetailTags.Visibility = Visibility.Collapsed;
                TxtEditTags.Visibility = Visibility.Visible;
            }
            else
            {
                BtnToggleEdit.Content = "编辑";
                MarkdownDetailContent.Visibility = Visibility.Visible;
                TxtEditContent.Visibility = Visibility.Collapsed;
                TxtDetailSource.Visibility = Visibility.Visible;
                TxtEditSource.Visibility = Visibility.Collapsed;
                TxtDetailTags.Visibility = Visibility.Visible;
                TxtEditTags.Visibility = Visibility.Collapsed;
                MarkdownDetailContent.Markdown = TxtEditContent.Text;
            }
        }
        private void TxtQuestion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    return;
                }
                else
                {
                    e.Handled = true;
                    BtnAskAi_Click(null, null);
                }
            }
        }
        private async void BtnAskAi_Click(object sender, RoutedEventArgs e)
        {
            var question = TxtQuestion.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;
            TxtQuestion.Text = string.Empty;
            _stopwatch.Restart();
            _timer.Start();
            try
            {
                var selectedNotes = LstNotes.SelectedItems.Cast<NoteItem>().ToList();
                string context = "";
                if (selectedNotes.Any())
                {
                    context = string.Join("\n\n", selectedNotes.Select(n => n.Content));
                }
                else
                {
                    context = string.Join("\n\n", _allNotes.Take(10).Select(n => n.Content));
                }
                var response = await _aiService.AskWithContextAsync(context, question);
                _stopwatch.Stop();
                _timer.Stop();
                TxtAiResponse.Markdown = response;
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                _timer.Stop();
                TxtAiResponse.Markdown = $"问答失败: {ex.Message}";
            }
        }
        private void BtnClearAi_Click(object sender, RoutedEventArgs e)
        {
            TxtAiResponse.Markdown = "";
            TxtQuestion.Text = string.Empty;
        }
        private async void BtnSaveToCurrent_Click(object sender, RoutedEventArgs e)
        {
            var item = LstNotes.SelectedItem as NoteItem;
            if (item != null && !string.IsNullOrWhiteSpace(TxtAiResponse.Markdown) && TxtAiResponse.Markdown != "")
            {
                int nextId = 1;
                while (item.Content != null && item.Content.Contains($"======条目{nextId}-AI补充======"))
                {
                    nextId++;
                }
                item.Content += $"\n\n======条目{nextId}-AI补充======\n" + TxtAiResponse.Markdown;
                await _noteService.UpdateNoteAsync(item);
                MarkdownDetailContent.Markdown = item.Content;
                TxtEditContent.Text = item.Content;
            }
        }
        private async void BtnSaveAsNew_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAiResponse.Markdown) || TxtAiResponse.Markdown == "") return;
            var newNote = new NoteItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = TxtAiResponse.Markdown,
                Source = "AI Assistant",
                CreatedAt = DateTime.Now,
                Tags = new List<string> { "AI" }
            };
            await _noteService.SaveNoteAsync(newNote);
            await LoadNotes();
            MessageBox.Show("已成功保存为新笔记！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
