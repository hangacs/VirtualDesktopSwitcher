using System;
using System.Windows.Forms;
using Microsoft.Win32;
using static VirtualDesktopSwitcher.KeyboardEventHelper;
using Linearstar.Windows.RawInput;
using System.Linq;
using Linearstar.Windows.RawInput.Native;

namespace VirtualDesktopSwitcher
{
    static class Program
    {
        private static NotifyIcon notifyIconLeft;
        private static NotifyIcon notifyIconRight;

        private static RawInputReceiverWindow reciever;

        private static bool StartWithWindows;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += Clean;

            CreateNotifyIcon();
            RegisterRawInput();

            Application.Run();
        }

        private static void Clean(object sender, EventArgs e)
        {
            RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
        }

        static void RegisterRawInput()
        {
            reciever = new RawInputReceiverWindow();

            var devices = RawInputDevice.GetDevices();
            var mouse = devices.OfType<RawInputMouse>();

            reciever.Input += (sender, e) => MouseWheelScroll(e.Data);

            try
            {
                RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.ExInputSink, reciever.Handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to enable scroll function. {ex.Message}");
            }
        }

        private static void CreateNotifyIcon()
        {
            StartWithWindows = IsStartWithWindows();
            var contextMenuStrip = CreateContextMenuStrip();

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

        private static void MouseWheelScroll(RawInputData data)
        {
            var lRect = NotifyIconHelper.GetIconRect(notifyIconLeft);
            var rRect = NotifyIconHelper.GetIconRect(notifyIconRight);

            if (NotifyIconHelper.InRect(Cursor.Position, lRect) || NotifyIconHelper.InRect(Cursor.Position, rRect))
                ScrollVirtualDesktop((data as RawInputMouseData).Mouse);
        }

        private static void ScrollVirtualDesktop(RawMouse mouse)
        {
            if (!mouse.Buttons.HasFlag(RawMouseButtonFlags.MouseWheel)) return;

            if (mouse.ButtonData > 0)
                GoToLeftDesktop();
            else if (mouse.ButtonData < 0)
                GoToRightDesktop();
        }

        private static ContextMenuStrip CreateContextMenuStrip()
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
                MessageBox.Show($"Failed to get registry key. {ex.Message}", "Error");
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
