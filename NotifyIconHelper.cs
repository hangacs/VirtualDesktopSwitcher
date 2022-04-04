using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// https://stackoverflow.com/a/26695961
/// </summary>
sealed class NotifyIconHelper
{
    public static Rectangle GetIconRect(NotifyIcon icon)
    {
        NOTIFYICONIDENTIFIER notifyIcon = new NOTIFYICONIDENTIFIER();

        notifyIcon.cbSize = Marshal.SizeOf(notifyIcon);
        //use hWnd and id of NotifyIcon instead of guid is needed
        notifyIcon.hWnd = GetHandle(icon);
        notifyIcon.uID = GetId(icon);

        int hresult = Shell_NotifyIconGetRect(ref notifyIcon, out var rect);
        //rect now has the position and size of icon

        return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NOTIFYICONIDENTIFIER
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public Guid guidItem;
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int Shell_NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier, [Out] out RECT iconLocation);

    private static FieldInfo windowField = typeof(NotifyIcon).GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
    private static IntPtr GetHandle(NotifyIcon icon)
    {
        if (windowField == null) throw new InvalidOperationException("[Useful error message]");
        NativeWindow window = windowField.GetValue(icon) as NativeWindow;

        if (window == null) throw new InvalidOperationException("[Useful error message]");  // should not happen?
        return window.Handle;
    }

    private static FieldInfo idField = typeof(NotifyIcon).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
    private static int GetId(NotifyIcon icon)
    {
        if (idField == null) throw new InvalidOperationException("[Useful error message]");
        return (int)idField.GetValue(icon);
    }

    public static bool InRect(Point point, Rectangle rect)
        => point.X < rect.Right && point.X > rect.Left && point.Y < rect.Bottom && point.Y > rect.Top;
}
