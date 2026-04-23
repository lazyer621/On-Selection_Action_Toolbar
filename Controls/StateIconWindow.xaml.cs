using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using WpfInput = System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using WpfPoint = System.Windows.Point;
namespace translation.Controls
{
    public partial class StateIconWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOOWNERZORDER = 0x0200;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private BitmapImage _iconOn;
        private BitmapImage _iconOff;
        private bool _isDragging = false;
        private bool _isClosing = false;
        private WpfPoint _startPoint;
        private DispatcherTimer _keepAliveTimer;
        private DispatcherTimer _visibilityTimer;
        public StateIconWindow()
        {
            InitializeComponent();
            InitializeIcons();
            InitializeKeepAliveTimer();
            InitializeVisibilityTimer();
            this.Deactivated += StateIconWindow_Deactivated;
            this.LocationChanged += StateIconWindow_LocationChanged;
            this.StateChanged += StateIconWindow_StateChanged;
            this.Activated += StateIconWindow_Activated;
            this.IsVisibleChanged += StateIconWindow_IsVisibleChanged;
            this.Loaded += StateIconWindow_Loaded;
        }
        private void StateIconWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ForceTopmost();
        }
        private void ForceTopmost()
        {
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                IntPtr hWnd = helper.Handle;
                if (hWnd != IntPtr.Zero)
                {
                    SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ForceTopmost API failed, falling back to Topmost property: {ex.Message}");
                this.Topmost = true;
            }
        }
        private void InitializeIcons()
        {
            try
            {
                string onPath = Services.IconHelper.GetIconPath("on.ico");
                if (File.Exists(onPath))
                {
                    _iconOn = new BitmapImage();
                    _iconOn.BeginInit();
                    _iconOn.UriSource = new Uri(onPath, UriKind.Absolute);
                    _iconOn.CacheOption = BitmapCacheOption.OnLoad;
                    _iconOn.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    _iconOn.EndInit();
                    _iconOn.Freeze();
                }
                string offPath = Services.IconHelper.GetIconPath("off.ico");
                if (File.Exists(offPath))
                {
                    _iconOff = new BitmapImage();
                    _iconOff.BeginInit();
                    _iconOff.UriSource = new Uri(offPath, UriKind.Absolute);
                    _iconOff.CacheOption = BitmapCacheOption.OnLoad;
                    _iconOff.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    _iconOff.EndInit();
                    _iconOff.Freeze();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeIcons failed: {ex.Message}");
            }
        }
        private void EnsureWindowVisible()
        {
            try
            {
                if (!this.IsVisible)
                {
                    this.Show();
                    Debug.WriteLine("StateIconWindow was hidden, now shown");
                }
                ForceTopmost();
                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                    Debug.WriteLine("StateIconWindow was minimized, restored to normal");
                }
                this.InvalidateVisual();
                this.UpdateLayout();
                if (IconImg != null)
                {
                    IconImg.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in EnsureWindowVisible: {ex.Message}");
            }
        }
        private void InitializeKeepAliveTimer()
        {
            _keepAliveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)  
            };
            _keepAliveTimer.Tick += KeepAliveTimer_Tick;
            _keepAliveTimer.Start();
        }
        private void InitializeVisibilityTimer()
        {
            _visibilityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)  
            };
            _visibilityTimer.Tick += (s, e) =>
            {
                try
                {
                    EnsureWindowVisible();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in visibility timer: {ex.Message}");
                }
            };
            _visibilityTimer.Start();
        }
        private void KeepAliveTimer_Tick(object sender, EventArgs e)
        {
            if (_isClosing || this.ContextMenu?.IsOpen == true) return;
            try
            {
                if (!this.IsVisible)
                {
                    this.Show();
                    Debug.WriteLine("StateIconWindow was hidden, now shown again");
                }
                if (!this.Topmost)
                {
                    this.Topmost = true;
                    Debug.WriteLine("StateIconWindow not topmost, reapplying Topmost=true");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in KeepAliveTimer_Tick: {ex.Message}");
            }
        }
        private void StateIconWindow_Deactivated(object sender, EventArgs e)
        {
            if (_isClosing || this.ContextMenu?.IsOpen == true) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_isClosing || this.ContextMenu?.IsOpen == true) return;
                if (!this.IsVisible)
                {
                    this.Show();
                    Debug.WriteLine("StateIconWindow deactivated but hidden, showing it");
                }
                if (!this.Topmost)
                {
                    this.Topmost = true;
                    Debug.WriteLine("StateIconWindow deactivated and lost topmost, restoring");
                }
                ForceTopmost();
            }), DispatcherPriority.ContextIdle);
        }
        private void StateIconWindow_LocationChanged(object sender, EventArgs e)
        {
            if (_isClosing || this.ContextMenu?.IsOpen == true) return;
            if (!this.IsVisible)
            {
                this.Show();
            }
            ForceTopmost();
            Debug.WriteLine($"StateIconWindow location changed to ({this.Left}, {this.Top})");
        }
        private void StateIconWindow_StateChanged(object sender, EventArgs e)
        {
            if (_isClosing) return;
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                Debug.WriteLine("StateIconWindow minimized, restored to normal");
            }
            if (!this.IsVisible)
            {
                this.Show();
            }
            ForceTopmost();
        }
        private void StateIconWindow_Activated(object sender, EventArgs e)
        {
            if (IconImg != null)
            {
                IconImg.InvalidateVisual();
                IconImg.UpdateLayout();
            }
            Debug.WriteLine("StateIconWindow activated, visual updated");
        }
        private void StateIconWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isClosing) return;
            if (!this.IsVisible)
            {
                this.Show();
                Debug.WriteLine("StateIconWindow visibility changed to hidden, showing it");
            }
            ForceTopmost();
        }
        public void UpdateIcon(bool isOn)
        {
            if (!this.CheckAccess())
            {
                this.Dispatcher.Invoke(() => UpdateIcon(isOn));
                return;
            }
            try
            {
                if (isOn && _iconOn != null)
                {
                    IconImg.Source = _iconOn;
                }
                else if (!isOn && _iconOff != null)
                {
                    IconImg.Source = _iconOff;
                }
                IconImg.InvalidateVisual();
                IconImg.UpdateLayout();
                Debug.WriteLine($"StateIconWindow icon updated to {(isOn ? "on.ico" : "off.ico")}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateIcon failed: {ex.Message}");
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            _isDragging = false;
            this.CaptureMouse();
        }
        protected override void OnMouseMove(WpfInput.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && this.IsMouseCaptured)
            {
                WpfPoint currentPoint = e.GetPosition(this);
                if (Math.Abs(currentPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    this.ReleaseMouseCapture();
                    this.DragMove();
                }
            }
        }
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
                if (!_isDragging)
                {
                    ((App)Application.Current).ToggleTool();
                }
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            _isClosing = true;
            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Stop();
                _keepAliveTimer = null;
            }
            if (_visibilityTimer != null)
            {
                _visibilityTimer.Stop();
                _visibilityTimer = null;
            }
            this.Deactivated -= StateIconWindow_Deactivated;
            this.LocationChanged -= StateIconWindow_LocationChanged;
            this.StateChanged -= StateIconWindow_StateChanged;
            this.Activated -= StateIconWindow_Activated;
            this.IsVisibleChanged -= StateIconWindow_IsVisibleChanged;
            this.Loaded -= StateIconWindow_Loaded;
            base.OnClosed(e);
        }
    }
}
