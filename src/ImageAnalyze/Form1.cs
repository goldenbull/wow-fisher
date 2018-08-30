using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageAnalyze
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click( object sender, EventArgs e )
		{
			// 路径和文件名
			const string rootPath = @"D:\pic\capture\3";
			string[] srcEdgeFiles = Directory.GetFiles( rootPath, "*边缘检测.bmp" );

			// 输出的表格
			StreamWriter report = new StreamWriter( rootPath + "\\calc.csv", false );

			Bitmap bmp0 = new Bitmap( srcEdgeFiles[0] );

			foreach( string fullname in srcEdgeFiles )
			{
				Trace.WriteLine( "处理" + fullname );
				int id = int.Parse( Path.GetFileName( fullname ).Split( '_' )[0] );
				Bitmap srcBMP = new Bitmap( fullname );

				// 自定义的几个指标，看是否存在简单的办法
				report.Write( id + "," );
				report.Write( GetIndex1( srcBMP ) + "," );
				report.Write( GetIndex2( srcBMP, bmp0 ) + "," );

				report.WriteLine();
			}

			report.Close();
		}

		/// <summary>
		/// 加权亮度之和，离中心近的权重高
		/// </summary>
		/// <param name="bmp"></param>
		/// <returns></returns>
		private double GetIndex1( Bitmap bmp )
		{
			BitmapData bmpData = bmp.LockBits( new Rectangle( new Point( 0 ), bmp.Size ), ImageLockMode.ReadOnly, bmp.PixelFormat );

			IntPtr ptr = bmpData.Scan0;

			int nStride = bmpData.Stride;
			int bytes = bmpData.Stride * bmp.Height;
			byte[] array = new byte[bytes];
			System.Runtime.InteropServices.Marshal.Copy( ptr, array, 0, bytes );

			bmp.UnlockBits( bmpData );

			// 计算像素按中心加权之和，越往中心的越重要
			double sum = 0;
			for( int x = 0; x < bmp.Width; x++ )
			{
				for( int y = 0; y < bmp.Height; y++ )
				{
					byte v = array[nStride * y + x];
					double distance =
						Math.Sqrt( ( x - bmp.Width / 2 ) * ( x - bmp.Width / 2 ) + ( y - bmp.Height / 2 ) * ( y - bmp.Height / 2 ) );
					sum += v / ( 1 + distance );
				}
			}
			return sum;
		}

		/// <summary>
		/// 简单亮度之和
		/// </summary>
		/// <param name="bmp"></param>
		/// <param name="bmp0"></param>
		/// <returns></returns>
		private double GetIndex2( Bitmap bmp, Bitmap bmp0 )
		{
			BitmapData bmpData = bmp.LockBits( new Rectangle( new Point( 0 ), bmp.Size ), ImageLockMode.ReadOnly, bmp.PixelFormat );

			IntPtr ptr = bmpData.Scan0;
			int nStride = bmpData.Stride;
			int bytes = bmpData.Stride * bmp.Height;
			byte[] array = new byte[bytes];
			System.Runtime.InteropServices.Marshal.Copy( ptr, array, 0, bytes );

			bmp.UnlockBits( bmpData );

			// 计算两幅图的差异
			double sum = 0;
			for( int x = 0; x < bmp.Width; x++ )
			{
				for( int y = 0; y < bmp.Height; y++ )
				{
					byte v = array[nStride * y + x];
					sum += v;
				}
			}
			return sum;
		}
	}
}