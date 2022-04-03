/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.

/// Author: rvknth043 (https://github.com/rvknth043)
/// Project: https://github.com/rvknth043/Global-Low-Level-Key-Board-And-Mouse-Hook

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

/// <summary>
/// Class for intercepting low level Windows mouse hooks.
/// </summary>
class MouseHook
{
    /// <summary>
    /// Internal callback processing function
    /// </summary>
    private delegate IntPtr MouseHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
    private MouseHookHandler hookHandler;

    /// <summary>
    /// Function to be called when defined even occurs
    /// </summary>
    /// <param name="mouseStruct">MSLLHOOKSTRUCT mouse structure</param>
    public delegate void MouseHookCallback(MSLLHOOKSTRUCT mouseStruct);

    #region Events
    public event MouseHookCallback MouseWheel;
    #endregion

    /// <summary>
    /// Low level mouse hook's ID
    /// </summary>
    private IntPtr hookID = IntPtr.Zero;

    /// <summary>
    /// Install low level mouse hook
    /// </summary>
    /// <param name="mouseHookCallbackFunc">Callback function</param>
    public void Install()
    {
        hookHandler = HookFunc;
        hookID = SetHook(hookHandler);
    }

    /// <summary>
    /// Remove low level mouse hook
    /// </summary>
    public void Uninstall()
    {
        if (hookID == IntPtr.Zero)
            return;

        UnhookWindowsHookEx(hookID);
        hookID = IntPtr.Zero;
    }

    /// <summary>
    /// Destructor. Unhook current hook
    /// </summary>
    ~MouseHook()
    {
        Uninstall();
    }

    /// <summary>
    /// Sets hook and assigns its ID for tracking
    /// </summary>
    /// <param name="proc">Internal callback function</param>
    /// <returns>Hook ID</returns>
    private IntPtr SetHook(MouseHookHandler proc)
    {
        using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(module.ModuleName), 0);
    }

    /// <summary>
    /// Callback function
    /// </summary>
    private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // parse system messages
        if (nCode >= 0 && WM_MOUSEWHEEL == (int)wParam && MouseWheel != null)
        {
            MouseWheel((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT)));
        }

        return CallNextHookEx(hookID, nCode, wParam, lParam);
    }

    #region WinAPI
    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEWHEEL = 0x020A;

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public int x;
        public int y;
        public int mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        MouseHookHandler lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    #endregion
}