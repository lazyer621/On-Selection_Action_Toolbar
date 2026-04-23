using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using translation.Controls;
using translation.Services;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Cursors = System.Windows.Input.Cursors;
using FontFamily = System.Windows.Media.FontFamily;
using Forms = System.Windows.Forms;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MenuItem = System.Windows.Controls.MenuItem;
using Orientation = System.Windows.Controls.Orientation;
using WpfApplication = System.Windows.Application;
using WpfControls = System.Windows.Controls;
using WpfMedia = System.Windows.Media;
using WpfPoint = System.Windows.Point;
namespace translation
{
    public class StyledDialogWindow : Window
    {
        protected static readonly WpfMedia.FontFamily FontChinese = new WpfMedia.FontFamily("宋体");
        protected static readonly WpfMedia.FontFamily FontEnglish = new WpfMedia.FontFamily("Times New Roman");
        protected static readonly WpfMedia.FontFamily FontTitle = new WpfMedia.FontFamily("黑体");
        protected static readonly FontFamily FontMixed =
            new FontFamily("Times New Roman, 宋体");
        protected static readonly FontFamily FontTitleMixed =
            new FontFamily("Times New Roman, 黑体");
        protected StyledDialogWindow()
        {
            FontFamily = FontMixed;
            FontSize = 14;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            this.Loaded += (s, e) => this.SizeToContent = SizeToContent.Manual;
        }
        protected static TextBlock MakeTitleBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = FontTitleMixed,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap
            };
        }
        protected static TextBlock MakeBodyBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = FontMixed,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 24
            };
        }
        protected static WpfControls.Button MakeButton(string text, bool isPrimary = false)
        {
            var btn = new Button
            {
                Content = text,
                FontFamily = FontMixed,
                FontSize = 13,
                Width = 88,
                Height = 32,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1),
            };
            if (isPrimary)
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                btn.Foreground = Brushes.White;
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 100, 180));
            }
            else
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                btn.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            }
            return btn;
        }
    }
    public class AuthorInfoDialog : StyledDialogWindow
    {
        public AuthorInfoDialog()
        {
            Title = "关于";
            Width = 340;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Content = BuildLayout();
        }

        private UIElement BuildLayout()
        {
            var root = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(228, 228, 228)),
                BorderThickness = new Thickness(1),
                ClipToBounds = true
            };

            var panel = new StackPanel();

            panel.Children.Add(BuildHeader());

            panel.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
            });

            panel.Children.Add(BuildContactSection());

            panel.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
            });

            panel.Children.Add(BuildFooter());

            root.Child = panel;
            return root;
        }

        private UIElement BuildHeader()
        {
            var header = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 28, 0, 22)
            };

            var avatarBorder = new Border
            {
                Width = 52,
                Height = 52,
                CornerRadius = new CornerRadius(26),
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(228, 228, 228)),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            };
            avatarBorder.Child = new TextBlock
            {
                Text = "621",
                FontSize = 15,
                FontWeight = FontWeights.Medium,
                FontFamily = FontEnglish,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleBlock = new TextBlock
            {
                Text = "Lazybones",
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                FontFamily = FontEnglish,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var subtitleBlock = new TextBlock
            {
                Text = "划词小工具",
                FontSize = 12,
                FontFamily = FontEnglish,
                Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            header.Children.Add(avatarBorder);
            header.Children.Add(titleBlock);
            header.Children.Add(subtitleBlock);
            return header;
        }

        private UIElement BuildContactSection()
        {
            var section = new StackPanel
            {
                Margin = new Thickness(20, 16, 20, 16)
            };

            section.Children.Add(new TextBlock
            {
                Text = "联系方式",
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                Margin = new Thickness(6, 0, 0, 8)
            });

            section.Children.Add(MakeCopyRow("制作人员", "lazyer-@621"));
            section.Children.Add(MakeCopyRow("联系邮箱", "xiaoliuzi216@gmail.com"));
            section.Children.Add(MakeCopyRow("个人Q Q", "2415732349"));
            section.Children.Add(MakeCopyRow("个人微信", "Civil-IT_a621"));

            return section;
        }

        private UIElement BuildFooter()
        {
            var closeBtn = new Button
            {
                Content = "关闭",
                Height = 36,
                FontSize = 13,
                Margin = new Thickness(20, 12, 20, 16),
                Cursor = Cursors.Hand
            };
            ApplyOutlineButtonStyle(closeBtn);
            closeBtn.Click += (_, _) => Close();
            return closeBtn;
        }

        private UIElement MakeCopyRow(string label, string value)
        {
            var hoverBg = new SolidColorBrush(Color.FromRgb(247, 247, 247));
            var normalBg = Brushes.Transparent;

            var row = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = normalBg,
                Padding = new Thickness(10, 8, 10, 8),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 1, 0, 1)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(labelBlock, 0);

            var valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 13,
                FontFamily = FontEnglish,
                Foreground = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(valueBlock, 1);

            var hintBlock = new TextBlock
            {
                Text = "复制",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            Grid.SetColumn(hintBlock, 2);

            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
            grid.Children.Add(hintBlock);
            row.Child = grid;

            row.MouseEnter += (_, _) => row.Background = hoverBg;
            row.MouseLeave += (_, _) => row.Background = normalBg;

            row.MouseLeftButtonUp += async (_, _) =>
            {
                try { Clipboard.SetText(value); } catch { return; }

                var successColor = new SolidColorBrush(Color.FromRgb(52, 168, 83));
                hintBlock.Text = "已复制";
                hintBlock.Foreground = successColor;

                await Task.Delay(1500);

                hintBlock.Text = "复制";
                hintBlock.Foreground = new SolidColorBrush(Color.FromRgb(190, 190, 190));
            };

            return row;
        }

        private static void ApplyOutlineButtonStyle(Button btn)
        {
            btn.Background = Brushes.White;
            btn.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210));
            btn.BorderThickness = new Thickness(1);
            btn.Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            btn.Template = CreateOutlineButtonTemplate();
        }

        private static ControlTemplate CreateOutlineButtonTemplate()
        {
            var tpl = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetBinding(Border.BackgroundProperty,
                new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty,
                new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty,
                new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);

            tpl.VisualTree = borderFactory;

            var trigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Border.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(247, 247, 247)),
                "bd"));
            borderFactory.Name = "bd";
            tpl.Triggers.Add(trigger);

            return tpl;
        }
    }
    public class AlreadyRunningDialog : StyledDialogWindow
    {
        public AlreadyRunningDialog()
        {
            Title = "提示";
            Width = 360;
            SizeToContent = SizeToContent.Height;
            var root = new Border
            {
                Background = Brushes.White,
                Padding = new Thickness(28, 24, 28, 20),
                CornerRadius = new CornerRadius(6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                BorderThickness = new Thickness(1)
            };
            var panel = new StackPanel();
            panel.Children.Add(MakeBodyBlock("程序已在运行，请在右下角系统托盘中查看。"));
            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            var okBtn = MakeButton("确定", isPrimary: true);
            okBtn.Click += (_, _) => Close();
            btnRow.Children.Add(okBtn);
            panel.Children.Add(btnRow);
            root.Child = panel;
            Content = root;
        }
    }
    public partial class App : WpfApplication
    {
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(
            IntPtr hMonitor, int dpiType, out uint dpiX, out uint dpiY);
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(
            POINT pt, uint dwFlags);
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        private const uint GW_OWNER = 4;
        private static void GetDpiScaleForPoint(int x, int y, out double dpiX, out double dpiY)
        {
            dpiX = dpiY = 1.0;
            try
            {
                const uint MONITOR_DEFAULTTONEAREST = 2;
                const int MDT_EFFECTIVE_DPI = 0;
                var pt = new POINT { X = x, Y = y };
                var hMon = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
                if (hMon == IntPtr.Zero) return;
                if (GetDpiForMonitor(hMon, MDT_EFFECTIVE_DPI,
                    out uint dx, out uint dy) == 0) 
                {
                    dpiX = dx / 96.0;
                    dpiY = dy / 96.0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetDpiScale] {ex.Message}");
            }
        }
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        const int WM_HOTKEY = 0x0312;
        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int MOD_NOREPEAT = 0x4000;
        private IntPtr _msgHwnd = IntPtr.Zero;
        private static readonly string ConfigPath =
            Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory,
                         "hotkey.cfg");
        private int _customHotkeyMod = MOD_CONTROL | MOD_NOREPEAT;
        private int _customHotkeyVk = 0x51; 
        private const int CUSTOM_HOTKEY_ID = 9001;
        private bool _isAutoPopupEnabled = true; 
        private IntPtr MsgWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                if (msg == WM_HOTKEY)
                {
                    int id = wParam.ToInt32();
                    if (id == CUSTOM_HOTKEY_ID)
                    {
                        Dispatcher.BeginInvoke(new Action(OnHotkeyTriggered), DispatcherPriority.Normal);
                        handled = true;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[MsgWndProc] {ex}"); }
            return IntPtr.Zero;
        }
        private static Mutex _singleInstanceMutex;
        private MouseHookService _mouseHook;
        private FloatingWindow _floatingWindow;
        private StateIconWindow _stateIconWindow;
        private TaskbarIcon _notifyIcon;
        private System.Drawing.Icon _trayIcon;
        private bool _isToolEnabled = true;
        private void LoadHotkeyConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return;
                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    if (parts[0].Trim() == "mod" && int.TryParse(parts[1].Trim(), out int mod))
                        _customHotkeyMod = mod | MOD_NOREPEAT;
                    if (parts[0].Trim() == "vk" && int.TryParse(parts[1].Trim(), out int vk))
                        _customHotkeyVk = vk;
                    if (parts[0].Trim() == "autopopup" && int.TryParse(parts[1].Trim(), out int autoPopup))
                        _isAutoPopupEnabled = autoPopup == 1;
                    if (parts[0].Trim() == "toolenabled" && int.TryParse(parts[1].Trim(), out int toolEnabled))
                        _isToolEnabled = toolEnabled == 1;
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[LoadHotkey] {ex.Message}"); }
        }
        private void SaveHotkeyConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                File.WriteAllLines(ConfigPath, new[]
                {
                    $"mod={(int)(_customHotkeyMod & ~MOD_NOREPEAT)}",
                    $"vk={_customHotkeyVk}",
                    $"autopopup={(_isAutoPopupEnabled ? 1 : 0)}",
                    $"toolenabled={(_isToolEnabled ? 1 : 0)}"
                });
            }
            catch (Exception ex) { Debug.WriteLine($"[SaveHotkey] {ex.Message}"); }
        }
        private void RegisterCustomHotkey()
        {
            if (_msgHwnd == IntPtr.Zero) return;
            UnregisterHotKey(_msgHwnd, CUSTOM_HOTKEY_ID);
            bool ok = RegisterHotKey(_msgHwnd, CUSTOM_HOTKEY_ID,
                                      _customHotkeyMod, _customHotkeyVk);
            Debug.WriteLine($"[CustomHotkey] Register={ok} mod={_customHotkeyMod} vk=0x{_customHotkeyVk:X}");
        }
        private string HotkeyToString(int mod, int vk)
        {
            string modStr = "";
            if ((mod & MOD_CONTROL) != 0) modStr += "Ctrl+";
            if ((mod & MOD_ALT) != 0) modStr += "Alt+";
            char c = (char)vk;
            return modStr + (char.IsLetter(c) ? c.ToString().ToUpper() : $"0x{vk:X}");
        }
        private string GetSelectedGif()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customgif.cfg");
            if (System.IO.File.Exists(path)) return System.IO.File.ReadAllText(path).Trim();
            return "";
        }
        private void SaveSelectedGif(string name)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customgif.cfg");
            System.IO.File.WriteAllText(path, name);
            if (_floatingWindow != null)
            {
                Dispatcher.BeginInvoke(new Action(() => _floatingWindow.ReloadGif()));
            }
        }
        private WpfControls.ContextMenu BuildContextMenu()
        {
            var menuFontFamily = new FontFamily("宋体, Times New Roman");
            var menu = new ContextMenu { FontFamily = menuFontFamily, FontSize = 13 };
            var authorItem = new MenuItem { Header = "关  于", FontFamily = menuFontFamily };
            authorItem.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (Window w in this.Windows)
                    {
                        if (w is AuthorInfoDialog existing)
                        {
                            if (existing.WindowState == WindowState.Minimized)
                                existing.WindowState = WindowState.Normal;
                            existing.Activate();
                            return;
                        }
                    }
                    var dlg = new AuthorInfoDialog();
                    dlg.Show();
                    dlg.Activate();
                }), DispatcherPriority.Normal);
            };
            menu.Items.Add(authorItem);
            string currentHotkeyLabel = HotkeyToString(_customHotkeyMod, _customHotkeyVk);
            var hotkeyItem = new MenuItem
            {
                Header = $"快捷键（当前：{currentHotkeyLabel}）",
                FontFamily = menuFontFamily
            };
            hotkeyItem.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(ShowHotkeySettingDialog), DispatcherPriority.Normal);
            };
            menu.Items.Add(hotkeyItem);
            var popupModeItem = new MenuItem { Header = "弹出方式", FontFamily = menuFontFamily };
            var modeA = new MenuItem { Header = "仅快捷键弹出", IsCheckable = true, IsChecked = !_isAutoPopupEnabled, FontFamily = menuFontFamily };
            var modeB = new MenuItem { Header = "自动弹出和快捷键弹出", IsCheckable = true, IsChecked = _isAutoPopupEnabled, FontFamily = menuFontFamily };
            modeA.Click += (s, e) =>
            {
                _isAutoPopupEnabled = false;
                modeA.IsChecked = true;
                modeB.IsChecked = false;
                SaveHotkeyConfig();
            };
            modeB.Click += (s, e) =>
            {
                _isAutoPopupEnabled = true;
                modeA.IsChecked = false;
                modeB.IsChecked = true;
                SaveHotkeyConfig();
            };
            popupModeItem.Items.Add(modeA);
            popupModeItem.Items.Add(modeB);
            menu.Items.Add(popupModeItem);
            var configItem = new MenuItem { Header = "配置管理", FontFamily = menuFontFamily };
            configItem.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (Window w in this.Windows)
                    {
                        if (w is translation.ConfigWindow existing)
                        {
                            if (existing.WindowState == WindowState.Minimized)
                                existing.WindowState = WindowState.Normal;
                            existing.Activate();
                            return;
                        }
                    }
                    var configWin = new translation.ConfigWindow();
                    configWin.Show();
                    configWin.Activate();
                }), DispatcherPriority.Normal);
            };
            menu.Items.Add(configItem);
            var customGifItem = new MenuItem { Header = "自定义GIF", FontFamily = menuFontFamily };
            var addGifItem = new MenuItem { Header = "+ 添加GIF...", FontFamily = menuFontFamily };
            addGifItem.Click += (s, e) =>
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "GIF 文件 (*.gif)|*.gif",
                    Title = "选择自定义GIF"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomGifs");
                        System.IO.Directory.CreateDirectory(targetFolder);
                        string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                        string targetPath = System.IO.Path.Combine(targetFolder, fileName);
                        if (!System.IO.File.Exists(targetPath))
                            System.IO.File.Copy(openFileDialog.FileName, targetPath);
                        SaveSelectedGif(fileName);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[添加 GIF] {ex.Message}");
                    }
                }
            };
            customGifItem.Items.Add(addGifItem);
            var dirGifItem = new MenuItem { Header = "📂 打开GIF目录", FontFamily = menuFontFamily };
            dirGifItem.Click += (s, e) =>
            {
                try
                {
                    string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomGifs");
                    System.IO.Directory.CreateDirectory(targetFolder);
                    Process.Start(new ProcessStartInfo { FileName = targetFolder, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Open GIF Dir] {ex.Message}");
                }
            };
            customGifItem.Items.Add(dirGifItem);

            var delGifItem = new MenuItem { Header = "- 删除当前GIF", FontFamily = menuFontFamily };
            delGifItem.Click += (s, e) =>
            {
                string currentGif = GetSelectedGif();
                if (!string.IsNullOrEmpty(currentGif))
                {
                    var result = System.Windows.MessageBox.Show($"确定要删除当前选中的 GIF ({currentGif}) 吗？", "删除GIF", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        try
                        {
                            string targetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomGifs", currentGif);
                            if (System.IO.File.Exists(targetPath))
                            {
                                System.IO.File.Delete(targetPath);
                            }
                            SaveSelectedGif("");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[删除 GIF] {ex.Message}");
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("当前使用的是默认动画，无法删除。", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            };
            customGifItem.Items.Add(delGifItem);

            customGifItem.Items.Add(new Separator());
            var defaultGifItem = new MenuItem
            {
                Header = "默认动画 (pet.gif)",
                IsCheckable = true,
                IsChecked = string.IsNullOrEmpty(GetSelectedGif())
            };
            defaultGifItem.Click += (s, e) => SaveSelectedGif("");
            customGifItem.Items.Add(defaultGifItem);
            try
            {
                string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomGifs");
                if (System.IO.Directory.Exists(folder))
                {
                    foreach (var file in System.IO.Directory.GetFiles(folder, "*.gif"))
                    {
                        string fName = System.IO.Path.GetFileName(file);
                        var item = new MenuItem { Header = fName, IsCheckable = true, IsChecked = GetSelectedGif() == fName };
                        item.Click += (s, e) => SaveSelectedGif(fName);
                        customGifItem.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Load Gifs] {ex.Message}");
            }
            menu.Items.Add(customGifItem);
            menu.Items.Add(new Separator());
            var notebookItem = new MenuItem { Header = "📒 打开笔记本", FontFamily = menuFontFamily };
            notebookItem.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try {
                        foreach (Window w in this.Windows)
                        {
                            if (w is translation.Controls.NotebookWindow existing)
                            {
                                if (existing.WindowState == WindowState.Minimized)
                                    existing.WindowState = WindowState.Normal;
                                existing.Activate();
                                return;
                            }
                        }
                        var notebookWin = new translation.Controls.NotebookWindow();
                        notebookWin.Topmost = true;
                        notebookWin.Show();
                        notebookWin.Activate();
                        notebookWin.Topmost = false;
                    } catch (Exception ex) {
                        MessageBox.Show($"Failed to open NotebookWindow:\n{ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }), DispatcherPriority.Normal);
            };
            menu.Items.Add(notebookItem);
            menu.Items.Add(new Separator());
            var reloadItem = new MenuItem { Header = "重新加载", FontFamily = menuFontFamily };
            reloadItem.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(ReloadApplication), DispatcherPriority.Normal);
            };
            menu.Items.Add(reloadItem);
            var exitItem = new MenuItem { Header = "退出程序", FontFamily = menuFontFamily };
            exitItem.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(Shutdown), DispatcherPriority.Normal);
            };
            menu.Items.Add(exitItem);
            return menu;
        }
        private void ShowHotkeySettingDialog()
        {
            foreach (Window w in this.Windows)
            {
                if (w is HotkeySettingWindow existing)
                {
                    if (existing.WindowState == WindowState.Minimized)
                        existing.WindowState = WindowState.Normal;
                    existing.Activate();
                    return;
                }
            }

            var dialog = new HotkeySettingWindow(_customHotkeyMod, _customHotkeyVk);
            dialog.FontFamily = new FontFamily("宋体, Times New Roman");
            dialog.FontSize = 14;
            if (dialog.ShowDialog() == true)
            {
                _customHotkeyMod = dialog.ResultMod | MOD_NOREPEAT;
                _customHotkeyVk = dialog.ResultVk;
                SaveHotkeyConfig();
                RegisterCustomHotkey();
                Debug.WriteLine($"[Hotkey] Updated to {HotkeyToString(_customHotkeyMod, _customHotkeyVk)}");
            }
        }
        private void ReloadApplication()
        {
            try
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                {
                    var errDlg = new StyledMessageDialog(
                        "错误",
                        "无法获取程序路径，重新加载失败。",
                        isError: true);
                    errDlg.ShowDialog();
                    return;
                }
                Process.Start(new ProcessStartInfo { FileName = exePath, UseShellExecute = true });
                Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Reload] {ex}");
                var errDlg = new StyledMessageDialog("错误", $"重新加载失败：{ex.Message}", isError: true);
                errDlg.ShowDialog();
            }
        }
        private void SetupTrayMenu()
        {
            if (_notifyIcon == null) return;
            _notifyIcon.ContextMenu = null;
            _notifyIcon.TrayRightMouseUp += (s, e) =>
            {
                try
                {
                    if (e != null) e.Handled = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (_stateIconWindow == null) return;
                            _stateIconWindow.Activate();
                            var menu = BuildContextMenu();
                            NativeMethods.GetCursorPos(out var pt);
                            double dpiX = 1.0, dpiY = 1.0;
                            var src = PresentationSource.FromVisual(_stateIconWindow);
                            if (src != null)
                            {
                                dpiX = src.CompositionTarget.TransformToDevice.M11;
                                dpiY = src.CompositionTarget.TransformToDevice.M22;
                            }
                            menu.PlacementTarget = _stateIconWindow;
                            menu.Placement = PlacementMode.AbsolutePoint;
                            menu.HorizontalOffset = pt.X / dpiX;
                            menu.VerticalOffset = pt.Y / dpiY;
                            menu.Closed += (_, _) => _stateIconWindow.ContextMenu = null;
                            _stateIconWindow.ContextMenu = menu;
                            menu.IsOpen = true;
                        }
                        catch (Exception ex) { Debug.WriteLine($"[TrayMenu] {ex}"); }
                    }), DispatcherPriority.Input);
                }
                catch (Exception ex) { Debug.WriteLine($"[TrayMenu Dispatch] {ex}"); }
            };
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(
                        System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));
            DispatcherUnhandledException += (s, ex) =>
            {
                Debug.WriteLine($"[DispatcherUnhandled] {ex.Exception}");
                ex.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                Debug.WriteLine($"[AppDomainUnhandled] {ex.ExceptionObject}");
            };
            TaskScheduler.UnobservedTaskException += (s, ex) =>
            {
                Debug.WriteLine($"[TaskSchedulerUnobserved] {ex.Exception}");
                ex.SetObserved();
            };
            const string mutexName = "Global\\translation_by621_single_instance";
            _singleInstanceMutex = new Mutex(true, mutexName, out bool createdNew);
            if (!createdNew)
            {
                ShowAlreadyRunningDialog();
                Shutdown();
                return;
            }
            _floatingWindow = new FloatingWindow();
            _floatingWindow.Hide();
            _stateIconWindow = new StateIconWindow();
            _stateIconWindow.Left = SystemParameters.WorkArea.Left + 6;
            _stateIconWindow.Top = SystemParameters.WorkArea.Bottom - _stateIconWindow.Height - 6;
            _stateIconWindow.PreviewMouseRightButtonUp += StateIconWindow_PreviewMouseRightButtonUp;
            _stateIconWindow.Show();
            var helper = new System.Windows.Interop.WindowInteropHelper(_stateIconWindow);
            _msgHwnd = helper.Handle;
            var source = System.Windows.Interop.HwndSource.FromHwnd(_msgHwnd);
            source?.AddHook(MsgWndProc);
            LoadHotkeyConfig();
            RegisterCustomHotkey();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _stateIconWindow.Activate();
                _stateIconWindow.Topmost = true;
            }), DispatcherPriority.Background);
            _notifyIcon = new TaskbarIcon
            {
                ToolTipText = "划词工具",
                Visibility = Visibility.Visible
            };
            _notifyIcon.TrayLeftMouseUp += NotifyIcon_TrayLeftMouseUp;
            SetupTrayMenu();
            UpdateIcons();
            _mouseHook = new MouseHookService();
            _mouseHook.OnSelectionDetected += MouseHook_OnSelectionDetected;
            _mouseHook.OnMouseDown += MouseHook_OnMouseDown;
        }
        private void NotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e != null) e.Handled = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { ToggleTool(); }
                    catch (Exception ex) { Debug.WriteLine($"[TrayClick Toggle] {ex}"); }
                }), DispatcherPriority.Normal);
            }
            catch (Exception ex) { Debug.WriteLine($"[TrayClick Dispatch] {ex}"); }
        }
        private void StateIconWindow_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var menu = BuildContextMenu();
                _stateIconWindow.ContextMenu = menu;
                menu.PlacementTarget = _stateIconWindow;
                menu.Placement = PlacementMode.MousePoint;
                menu.Closed += (_, _) => _stateIconWindow.ContextMenu = null;
                _stateIconWindow.ContextMenu.IsOpen = true;
            }), DispatcherPriority.Input);
        }
        private void ReleaseMutexSafely()
        {
            if (_singleInstanceMutex == null) return;
            try { _singleInstanceMutex.ReleaseMutex(); } catch { }
            try { _singleInstanceMutex.Dispose(); } catch { }
            _singleInstanceMutex = null;
        }
        private async void OnHotkeyTriggered()
        {
            if (!_isToolEnabled) return;
            if (_floatingWindow != null && _floatingWindow.IsVisible)
            {
                _floatingWindow.Hide();
                return;
            }
            NativeMethods.GetCursorPos(out var pt);
            await Task.Delay(80);
            string sel = await GetSelectedTextAsync();
            GetDpiScaleForPoint(pt.X, pt.Y, out double dpiX, out double dpiY);
            IntPtr hMonitor = MonitorFromPoint(new POINT { X = pt.X, Y = pt.Y }, 2);
            Rect workArea;
            if (hMonitor != IntPtr.Zero)
            {
                NativeMethods.MONITORINFOEX monitorInfo = new NativeMethods.MONITORINFOEX();
                monitorInfo.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                if (NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    workArea = new Rect(
                        monitorInfo.rcWork.Left,
                        monitorInfo.rcWork.Top,
                        monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                        monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
                }
                else
                {
                    workArea = SystemParameters.WorkArea;
                }
            }
            else
            {
                workArea = SystemParameters.WorkArea;
            }
            _floatingWindow.SetText(string.IsNullOrWhiteSpace(sel) ? "" : sel);
            if (!_floatingWindow.IsVisible)
                _floatingWindow.Show();
            else
                _floatingWindow.Activate();
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(_floatingWindow);
                if (helper.Handle != IntPtr.Zero)
                {
                    int finalX = pt.X + 1;
                    int finalY = pt.Y + 1;
                    if (hMonitor != IntPtr.Zero)
                    {
                        var info = new NativeMethods.MONITORINFOEX();
                        info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                        if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
                        {
                            int physWidth = (int)(_floatingWindow.ActualWidth * dpiX);
                            int physHeight = (int)(_floatingWindow.ActualHeight * dpiY);
                            if (finalX + physWidth > info.rcWork.Right)
                                finalX = info.rcWork.Right - physWidth - 5;
                            if (finalY + physHeight > info.rcWork.Bottom)
                                finalY = info.rcWork.Bottom - physHeight - 5;
                            if (finalX < info.rcWork.Left)
                                finalX = info.rcWork.Left + 5;
                            if (finalY < info.rcWork.Top)
                                finalY = info.rcWork.Top + 5;
                        }
                    }
                    SetWindowPos(helper.Handle, IntPtr.Zero, finalX, finalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SetWindowPos Error] {ex.Message}");
            }
        }
        private void ShowAlreadyRunningDialog()
        {
            var dlg = new AlreadyRunningDialog();
            dlg.ShowDialog();
        }
        public void ToggleTool()
        {
            _isToolEnabled = !_isToolEnabled;
            UpdateIcons();
            SaveHotkeyConfig();
        }
        private void UpdateIcons()
        {
            try { _stateIconWindow?.UpdateIcon(_isToolEnabled); }
            catch (Exception ex) { Debug.WriteLine($"[UpdateStateIcon] {ex}"); }
            if (_notifyIcon == null || _trayIcon != null) return;
            string iconPath = IconHelper.GetIconPath("tray.ico");
            if (string.IsNullOrEmpty(iconPath) || !File.Exists(iconPath))
            {
                TryUseExeIcon();
                return;
            }
            try
            {
                if (iconPath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    _trayIcon = new System.Drawing.Icon(iconPath);
                    _notifyIcon.Icon = _trayIcon;
                }
                else
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(iconPath, UriKind.Absolute);
                    bmp.EndInit();
                    _notifyIcon.IconSource = bmp;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateTrayIcon] {ex}");
                TryUseExeIcon();
            }
        }
        private void TryUseExeIcon()
        {
            try
            {
                if (_trayIcon != null) return;
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath)) return;
                _trayIcon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (_trayIcon != null) _notifyIcon.Icon = _trayIcon;
            }
            catch (Exception ex) { Debug.WriteLine($"[FallbackTrayIcon] {ex}"); }
        }
        private void MouseHook_OnMouseDown(object sender, WpfPoint e)
        {
            if (!_isToolEnabled || _floatingWindow == null || !_floatingWindow.IsVisible) return;
            if (IsPointInWindow(_floatingWindow, e) || IsPointInWindow(_stateIconWindow, e)) return;
            if (_floatingWindow.MyFloatingMenu.IsPinned) return;
            _floatingWindow.Hide();
        }
        private async void MouseHook_OnSelectionDetected(object sender, WpfPoint e)
        {
            if (!_isAutoPopupEnabled) return;
            if (!_isToolEnabled || _floatingWindow == null || _floatingWindow.IsActive) return;
            if (_floatingWindow.IsVisible && IsPointInWindow(_floatingWindow, e)) return;
            await Task.Delay(150);
            string sel = await GetSelectedTextAsync();
            if (string.IsNullOrWhiteSpace(sel)) return;
            try
            {
                GetDpiScaleForPoint((int)e.X, (int)e.Y, out double dpiX, out double dpiY);
                IntPtr hMonitor = MonitorFromPoint(new POINT { X = (int)e.X, Y = (int)e.Y }, 2);
                Rect workArea;
                if (hMonitor != IntPtr.Zero)
                {
                    NativeMethods.MONITORINFOEX monitorInfo = new NativeMethods.MONITORINFOEX();
                    monitorInfo.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                    if (NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
                    {
                        workArea = new Rect(
                            monitorInfo.rcWork.Left,
                            monitorInfo.rcWork.Top,
                            monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                            monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
                    }
                    else
                    {
                        workArea = SystemParameters.WorkArea;
                    }
                }
                else
                {
                    workArea = SystemParameters.WorkArea;
                }
                _floatingWindow.SetText(sel);
                if (!_floatingWindow.IsVisible) _floatingWindow.Show();
                try
                {
                    if (!_floatingWindow.MyFloatingMenu.IsPinned)
                    {
                        var helper = new System.Windows.Interop.WindowInteropHelper(_floatingWindow);
                        if (helper.Handle != IntPtr.Zero)
                        {
                            int finalX = (int)e.X + 1;
                            int finalY = (int)e.Y + 1;
                            if (hMonitor != IntPtr.Zero)
                            {
                                var info = new NativeMethods.MONITORINFOEX();
                                info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                                if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
                                {
                                    int physWidth = (int)(_floatingWindow.ActualWidth * dpiX);
                                    int physHeight = (int)(_floatingWindow.ActualHeight * dpiY);
                                    if (finalX + physWidth > info.rcWork.Right)
                                        finalX = info.rcWork.Right - physWidth - 5;
                                    if (finalY + physHeight > info.rcWork.Bottom)
                                        finalY = info.rcWork.Bottom - physHeight - 5;
                                    if (finalX < info.rcWork.Left)
                                        finalX = info.rcWork.Left + 5;
                                    if (finalY < info.rcWork.Top)
                                        finalY = info.rcWork.Top + 5;
                                }
                            }
                            SetWindowPos(helper.Handle, IntPtr.Zero, finalX, finalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SetWindowPos Error OnSelection] {ex.Message}");
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[OnSelection] {ex.Message}"); }
        }
        private static bool IsPointInWindow(Window window, System.Windows.Point p)
        {
            if (window == null || !window.IsVisible) return false;
            var ownerHandle =
                new System.Windows.Interop.WindowInteropHelper(window).Handle;
            if (ownerHandle == IntPtr.Zero) return false;
            if (RectContainsPoint(ownerHandle, p)) return true;
            bool found = false;
            EnumWindows((hWnd, _) =>
            {
                if (hWnd == ownerHandle) return true;
                var owner = GetWindow(hWnd, GW_OWNER);
                if (owner == ownerHandle && RectContainsPoint(hWnd, p))
                {
                    found = true;
                    return false; 
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }
        private static bool RectContainsPoint(IntPtr hWnd, System.Windows.Point p)
        {
            if (!GetWindowRect(hWnd, out RECT r)) return false;
            return p.X >= r.Left && p.X <= r.Right
                && p.Y >= r.Top  && p.Y <= r.Bottom;
        }
        private async Task<string> GetSelectedTextAsync()
        {
            string prev = null;
            bool hasPrev = false;
            try { if (Clipboard.ContainsText()) { prev = Clipboard.GetText(); hasPrev = true; } } catch { }
            try { Clipboard.Clear(); } catch { }
            try
            {
                NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_C, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_C, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch (Exception ex) { Debug.WriteLine($"[Ctrl+C] {ex.Message}"); }
            string result = null;
            for (int i = 0; i < 15; i++)
            {
                await Task.Delay(30 + i * 5);
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        result = Clipboard.GetText();
                        if (!string.IsNullOrEmpty(result)) break;
                    }
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    Debug.WriteLine($"[CB COM] {i + 1}: {ex.Message}");
                }
                catch (Exception ex) { Debug.WriteLine($"[CB] {i + 1}: {ex.Message}"); }
            }
            try
            {
                if (hasPrev && !string.IsNullOrEmpty(prev)) 
                {
                    
                    await Task.Delay(1000);
                    Clipboard.SetText(prev);
                }
                else 
                {
                    Clipboard.Clear();
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[RestoreCB] {ex.Message}"); }
            return result;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (_msgHwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_msgHwnd, CUSTOM_HOTKEY_ID);
                _msgHwnd = IntPtr.Zero;
            }
            _notifyIcon?.Dispose();
            _trayIcon?.Dispose();
            _mouseHook?.Dispose();
            ReleaseMutexSafely();
            base.OnExit(e);
        }
    }
    public class StyledMessageDialog : StyledDialogWindow
    {
        public StyledMessageDialog(string title, string message, bool isError = false)
        {
            Title = title;
            Width = 360;
            SizeToContent = SizeToContent.Height;
            var root = new Border
            {
                Background = Brushes.White,
                Padding = new Thickness(28, 24, 28, 20),
                CornerRadius = new CornerRadius(6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                BorderThickness = new Thickness(1)
            };
            var panel = new StackPanel();
            var titleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            titleRow.Children.Add(new TextBlock
            {
                Text = isError ? "✕" : "ℹ",
                FontSize = 18,
                Foreground = isError
                    ? new SolidColorBrush(Color.FromRgb(196, 43, 28))
                    : new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                Margin = new Thickness(0, 1, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            var tb = MakeTitleBlock(title);
            tb.Margin = new Thickness(0);
            tb.VerticalAlignment = VerticalAlignment.Center;
            titleRow.Children.Add(tb);
            panel.Children.Add(titleRow);
            panel.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                Margin = new Thickness(0, 0, 0, 14)
            });
            panel.Children.Add(MakeBodyBlock(message));
            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            var okBtn = MakeButton("确定", isPrimary: true);
            okBtn.Click += (_, _) => Close();
            btnRow.Children.Add(okBtn);
            panel.Children.Add(btnRow);
            root.Child = panel;
            Content = root;
        }
    }
}
