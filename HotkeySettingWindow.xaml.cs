using System.Collections.Generic;
using System.Windows;
using WpfInput = System.Windows.Input;
using WpfMedia = System.Windows.Media;
namespace translation
{
    public partial class HotkeySettingWindow : Window
    {
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_ALT     = 0x0001;
        private static readonly WpfMedia.FontFamily FontMixed = new WpfMedia.FontFamily("宋体, Times New Roman");
        private static readonly HashSet<Key> AllowedKeys = BuildAllowedKeys();
        private static HashSet<Key> BuildAllowedKeys()
        {
            var set = new HashSet<Key>();
            for (var k = Key.D0;      k <= Key.D9;      k++) set.Add(k); 
            for (var k = Key.NumPad0; k <= Key.NumPad9; k++) set.Add(k); 
            for (var k = Key.F1;      k <= Key.F12;     k++) set.Add(k); 
            return set;
        }
        public int ResultMod { get; private set; }
        public int ResultVk  { get; private set; }
        private int _pendingVk = 0; 
        public HotkeySettingWindow(int currentMod, int currentVk)
        {
            InitializeComponent();
            ChkCtrl.IsChecked = (currentMod & MOD_CONTROL) != 0;
            ChkAlt.IsChecked  = (currentMod & MOD_ALT)     != 0;
            if (currentVk > 0)
            {
                _pendingVk = currentVk;
                var key = KeyInterop.KeyFromVirtualKey(currentVk);
                TxtKey.Text = VkToDisplayString(currentVk, key);
                TxtKey.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            }
            else
            {
                SetPlaceholder();
            }
            UpdateHint();
        }
        private void Mod_Changed(object sender, RoutedEventArgs e) => UpdateHint();
        private void TxtKey_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_pendingVk == 0)
                SetPlaceholder(focused: true);
        }
        private void TxtKey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_pendingVk == 0)
                SetPlaceholder(focused: false);
        }
        private void TxtKey_PreviewKeyDown(object sender, WpfInput.KeyEventArgs e)
        {
            e.Handled = true; 
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifierKey(key)) return;
            if (key == Key.Back || key == Key.Delete)
            {
                _pendingVk = 0;
                SetPlaceholder(focused: true);
                UpdateHint();
                return;
            }
            bool isLetter  = key >= Key.A && key <= Key.Z;
            bool isAllowed = isLetter || AllowedKeys.Contains(key);
            if (!isAllowed)
            {
                SetHint("不支持该按键，请使用字母、数字键或 F1–F12。", HintLevel.Error);
                return;
            }
            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk <= 0)
            {
                SetHint("无法识别该按键，请重试。", HintLevel.Error);
                return;
            }
            _pendingVk = vk;
            TxtKey.Text = VkToDisplayString(vk, key);
            TxtKey.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            UpdateHint();
        }
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm()) return;
            int mod = 0;
            if (ChkCtrl.IsChecked == true) mod |= MOD_CONTROL;
            if (ChkAlt.IsChecked  == true) mod |= MOD_ALT;
            ResultMod    = mod;
            ResultVk     = _pendingVk;
            DialogResult = true;
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private bool CanConfirm() =>
            (_pendingVk > 0) &&
            (ChkCtrl.IsChecked == true || ChkAlt.IsChecked == true);
        private void UpdateHint()
        {
            bool hasMod = ChkCtrl.IsChecked == true || ChkAlt.IsChecked == true;
            bool hasKey = _pendingVk > 0;
            if (!hasMod && !hasKey)
                SetHint("请勾选修饰键，再点击输入框按下主键。", HintLevel.Info);
            else if (!hasMod)
                SetHint("请至少勾选一个修饰键（Ctrl / Alt）。", HintLevel.Error);
            else if (!hasKey)
                SetHint("请点击上方输入框，然后按下主键。", HintLevel.Info);
            else
            {
                string mod = BuildModString();
                string key = VkToDisplayString(_pendingVk,
                                 KeyInterop.KeyFromVirtualKey(_pendingVk));
                SetHint($"预览：{mod}{key}", HintLevel.Ok);
            }
            BtnOk.IsEnabled = CanConfirm();
        }
        private string BuildModString()
        {
            string s = "";
            if (ChkCtrl.IsChecked == true) s += "Ctrl + ";
            if (ChkAlt.IsChecked  == true) s += "Alt + ";
            return s;
        }
        private void SetPlaceholder(bool focused = false)
        {
            TxtKey.Text = focused
                ? "请按下目标按键…"
                : "（点击此处，再按下目标按键）";
            TxtKey.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        }
        private enum HintLevel { Info, Error, Ok }
        private void SetHint(string text, HintLevel level)
        {
            TbHint.Text = text;
            TbHint.Foreground = level switch
            {
                HintLevel.Error => new SolidColorBrush(Color.FromRgb(196, 43, 28)),
                HintLevel.Ok    => new SolidColorBrush(Color.FromRgb(0, 128, 0)),
                _               => new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
        }
        private static bool IsModifierKey(Key key) =>
            key == Key.LeftCtrl  || key == Key.RightCtrl  ||
            key == Key.LeftAlt   || key == Key.RightAlt   ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin      || key == Key.RWin       ||
            key == Key.None;
        private static string VkToDisplayString(int vk, Key key)
        {
            if (key >= Key.A      && key <= Key.Z)      return key.ToString();           
            if (key >= Key.D0     && key <= Key.D9)     return ((int)(key - Key.D0)).ToString(); 
            if (key >= Key.NumPad0 && key <= Key.NumPad9) return $"Num{(int)(key - Key.NumPad0)}"; 
            if (key >= Key.F1     && key <= Key.F12)    return key.ToString();           
            return $"0x{vk:X2}";
        }
    }
}
