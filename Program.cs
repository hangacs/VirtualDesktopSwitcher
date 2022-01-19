using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace VirtualDesktopSwitcher
{
    static class Program
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

        private static NotifyIcon notifyIconLeft;
        private static NotifyIcon notifyIconRight;

        private static bool StartWithWindows;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            StartWithWindows = IsStartWithWindows();
            var contextMenuStrip = GetContext();

            notifyIconLeft = new NotifyIcon() {
                Text = "Left",
                ContextMenuStrip = contextMenuStrip,
                Icon = Properties.Resources.left_light,
                Visible = true,
            };

            notifyIconRight = new NotifyIcon {
                Text = "Right",
                ContextMenuStrip = contextMenuStrip,
                Icon = Properties.Resources.right_light,
                Visible = true,
            };

            notifyIconLeft.MouseClick += (o, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    GoToLeftDesktop();
                }
                else if (e.Button== MouseButtons.Right)
                {
                    StartWithWindows = IsStartWithWindows();
                    (contextMenuStrip.Items[3] as ToolStripMenuItem).Checked = StartWithWindows;
                }
            };

            notifyIconRight.MouseClick += (o, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    GoToRightDesktop();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    StartWithWindows = IsStartWithWindows();
                    (contextMenuStrip.Items[3] as ToolStripMenuItem).Checked = StartWithWindows;
                }
            };

            Application.Run();
        }

        private static ContextMenuStrip GetContext()
        {
            ContextMenuStrip CMS = new ContextMenuStrip();

            CMS.Items.Add("About", null, new EventHandler((o, e) => System.Diagnostics.Process.Start("https://github.com/hangacs/VirtualDesktopSwitcher")));
            CMS.Items.Add("Close Current Desktop", null, new EventHandler((o, e) => CloseCurrentDesktop()));
            CMS.Items.Add("Open New Desktop", null, new EventHandler((o, e) => OpenNewDesktop()));
            CMS.Items.Add(new ToolStripMenuItem("Start With Windows", null,
                (o, e) => {
                    StartWithWindows = !StartWithWindows;
                    AutoStart(StartWithWindows);
                    (o as ToolStripMenuItem).Checked = StartWithWindows;
                }) {
                Checked = StartWithWindows
            });
            CMS.Items.Add("Exit", null, new EventHandler((o, e) => {
                notifyIconLeft.Dispose();
                notifyIconRight.Dispose();
                Application.Exit();
            }));

            return CMS;
        }

        #region Keyboard Event
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
        #endregion Keyboard Event

        private static bool IsStartWithWindows()
        {
            try
            {
                bool result;
                RegistryKey R_local = Registry.CurrentUser;//RegistryKey R_local = Registry.CurrentUser;
                RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                result = Application.ExecutablePath == R_run.GetValue("VirtualDesktopSwitcher") as string;
                R_run.Close();
                R_local.Close();

                return result;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add registry key. {ex.Message}", "Error");
                return false;
            }
        }

        private static void AutoStart(bool autoStart)
        {
            try
            {
                if (autoStart)
                {
                    RegistryKey R_local = Registry.CurrentUser;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    R_run.SetValue("VirtualDesktopSwitcher", Application.ExecutablePath);
                    R_run.Close();
                    R_local.Close();
                }
                else
                {
                    RegistryKey R_local = Registry.CurrentUser;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    R_run.DeleteValue("VirtualDesktopSwitcher", false);
                    R_run.Close();
                    R_local.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add/remove registry key. {ex.Message}", "Error");
            }
        }
    }
}
