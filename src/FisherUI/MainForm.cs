using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace FisherUI
{
    public partial class MainForm : Form
    {
        // wow主窗口
        private IntPtr m_hWnd;

        // 独立线程后台钓鱼
        private bool m_bRunning = false;
        private Thread m_hWorker = null;

        // 鱼漂的截图相关数据
        private const int MaxCaptureSize = 80;
        private Bitmap m_bmpForBait;
        private Graphics m_graphics;

        // 抽边工具
        private readonly SobelEdgeDetector sobel = new SobelEdgeDetector();

        // 判据的阈值
        private int m_num阈值 = 12;

        private void num阈值_ValueChanged(object sender, EventArgs e)
        {
            m_num阈值 = (int) num阈值.Value;
        }

        public MainForm()
        {
            InitializeComponent();
            this.picBox截屏.Size = new Size(MaxCaptureSize, MaxCaptureSize);
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            ToggleStatus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bRunning)
                ToggleStatus();
        }

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

        /// <summary>
        /// 定位游戏窗口
        /// </summary>
        private bool InitWnd()
        {
            m_hWnd = Win32.FindWindow(null, "魔兽世界");
            //m_hWnd = FindWindow( null, "魔獸世界" );
            if (m_hWnd != IntPtr.Zero)
            {
                ShowStatus("快切换到游戏窗口！");
                Win32.FocusToGameWnd(m_hWnd);
                Thread.Sleep(5000);
            }

            return m_hWnd != IntPtr.Zero;
        }

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
                if (!MoveCursorToFish())
                {
                    ShowStatus("没找到鱼漂");
                    continue;
                }

                // 等待上钩事件，包括了右键点击收杆
                WaitForFish();
            }
        }

        /// <summary>
        /// invoke方式输出debug信息
        /// </summary>
        private void ShowDebugInfo(Bitmap img, double eval)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<Bitmap, double>(ShowDebugInfo), img, eval);
            }
            else
            {
                picBox截屏.Image = img;
                tb判据.Text = eval.ToString("P4");
                int nValue = (int) (eval * 100) + (prgsBar判据.Maximum - prgsBar判据.Minimum) / 2;
                if (nValue > prgsBar判据.Maximum) nValue = prgsBar判据.Maximum;
                if (nValue < prgsBar判据.Minimum) nValue = prgsBar判据.Minimum;
                prgsBar判据.Value = nValue;
            }
        }

        /// <summary>
        /// 显示当前状态
        /// </summary>
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

        /// <summary>
        /// 截图并抽边
        /// </summary>
        public Bitmap CaptureScreen()
        {
            m_graphics.CopyFromScreen(Cursor.Position.X - m_bmpForBait.Width / 2,
                                      Cursor.Position.Y - m_bmpForBait.Height / 2,
                                      0, 0,
                                      m_bmpForBait.Size);

            var grayscaleBMP = Grayscale.CommonAlgorithms.RMY.Apply(m_bmpForBait);
            sobel.ApplyInPlace(grayscaleBMP);
            return grayscaleBMP;
        }

        /// <summary>
        /// 开始用图像识别的办法等待上钩事件，包括了右键点击收杆
        /// </summary>
        private void WaitForFish()
        {
            ShowStatus("等待鱼儿上钩……");

            // 截获第一幅图作为标准
            var img0 = CaptureScreen();

            // 不停的截图，比较指标差异，超过时间则退出
            while (DateTime.Now - m_下钩时刻 < new TimeSpan(0, 0, 25))
            {
                var img = CaptureScreen();
                var diff = CalcImgDiff(img0, img);

                // debug输出
                ShowDebugInfo(img, diff);

                // 阈值判断，特效全开时根据水花判断，特效全关时根据负的亮度变化判断，不知道哪个更合适……
                if (diff > m_num阈值 / 100.0)
                {
                    ShowStatus("上钩了！");
                    Win32.ClickScreen(MouseButtons.Right);
                    Thread.Sleep(3000);
                    return;
                }
            }
        }

        static byte[] GetImageBytes(Bitmap img)
        {
            var bmpData = img.LockBits(new Rectangle(new Point(0), img.Size),
                                       ImageLockMode.ReadOnly,
                                       img.PixelFormat);
            var ptr = bmpData.Scan0;
            var bytes = bmpData.Stride * img.Height;
            var array = new byte[bytes];
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
                    double distance = Math.Sqrt((x - img.Width / 2) * (x - img.Width / 2) +
                                                (y - img.Height / 2) * (y - img.Height / 2));
                    sum += diff / (1 + distance);
                    //sum += v;
                }
            }

            return sum / arr.Length / 10; // 归一化
        }

        /// <summary>
        /// 定位鼠标
        /// </summary>
        private bool MoveCursorToFish()
        {
            ShowStatus("定位鱼浮位置……");

            var rectWnd = Win32.GetRect(m_hWnd);

            // 首先定位到肯定不会有鱼漂的位置
            Win32.MoveCursorTo(new Point(rectWnd.Left + 100, rectWnd.Top + 100));
            Thread.Sleep(100);
            // 获得鼠标ID，用于判断是否找到了鱼钩
            var curID = Win32.GetCursorID();

            // 从屏幕中间往下搜索1/8高度，直到光标改变或超出搜索范围
            var bFound = false;
            var searchRange = new Rectangle(rectWnd.Left + 25,
                                            rectWnd.Top + rectWnd.Height / 2,
                                            rectWnd.Width - 50,
                                            rectWnd.Height / 8);
            for (int y = searchRange.Top; y < searchRange.Bottom; y += 30)
            {
                for (int x = searchRange.Left; x < searchRange.Right; x += 15)
                {
                    Win32.MoveCursorTo(new Point(x, y));
                    if (Win32.GetCursorID() != curID)
                    {
                        bFound = true;
                        goto Label;
                    }
                }
            }

            Label:
            if (!bFound)
            {
                Win32.MoveCursorTo(searchRange.Location);
                return false;
            }

            // 细节定位
            var baitPt = Cursor.Position;
            var rectBait = new Rectangle();

            // 向左微调
            curID = Win32.GetCursorID();
            do
            {
                Win32.MoveCursorDelta(-3, 0);
                if (Cursor.Position.X <= baitPt.X - MaxCaptureSize)
                    return false;
                rectBait.X = Cursor.Position.X + 1;
            } while (Win32.GetCursorID() == curID);

            // 向右微调
            Win32.MoveCursorTo(baitPt);
            curID = Win32.GetCursorID();
            do
            {
                Win32.MoveCursorDelta(3, 0);
                if (Cursor.Position.X >= baitPt.X + MaxCaptureSize)
                    return false;
                rectBait.Width = Cursor.Position.X - rectBait.Left;
            } while (Win32.GetCursorID() == curID);

            // 移到中间，再上下微调
            baitPt.X = rectBait.Left + rectBait.Width / 2;

            // 上下微调
            Win32.MoveCursorTo(baitPt);
            curID = Win32.GetCursorID();
            do
            {
                Win32.MoveCursorDelta(0, -3);
                if (Cursor.Position.Y <= baitPt.Y - MaxCaptureSize)
                    return false;
                rectBait.Y = Cursor.Position.Y;
            } while (Win32.GetCursorID() == curID);

            Win32.MoveCursorTo(baitPt);
            curID = Win32.GetCursorID();
            do
            {
                Win32.MoveCursorDelta(0, 3);
                if (Cursor.Position.Y >= baitPt.Y + MaxCaptureSize)
                    return false;
                rectBait.Height = Cursor.Position.Y - rectBait.Top;
            } while (Win32.GetCursorID() == curID);

            // 得到了鱼漂的rect，光标定位到中间位置
            Win32.MoveCursorTo(rectBait.Location + new Size(rectBait.Width / 2, rectBait.Height / 2));

            // 创建截屏所需的graphics和image
            if (m_graphics != null)
            {
                m_graphics.Dispose();
                m_bmpForBait.Dispose();
            }

            m_bmpForBait = new Bitmap(rectBait.Width, rectBait.Height, PixelFormat.Format24bppRgb);
            m_graphics = Graphics.FromImage(m_bmpForBait);
            return true;
        }

        private DateTime m_上次鱼饵buff = DateTime.Now.AddDays(-1);
        private DateTime m_下钩时刻;
        private static readonly TimeSpan 鱼饵周期 = new TimeSpan(0, 10, 0);

        /// <summary>
        /// 准备钓鱼，包括检查鱼饵、下钩
        /// </summary>
        private void InitForOneFish()
        {
            /*
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
            */

            // 下钩，快捷键1
            ShowStatus("下钩");
            SendKeys.SendWait("1");
            Thread.Sleep(1000);
            m_下钩时刻 = DateTime.Now;
        }

        #endregion
    }
}