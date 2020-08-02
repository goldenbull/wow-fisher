using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FisherUI
{
    /// <summary>
    /// windows API封装
    /// </summary>
    static class Win32
    {
        #region 窗口相关的操作

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        public static void FocusToGameWnd(IntPtr hWnd)
        {
            BringWindowToTop(hWnd);
            SetFocus(hWnd);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// 返回窗口在屏幕上的位置
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetRect(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out var r);
            return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        #endregion

        #region 鼠标工具

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        /// <summary>
        /// 单击左键或右键
        /// </summary>
        /// <param name="button"></param>
        public static void ClickScreen(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                mouse_event((uint) MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                Thread.Sleep(200);
                mouse_event((uint) MouseEventFlags.LEFTUP, 0, 0, 0, 0);
            }
            else if (button == MouseButtons.Right)
            {
                mouse_event((uint) MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
                Thread.Sleep(200);
                mouse_event((uint) MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
            }
        }

        #endregion

        #region 光标工具

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public Int32 x;
            public Int32 y;

            public override string ToString()
            {
                return $"({x}, {y})";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            // Specifies the size, in bytes, of the structure. 
            // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public Int32 cbSize;

            // Specifies the cursor state. This parameter can be one of the following values:
            //    0             The cursor is hidden.
            //    1    The cursor is showing.
            public Int32 flags;

            public IntPtr hCursor; // Handle to the cursor. 
            public POINT ptScreenPos; // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        public static long GetCursorID()
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out pci);
            return pci.hCursor.ToInt64();
        }

        public static void MoveCursorTo(Point pt)
        {
            Cursor.Position = pt;
            Thread.Sleep(50);
        }

        /// <summary>
        /// 移动一个偏移量
        /// </summary>
        public static void MoveCursorDelta(int dx, int dy)
        {
            var pt = Cursor.Position;
            pt.X += dx;
            pt.Y += dy;
            MoveCursorTo(pt);
        }

        #endregion
    }
}