using System.IO;
using System.Windows;
using translation.Controls;
using System.Reflection;
using System;
using System.Linq;
namespace translation.Services
{
    public static class IconHelper
    {
        private static readonly string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        public static string GetIconPath(string iconName)
        {
            string resourceName = ResourceNames.FirstOrDefault(r => 
                r.EndsWith($".imgs.{iconName}") || 
                r.EndsWith($".imgs\\{iconName}") ||
                r.EndsWith($"imgs.{iconName}"));
            if (!string.IsNullOrEmpty(resourceName))
            {
                try
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            string tempPath = Path.Combine(Path.GetTempPath(), $"AI 划词助手@621_{iconName}");
                            bool needCreate = true;
                            if (File.Exists(tempPath))
                            {
                                try
                                {
                                    if (new FileInfo(tempPath).Length == stream.Length)
                                    {
                                        needCreate = false;
                                    }
                                }
                                catch { }
                            }
                            if (needCreate)
                            {
                                using (var fileStream = File.Create(tempPath))
                                {
                                    stream.CopyTo(fileStream);
                                }
                            }
                            return tempPath;
                        }
                    }
                }
                catch
                {
                }
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imgs", iconName);
        }
        private static StateIconWindow _stateIconWindow;
        public static void ShowStateIconWindow()
        {
            _stateIconWindow = new StateIconWindow();
            _stateIconWindow.Left = SystemParameters.WorkArea.Left + 6;
            _stateIconWindow.Top = SystemParameters.WorkArea.Bottom - _stateIconWindow.Height - 6;
            _stateIconWindow.Show();
        }
    }
}