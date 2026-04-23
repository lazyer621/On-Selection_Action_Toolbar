using System;
using System.Windows;
using translation.Services;
using Forms = System.Windows.Forms;
namespace translation.Controls
{
    public partial class FloatingWindow : Window
    {
        private double _dpiX = 1.0;
        private double _dpiY = 1.0;
        private bool _userResizing = false; 
        public FloatingWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.PrimaryScreenHeight / 2;
            MyFloatingMenu.RequestClose += (s, e) => { Hide(); };
            Loaded += (_, _) => EnsureOnScreen();
            LocationChanged += (_, _) => EnsureOnScreen();
            SizeChanged += (_, _) =>
            {
                EnsureOnScreen();
                var menu = MyFloatingMenu;
                if (menu != null)
                {
                    double minH = menu.GetMinimumRequiredHeight();
                    minH /= _dpiY;
                    if (minH > 0 && this.MinHeight != minH)
                    {
                        this.MinHeight = minH;
                    }
                }
            };
            this.Deactivated += FloatingWindow_Deactivated;
        }
        public void ReloadGif()
        {
            MyFloatingMenu.ReloadGif();
        }
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == System.Windows.Input.Key.X && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                MyFloatingMenu.PerformSystemCut();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.C && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                MyFloatingMenu.PerformSystemCopy();
                e.Handled = true;
            }
        }
        public void SetText(string text)
        {
            MyFloatingMenu.SetSelectedText(text);
            if (!_userResizing)
            {
                this.SizeToContent = SizeToContent.Manual;
                this.SizeToContent = SizeToContent.Height;
            }
        }
        private DateTime _lastShownTime = DateTime.MinValue;
        public new void Show()
        {
            _userResizing = false;
            this.SizeToContent = SizeToContent.Manual;
            this.SizeToContent = SizeToContent.Height;
            this.MaxHeight = SystemParameters.WorkArea.Height / 2 + 300;
            this.MaxWidth = SystemParameters.WorkArea.Width / 2 + 500;
            _lastShownTime = DateTime.UtcNow;
            try { base.Show(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FloatingWindow.Show(): {ex.Message}");
                try { base.Show(); } catch { }
            }
        }
        private void FloatingWindow_Deactivated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if ((DateTime.UtcNow - _lastShownTime).TotalMilliseconds < 300)
                    return;

                bool hasVisibleOwnedWindow = false;
                foreach (Window owned in this.OwnedWindows)
                {
                    if (owned.IsVisible)
                    {
                        hasVisibleOwnedWindow = true;
                        break;
                    }
                }

                if (!this.IsActive && this.IsVisible && !hasVisibleOwnedWindow && !MyFloatingMenu.IsPinned)
                    Hide();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        private void Resize_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.Thumb thumb && thumb.Tag is string tag)
            {
                if (SizeToContent != SizeToContent.Manual) SizeToContent = SizeToContent.Manual;
                if (!_userResizing)
                {
                    _userResizing = true;
                    var workArea = SystemParameters.WorkArea;
                    this.MaxHeight = workArea.Height;
                    this.MaxWidth = workArea.Width;
                }
                double deltaH = e.HorizontalChange;
                double deltaV = e.VerticalChange;
                deltaH /= _dpiX;
                deltaV /= _dpiY;
                if (tag.Contains("Right")) Width = Math.Max(MinWidth, Width + deltaH);
                if (tag.Contains("Bottom")) Height = Math.Max(MinHeight, Height + deltaV);
                if (tag.Contains("Left")) { double change = Math.Min(deltaH, Width - MinWidth); Left += change; Width -= change; }
                if (tag.Contains("Top")) { double change = Math.Min(deltaV, Height - MinHeight); Top += change; Height -= change; }
            }
        }
        private Rect GetWorkAreaForWindow()
        {
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                if (helper.Handle == IntPtr.Zero)
                {
                    return SystemParameters.WorkArea;
                }
                IntPtr hMonitor = NativeMethods.MonitorFromWindow(helper.Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);
                if (hMonitor == IntPtr.Zero)
                {
                    return SystemParameters.WorkArea;
                }
                NativeMethods.MONITORINFOEX monitorInfo = new NativeMethods.MONITORINFOEX();
                monitorInfo.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                if (NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    return new Rect(
                        monitorInfo.rcWork.Left,
                        monitorInfo.rcWork.Top,
                        monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                        monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetWorkAreaForWindow] {ex.Message}");
            }
            return SystemParameters.WorkArea;
        }
        private void EnsureOnScreen()
        {
        }
        private Rect GetCombinedScreenWorkArea()
        {
            try
            {
                var screens = Forms.Screen.AllScreens;
                if (screens != null && screens.Length > 0)
                {
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;
                    foreach (var screen in screens)
                    {
                        minX = Math.Min(minX, screen.WorkingArea.Left);
                        minY = Math.Min(minY, screen.WorkingArea.Top);
                        maxX = Math.Max(maxX, screen.WorkingArea.Right);
                        maxY = Math.Max(maxY, screen.WorkingArea.Bottom);
                    }
                    return new Rect(minX, minY, maxX - minX, maxY - minY);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetCombinedScreenWorkArea] {ex.Message}");
            }
            return SystemParameters.WorkArea;
        }
    }
}
