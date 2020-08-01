using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace FisherUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.picBox截屏.Size = new Size(CaptureSize, CaptureSize);
        }

        /// <summary>
        /// 用户唯一的控制入口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAction_Click(object sender, EventArgs e)
        {
            ToggleStatus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bRunning)
            {
                ToggleStatus();
            }
        }

        #region 系统启动和停止

        private bool m_bRunning = false;
        private Thread m_hWorker = null;

        /// <summary>
        /// 切换状态
        /// </summary>
        private void ToggleStatus()
        {
            if (m_bRunning)
            {
                m_bRunning = false;
                m_hWorker.Join();
                btnAction.Text = "Start";
                ShowStatus("钓鱼已停止");
            }
            else
            {
                m_bRunning = true;
                btnAction.Text = "Stop";
                m_hWorker = new Thread(WorkerProc);
                m_hWorker.Start();
                ShowStatus("自动钓鱼中");
            }
        }

        #endregion

        #region 窗口和鼠标相关的操作

        private IntPtr m_hWnd;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetFocus(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        public void FocusToGameWnd()
        {
            BringWindowToTop(m_hWnd);
            SetFocus(m_hWnd);
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
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// 返回窗口在屏幕上的位置
        /// </summary>
        /// <returns></returns>
        public Rectangle GetRect()
        {
            RECT r;
            GetWindowRect(m_hWnd, out r);
            return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        /// <summary>
        /// 定位游戏窗口
        /// </summary>
        /// <returns></returns>
        private bool InitWnd()
        {
            m_hWnd = FindWindow(null, "魔兽世界");
            //m_hWnd = FindWindow( null, "魔獸世界" );
            if (m_hWnd != IntPtr.Zero)
            {
                ShowStatus("快切换到游戏窗口！");
                FocusToGameWnd();
                Thread.Sleep(5000);
            }

            m_bmpForCapture = new Bitmap(CaptureSize, CaptureSize, PixelFormat.Format24bppRgb);
            m_graphics = Graphics.FromImage(m_bmpForCapture);

            return m_hWnd != IntPtr.Zero;
        }

        #endregion

        #region 鼠标工具

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

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
                return string.Format("({0}, {1})", x, y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CURSORINFO
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
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        /// <summary>
        /// 获得鼠标ID，用于判断是否找到了鱼钩
        /// </summary>
        /// <returns></returns>
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

        // 移动一个偏移量
        public static void MoveCursorDelta(int dx, int dy)
        {
            Point pt = Cursor.Position;
            pt.X += dx;
            pt.Y += dy;
            MoveCursorTo(pt);
        }

        #endregion

        #region 图像处理相关

        private const int CaptureSize = 48;
        private Bitmap m_bmpForCapture;
        private Graphics m_graphics;
        private readonly SobelEdgeDetector sobel = new SobelEdgeDetector();

        /// <summary>
        /// 截图，抽边
        /// </summary>
        /// <returns></returns>
        private Bitmap CaptureScreen()
        {
            // 截图
            m_graphics.CopyFromScreen(Cursor.Position.X - m_bmpForCapture.Width / 2,
                Cursor.Position.Y - m_bmpForCapture.Height / 2,
                0, 0,
                m_bmpForCapture.Size);

            Bitmap grayscaleBMP = Grayscale.CommonAlgorithms.RMY.Apply(m_bmpForCapture);
            sobel.ApplyInPlace(grayscaleBMP);
            return grayscaleBMP;
        }

        #endregion

        #region 实际钓鱼操作

        /// <summary>
        /// 工作线程
        /// </summary>
        private void WorkerProc()
        {
            if (!InitWnd())
            {
                MessageBox.Show("没找到游戏窗口");
                return;
            }

            // 开始正式钓鱼！
            while (m_bRunning)
            {
                // 进行准备工作，包括上鱼饵、下钩等操作
                InitForOneFish();

                // 鼠标移动到鱼钩的位置
                MoveCursorToFish();

                // 等待上钩事件，包括了右键点击收杆
                WaitForFish();
            }
        }

        /// <summary>
        /// invoke方式输出debug信息
        /// </summary>
        /// <param name="img"></param>
        /// <param name="idx"></param>
        private void ShowDebugInfo(Bitmap img, double idx)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<Bitmap, double>(ShowDebugInfo), img, idx);
            }
            else
            {
                picBox截屏.Image = img;
                tb判据.Text = idx.ToString("P4");

                int nValue = (int) (idx * 100) + (prgsBar判据.Maximum - prgsBar判据.Minimum) / 2;
                if (nValue > prgsBar判据.Maximum)
                {
                    nValue = prgsBar判据.Maximum;
                }

                if (nValue < prgsBar判据.Minimum)
                {
                    nValue = prgsBar判据.Minimum;
                }

                prgsBar判据.Value = nValue;
            }
        }

        /// <summary>
        /// 显示当前状态
        /// </summary>
        /// <param name="msg"></param>
        private void ShowStatus(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(ShowStatus), msg);
            }
            else
            {
                tbStatus.Text = msg;
            }
        }

        private int m_num阈值 = 12;

        private void num阈值_ValueChanged(object sender, EventArgs e)
        {
            m_num阈值 = (int) num阈值.Value;
        }

        /// <summary>
        /// 开始用图像识别的办法等待上钩事件，包括了右键点击收杆
        /// </summary>
        private void WaitForFish()
        {
            ShowStatus("等待鱼儿上钩……");

            // 截获第一幅图作为标准
            Bitmap img0 = CaptureScreen();

            // 不停的截图，比较指标差异，超过时间则退出
            while (DateTime.Now - m_下钩时刻 < new TimeSpan(0, 0, 25))
            {
                Bitmap img = CaptureScreen();
                double diff = CalcImgDiff(img0, img);

                // debug输出
                ShowDebugInfo(img, diff);

                // 阈值判断，特效全开时根据水花判断，特效全关时根据负的亮度变化判断，不知道哪个更合适……
                if (diff > m_num阈值 / 100.0)
                {
                    ShowStatus("上钩了！");
                    ClickScreen(MouseButtons.Right);
                    Thread.Sleep(3000);
                    return;
                }
            }
        }

        static byte[] GetImageBytes(Bitmap img)
        {
            BitmapData bmpData = img.LockBits(new Rectangle(new Point(0), img.Size), ImageLockMode.ReadOnly, img.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * img.Height;
            byte[] array = new byte[bytes];
            Marshal.Copy(ptr, array, 0, bytes);
            img.UnlockBits(bmpData);
            return array;
        }
        /// <summary>
        /// 用亮度做指标，计算两幅图片的差异程度
        /// </summary>
        private static double CalcImgDiff(Bitmap img0, Bitmap img)
        {
            var arr0 = GetImageBytes(img0);
            var arr = GetImageBytes(img);

            // 计算各像素的差异的绝对值按中心加权之和，越往中心的越重要
            double sum = 0;
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    var diff = Math.Abs(arr[img0.Width * y + x] - arr0[img0.Width * y + x]);
                    double distance =
                        Math.Sqrt((x - img.Width / 2) * (x - img.Width / 2) +
                                  (y - img.Height / 2) * (y - img.Height / 2));
                    sum += diff / (1 + distance);
                    //sum += v;
                }
            }

            return sum/arr.Length/10; // 归一化
        }

        /// <summary>
        /// 定位鼠标
        /// </summary>
        private void MoveCursorToFish()
        {
            ShowStatus("定位鱼浮位置……");

            Rectangle rectWnd = GetRect();

            // 首先定位到肯定不会有鱼漂的位置
            MoveCursorTo(new Point(rectWnd.Left + 100, rectWnd.Top + 100));
            Thread.Sleep(100);
            long curID = GetCursorID();

            // 从屏幕中间往下搜索1/8高度，直到光标改变或超出搜索范围
            bool bFound = false;
            Rectangle searchRange = new Rectangle(rectWnd.Left + 25,
                rectWnd.Top + rectWnd.Height / 2,
                rectWnd.Width - 50,
                rectWnd.Height / 8);
            for (int y = searchRange.Top; y < searchRange.Bottom; y += 30)
            {
                for (int x = searchRange.Left; x < searchRange.Right; x += 15)
                {
                    MoveCursorTo(new Point(x, y));
                    if (GetCursorID() != curID)
                    {
                        bFound = true;
                        goto Label;
                    }
                }
            }

            Label:
            if (!bFound)
            {
                MoveCursorTo(searchRange.Location);
                return;
            }

            // 细节定位
            Point baitPt = Cursor.Position;
            Rectangle rectBait = new Rectangle();

            // 向左微调
            curID = GetCursorID();
            do
            {
                MoveCursorDelta(-3, 0);
                if (Cursor.Position.X <= baitPt.X - CaptureSize)
                {
                    return;
                }
            } while (GetCursorID() == curID);

            rectBait.X = Cursor.Position.X + 1;

            // 向右微调
            MoveCursorTo(baitPt);
            curID = GetCursorID();
            do
            {
                MoveCursorDelta(3, 0);
                if (Cursor.Position.X >= baitPt.X + CaptureSize)
                {
                    return;
                }
            } while (GetCursorID() == curID);

            rectBait.Width = Cursor.Position.X - rectBait.Left;

            // 移到中间，再上下微调
            baitPt.X = rectBait.Left + rectBait.Width / 2;

            // 上下微调
            MoveCursorTo(baitPt);
            curID = GetCursorID();
            do
            {
                MoveCursorDelta(0, -3);
                if (Cursor.Position.Y <= baitPt.Y - CaptureSize)
                {
                    return;
                }
            } while (GetCursorID() == curID);

            rectBait.Y = Cursor.Position.Y;

            MoveCursorTo(baitPt);
            curID = GetCursorID();
            do
            {
                MoveCursorDelta(0, 3);
                if (Cursor.Position.Y >= baitPt.Y + CaptureSize)
                {
                    return;
                }
            } while (GetCursorID() == curID);

            rectBait.Height = Cursor.Position.Y - rectBait.Top;

            // 定位到中间位置
            MoveCursorTo(rectBait.Location + new Size(rectBait.Width / 2, rectBait.Height / 2));
        }

        private DateTime m_上次鱼饵buff = DateTime.Now.AddDays(-1);
        private DateTime m_下钩时刻;
        private static readonly TimeSpan 鱼饵周期 = new TimeSpan(0, 10, 0);

        /// <summary>
        /// 准备钓鱼，包括检查鱼饵、下钩
        /// </summary>
        private void InitForOneFish()
        {
            // 更新鱼饵
            if (DateTime.Now - m_上次鱼饵buff >= 鱼饵周期)
            {
                // 快捷键2使用鱼饵
                ShowStatus("准备鱼饵buff……");
                SendKeys.SendWait("2");
                Thread.Sleep(7000);
                m_上次鱼饵buff = DateTime.Now;

                // 快捷键3摧毁不用的物品
                SendKeys.SendWait("3");
                Thread.Sleep(5000);
            }

            // 下钩，快捷键1
            ShowStatus("下钩");
            SendKeys.SendWait("1");
            Thread.Sleep(1500);
            m_下钩时刻 = DateTime.Now;
        }

        #endregion
    }
}