﻿using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace WindowsFilter
{
    /// <summary>
    /// List visible windows and send keyboard messages
    /// </summary>
    public class EnumWindows
    {
        /*
         * this is the class to enumberate all visible windows (main windows actually) and pass keyboard message to the selected ones. The main trick is to use the .Net the native code interface to WinAPI.
         */
        const int WM_KEYDOWN = 0x0100;

        const int VK_NEXT = 0x22;
        const int VK_PRIOR = 0x21;
        const int WM_SETFOCUS = 0x0007;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        /// <summary>
        /// Window handles (HWND) used for hWndInsertAfter
        /// </summary>
        public static class HWND
        {
            public static IntPtr
            NoTopMost = new IntPtr(-2),
            TopMost = new IntPtr(-1),
            Top = new IntPtr(0),
            Bottom = new IntPtr(1);
        }
        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }
        /// <summary>
        ///     Special window handles
        /// </summary>
        public enum SpecialWindowHandles
        {

            HWND_TOP = 0,

            HWND_BOTTOM = 1,

            HWND_TOPMOST = -1,

            HWND_NOTOPMOST = -2

        }

        [Flags]
        public enum SetWindowPosFlags : uint
        {

            SWP_ASYNCWINDOWPOS = 0x4000,

            SWP_DEFERERASE = 0x2000,

            SWP_DRAWFRAME = 0x0020,

            SWP_FRAMECHANGED = 0x0020,

            SWP_HIDEWINDOW = 0x0080,

            SWP_NOACTIVATE = 0x0010,

            SWP_NOCOPYBITS = 0x0100,

            SWP_NOMOVE = 0x0002,

            SWP_NOOWNERZORDER = 0x0200,

            SWP_NOREDRAW = 0x0008,

            SWP_NOREPOSITION = 0x0200,

            SWP_NOSENDCHANGING = 0x0400,

            SWP_NOSIZE = 0x0001,

            SWP_NOZORDER = 0x0004,

            SWP_SHOWWINDOW = 0x0040,

        }

        public abstract class WindowStyles
        {
            public const uint WS_OVERLAPPED = 0x00000000;
            public const uint WS_POPUP = 0x80000000;
            public const uint WS_CHILD = 0x40000000;
            public const uint WS_MINIMIZE = 0x20000000;
            public const uint WS_VISIBLE = 0x10000000;
            public const uint WS_DISABLED = 0x08000000;
            public const uint WS_CLIPSIBLINGS = 0x04000000;
            public const uint WS_CLIPCHILDREN = 0x02000000;
            public const uint WS_MAXIMIZE = 0x01000000;
            public const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
            public const uint WS_BORDER = 0x00800000;
            public const uint WS_DLGFRAME = 0x00400000;
            public const uint WS_VSCROLL = 0x00200000;
            public const uint WS_HSCROLL = 0x00100000;
            public const uint WS_SYSMENU = 0x00080000;
            public const uint WS_THICKFRAME = 0x00040000;
            public const uint WS_GROUP = 0x00020000;
            public const uint WS_TABSTOP = 0x00010000;

            public const uint WS_MINIMIZEBOX = 0x00020000;
            public const uint WS_MAXIMIZEBOX = 0x00010000;

            public const uint WS_TILED = WS_OVERLAPPED;
            public const uint WS_ICONIC = WS_MINIMIZE;
            public const uint WS_SIZEBOX = WS_THICKFRAME;
            public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

            // Common Window Styles

            public const uint WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);

            public const uint WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);

            public const uint WS_CHILDWINDOW = WS_CHILD;

            //Extended Window Styles

            public const uint WS_EX_DLGMODALFRAME = 0x00000001;
            public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
            public const uint WS_EX_TOPMOST = 0x00000008;
            public const uint WS_EX_ACCEPTFILES = 0x00000010;
            public const uint WS_EX_TRANSPARENT = 0x00000020;

            //#if(WINVER >= 0x0400)
            public const uint WS_EX_MDICHILD = 0x00000040;
            public const uint WS_EX_TOOLWINDOW = 0x00000080;
            public const uint WS_EX_WINDOWEDGE = 0x00000100;
            public const uint WS_EX_CLIENTEDGE = 0x00000200;
            public const uint WS_EX_CONTEXTHELP = 0x00000400;

            public const uint WS_EX_RIGHT = 0x00001000;
            public const uint WS_EX_LEFT = 0x00000000;
            public const uint WS_EX_RTLREADING = 0x00002000;
            public const uint WS_EX_LTRREADING = 0x00000000;
            public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
            public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;

            public const uint WS_EX_CONTROLPARENT = 0x00010000;
            public const uint WS_EX_STATICEDGE = 0x00020000;
            public const uint WS_EX_APPWINDOW = 0x00040000;

            public const uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
            public const uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
            //#endif /* WINVER >= 0x0400 */

            //#if(_WIN32_WINNT >= 0x0500)
            public const uint WS_EX_LAYERED = 0x00080000;
            //#endif /* _WIN32_WINNT >= 0x0500 */

            //#if(WINVER >= 0x0500)
            public const uint WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
            public const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
                                                            //#endif /* WINVER >= 0x0500 */

            //#if(_WIN32_WINNT >= 0x0500)
            public const uint WS_EX_COMPOSITED = 0x02000000;
            public const uint WS_EX_NOACTIVATE = 0x08000000;
            //#endif /* _WIN32_WINNT >= 0x0500 */
        }
        enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }


        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop,
        EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetWindowText(IntPtr hWnd,
        System.Text.StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "EnumWindows")]
        private static extern bool _enumWindows(EnumDelegate callback, IntPtr extraData);


        List<Process> processList = new List<Process>();


        /// <summary>
        /// find all visible windows process and store them to processlist 
        /// </summary>
        /// <param name="is_clean"></param>
        public void getProcess(Boolean is_clean = false)
        {

            IntPtr current_handle = Process.GetCurrentProcess().MainWindowHandle;
            Process[] process_all = Process.GetProcesses();
            if (is_clean)
                processList = new List<Process>();
            foreach (Process process in process_all)
            {
                if (!System.String.IsNullOrEmpty(process.MainWindowTitle) && current_handle != process.MainWindowHandle
                    && !processList.Exists(p => p.MainWindowTitle == process.MainWindowTitle)
                    )

                {
                    processList.Add(process);
                }
            }
            processList.Sort((a, b) => a.Id - b.Id);

        }

        /// <summary>
        /// return the number of windows
        /// </summary>
        /// <returns></returns>
        public int getSize()
        {
            return processList.Count;
        }

        public List<string> getContents()
        {

            List<string> s = new List<string>();
            foreach (Process process in processList)
            {
                try
                {
                    s.Add(string.Format("{0}:{1}", process.Id, process.MainWindowTitle));
                }
                catch
                {

                }

            }
            return s;
        }
        public IntPtr getHandle(int index)
        {
            return processList[index].MainWindowHandle;

        }
        public void setToFront(IntPtr handle)
        {
            SetForegroundWindow(handle);
        }
        public void sendKeyPress(int index, KeyEventArgs e)
        {
            IntPtr handle = getHandle(index);
            PostMessage(handle, WM_SETFOCUS, (IntPtr)0, (IntPtr)0);
            SetFocus(handle);
            PostMessage(handle, WM_KEYDOWN, (IntPtr)e.KeyCode, (IntPtr)0);
        }
        public void sendMessage(int index, string s)
        {
            IntPtr handle = getHandle(index);
            PostMessage(handle, WM_SETFOCUS, (IntPtr)0, (IntPtr)0);
            SetFocus(handle);
            switch (s)
            {
                case "next":
                    PostMessage(handle, WM_KEYDOWN, (IntPtr)VK_NEXT, (IntPtr)0);
                    break;
                case "prior":
                    PostMessage(handle, WM_KEYDOWN, (IntPtr)VK_PRIOR, (IntPtr)0);
                    break;

            }


        }
        /// <summary>
        /// set the window at given index full screen. 
        /// the screen dx and dy is use zoom in or out the woring Area
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="screen_dx">scale to desktop in x</param>
        /// <param name="screen_dy">scale to desktop in y</param>
        public void setFullscreen(int index, int screen_dx, int screen_dy, int offset_dx=0,int offset_dy=0)
        {
            IntPtr handle = getHandle(index);

            int style = GetWindowLong(handle, (int)WindowLongFlags.GWL_STYLE);
            SetWindowLong(handle, (int)WindowLongFlags.GWL_STYLE, (style & ~((int)WindowStyles.WS_CAPTION) | (int)WindowStyles.WS_THICKFRAME));
            var screen = Screen.AllScreens[0];
            SetWindowPos(handle, (IntPtr)SpecialWindowHandles.HWND_BOTTOM,
                screen.WorkingArea.Left - screen_dx+offset_dx,
                screen.WorkingArea.Top - screen_dy+offset_dy,
                 screen.WorkingArea.Width + 2 * screen_dx,
                 screen.WorkingArea.Height + 2 * screen_dy,
             SetWindowPosFlags.SWP_DRAWFRAME);

        }
    }

}

