using System.Runtime.InteropServices;

namespace VirtualDesktopSwitcher
{
    internal static class KeyboardEventHelper
    {
        public const int KEYEVENTF_KEYDOWN = 0x0000; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        //Key Code
        public const int VK_LCONTROL = 0x0011;
        public const int VK_WIN = 0x005B;
        public const int VK_LEFT = 0x0025;
        public const int VK_RIGHT = 0x0027;
        public const int KEY_D = 0x0044;
        public const int VK_F4 = 0x0073;
        public const int VK_TAB = 0x0009;

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void OpenNewDesktop() => CombineKey(KEY_D);

        public static void CloseCurrentDesktop() => CombineKey(VK_F4);

        public static void GoToLeftDesktop() => CombineKey(VK_LEFT);

        public static void GoToRightDesktop() => CombineKey(VK_RIGHT);

        private static void CombineKey(byte KEY)
        {
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(KEY, 0, KEYEVENTF_KEYDOWN, 0);

            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(KEY, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}
