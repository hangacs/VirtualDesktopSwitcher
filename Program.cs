using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;
using static VirtualDesktopSwitcher.KeyboardEventHelper;

namespace VirtualDesktopSwitcher
{
    static class Program
    {
        private static NotifyIcon notifyIconLeft;
        private static NotifyIcon notifyIconRight;

        private static bool StartWithWindows;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitializeNotifyIcon();
            SetMouseWheelHook();

            Application.Run();
        }

        private static void InitializeNotifyIcon()
        {
            StartWithWindows = IsStartWithWindows();
            var contextMenuStrip = GetContext();

            notifyIconLeft = new NotifyIcon()
            {
                Text = "Left",
                ContextMenuStrip = contextMenuStrip,
                Icon = Properties.Resources.left_light,
                Visible = true,
            };

            notifyIconRight = new NotifyIcon
            {
                Text = "Right",
                ContextMenuStrip = contextMenuStrip,
                Icon = Properties.Resources.right_light,
                Visible = true,
            };

            notifyIconLeft.MouseClick += (object o, MouseEventArgs e) => NotifyIconClick(o, e, contextMenuStrip);
            notifyIconRight.MouseClick += (object o, MouseEventArgs e) => NotifyIconClick(o, e, contextMenuStrip);
        }

        private static void NotifyIconClick(object o, MouseEventArgs e, ContextMenuStrip contextMenuStrip)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (o as NotifyIcon == notifyIconLeft)
                    GoToLeftDesktop();
                else
                    GoToRightDesktop();
            }
            else if (e.Button == MouseButtons.Right)
            {
                StartWithWindows = IsStartWithWindows();
                (contextMenuStrip.Items[3] as ToolStripMenuItem).Checked = StartWithWindows;
            }
        }

        private static void SetMouseWheelHook()
        {
            MouseHook mouseHook = new MouseHook();
            mouseHook.MouseWheel += new MouseHook.MouseHookCallback(MouseWheelScroll);
            mouseHook.Install();
        }

        private static void MouseWheelScroll(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            var lRect = NotifyIconHelper.GetIconRect(notifyIconLeft);
            var rRect = NotifyIconHelper.GetIconRect(notifyIconRight);

            if (InRect(mouseStruct.x, mouseStruct.y, lRect) || InRect(mouseStruct.x, mouseStruct.y, rRect))
                ScrollVirtualDesktop(mouseStruct.mouseData);
        }

        private static bool InRect(int x, int y, Rectangle rect)
            => x < rect.Right && x > rect.Left && y < rect.Bottom && y > rect.Top;

        private static void ScrollVirtualDesktop(int mouseData)
        {
            if (mouseData > 0)
                GoToLeftDesktop();
            else
                GoToRightDesktop();
        }

        private static ContextMenuStrip GetContext()
        {
            ContextMenuStrip CMS = new ContextMenuStrip();

            CMS.Items.Add("About", null, new EventHandler((o, e)
                => System.Diagnostics.Process.Start("https://github.com/hangacs/VirtualDesktopSwitcher")));
            CMS.Items.Add("Close Current Desktop", null, new EventHandler((o, e) => CloseCurrentDesktop()));
            CMS.Items.Add("Open New Desktop", null, new EventHandler((o, e) => OpenNewDesktop()));
            CMS.Items.Add(new ToolStripMenuItem("Start With Windows", null, (o, e) =>
                {
                    StartWithWindows = !StartWithWindows;
                    AutoStart(StartWithWindows);
                    (o as ToolStripMenuItem).Checked = StartWithWindows;
                })
            {
                Checked = StartWithWindows
            });
            CMS.Items.Add("Exit", null, new EventHandler((o, e) =>
            {
                notifyIconLeft.Dispose();
                notifyIconRight.Dispose();
                Application.Exit();
            }));

            return CMS;
        }

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
