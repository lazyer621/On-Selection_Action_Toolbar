using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using WpfPoint = System.Windows.Point;
using translation.Services;
namespace translation.Services
{
    public class MouseHookService : IDisposable
    {
        private static NativeMethods.LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static MouseHookService _instance;
        private NativeMethods.POINT _startPoint;
        private bool _isPressed = false;
        private const int DragThreshold = 5;
        public event EventHandler<WpfPoint> OnSelectionDetected;
        public event EventHandler<WpfPoint> OnMouseDown;
        public MouseHookService()
        {
            _instance = this;
            _hookID = SetHook(_proc);
        }
        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
            _instance = null;
        }
        private static IntPtr SetHook(NativeMethods.LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _instance != null)
            {
                try
                {
                    int msg = (int)wParam;
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    if (msg == NativeMethods.WM_LBUTTONDOWN)
                    {
                        _instance._startPoint = hookStruct.pt;
                        _instance._isPressed = true;
                        if (Application.Current != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    _instance.OnMouseDown?.Invoke(_instance, new WpfPoint(hookStruct.pt.X, hookStruct.pt.Y));
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error in OnMouseDown: {ex.Message}");
                                }
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                    if (msg == NativeMethods.WM_RBUTTONDOWN || msg == NativeMethods.WM_MBUTTONDOWN)
                    {
                        if (Application.Current != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    _instance.OnMouseDown?.Invoke(_instance, new WpfPoint(hookStruct.pt.X, hookStruct.pt.Y));
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error in OnMouseDown: {ex.Message}");
                                }
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                    if (msg == NativeMethods.WM_LBUTTONUP)
                    {
                        if (_instance._isPressed)
                        {
                            int dx = hookStruct.pt.X - _instance._startPoint.X;
                            int dy = hookStruct.pt.Y - _instance._startPoint.Y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            if (distance > DragThreshold)
                            {
                                if (Application.Current != null)
                                {
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        try
                                        {
                                            _instance.OnSelectionDetected?.Invoke(_instance, new WpfPoint(hookStruct.pt.X, hookStruct.pt.Y));
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Error in OnSelectionDetected: {ex.Message}");
                                        }
                                    }), System.Windows.Threading.DispatcherPriority.Background);
                                }
                            }
                        }
                        _instance._isPressed = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in HookCallback: {ex.Message}");
                }
            }
            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public NativeMethods.POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
