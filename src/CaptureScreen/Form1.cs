using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace CaptureScreen
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click( object sender, EventArgs e )
		{
			ToggleStatus();
		}

		private bool m_bRunning = false;
		private Thread m_hWorker = null;

		private void ToggleStatus()
		{
			if( m_bRunning )
			{
				m_bRunning = false;
				m_hWorker.Join();
				button1.Text = "Start";
			}
			else
			{
				m_bRunning = true;
				button1.Text = "Stop";
				m_hWorker = new Thread( WorkerProc );
				m_hWorker.Start();
			}
		}

		private void WorkerProc()
		{
			// 源图片
			Bitmap srcBMP = new Bitmap( 128, 128, PixelFormat.Format24bppRgb );
			Graphics g = Graphics.FromImage( srcBMP );
			int ncount = 0;

			// 路径
			DirectoryInfo folder =
				Directory.CreateDirectory( Path.Combine( "C:\\capture", DateTime.Now.ToString( "yyyyMMdd_HHmmss" ) ) );

			// 图像变换
			SobelEdgeDetector sobel = new SobelEdgeDetector();

			while( m_bRunning )
			{
				ncount++;
				g.CopyFromScreen( Cursor.Position.X - srcBMP.Width / 2, Cursor.Position.Y - srcBMP.Height / 2, 0, 0, srcBMP.Size );

				// 文件名统一前缀
				string prefix = Path.Combine( folder.FullName, ncount.ToString( "000000" ) );

				// 分别存盘
				srcBMP.Save( prefix + ".bmp" );

				Bitmap grayscaleBMP = Grayscale.CommonAlgorithms.RMY.Apply( srcBMP );
				grayscaleBMP.Save( prefix + "_0灰度.bmp" );

				Bitmap edgeBMP = sobel.Apply( grayscaleBMP );
				edgeBMP.Save( prefix + "_1边缘检测.bmp" );
			}
			g.Dispose();
		}

		private void Form1_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( m_bRunning )
			{
				ToggleStatus();
			}
		}
	}
}