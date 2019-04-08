using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace VitrualDesktopSwitcher
{
    
    static class Program
    {
        [STAThread]
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

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

        static NotifyIcon notifyIconLeft = new NotifyIcon();
        static NotifyIcon notifyIconRight = new NotifyIcon();

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Left Notify Icon
            notifyIconLeft.Text="Left";
            notifyIconLeft.ContextMenuStrip = GetContext();
            notifyIconLeft.Icon = new Icon("Left.ico");

            //Right Notify Icon
            notifyIconRight.Text = "Right";
            notifyIconRight.ContextMenuStrip = GetContext();
            notifyIconRight.Icon = new Icon("Right.ico");

            notifyIconLeft.Visible = true;
            notifyIconRight.Visible = true;

            notifyIconLeft.MouseClick += LeftClickHandler;
            notifyIconRight.MouseClick += RightClickHandler;

            Application.Run();
        }

        //Contex Menu
        private static ContextMenuStrip GetContext()
        {
            ContextMenuStrip CMS = new ContextMenuStrip();
            CMS.Items.Add("About", null, new EventHandler(About_Program));
            CMS.Items.Add("Close Current Desktop", null, new EventHandler(Close_Current_Desktop));
            CMS.Items.Add("Open New Desktop", null, new EventHandler(Open_New_Desktop));
            CMS.Items.Add("Exit",null,new EventHandler(Exit_Click));
            
            return CMS;
        }
        private static void About_Program(object sender,EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/hangacs/VirtualDesktopSwitcher");
        }
        
        private static void Close_Current_Desktop(object sender, EventArgs e)
        {
            CloseCurentDesktop();
        }
        private static void Open_New_Desktop(object sender, EventArgs e)
        {
            OpenNewDesktop();
        }

        
        private static void Exit_Click(object sender, EventArgs e)
        {
            notifyIconLeft.Dispose();
            notifyIconRight.Dispose();
            Application.Exit();
        }
        //Switch Virtual Desktop
        private static void LeftClickHandler(object sender,MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GoLeftDesktop();
            }
        }
        private static void RightClickHandler(object sender,MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GoRightDesktop();
            }
        }


        public static void OpenNewDesktop()
        {
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(KEY_D, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(KEY_D, 0, KEYEVENTF_KEYUP, 0);
        }
        public static void CloseCurentDesktop()
        {
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_F4, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_F4, 0, KEYEVENTF_KEYUP, 0);
        }
        public static void GoLeftDesktop()
        {
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_LEFT, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LEFT, 0, KEYEVENTF_KEYUP, 0);
        }
        public static void GoRightDesktop()
        {
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_RIGHT, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_WIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_RIGHT, 0, KEYEVENTF_KEYUP, 0);
        }
        
    }
}
