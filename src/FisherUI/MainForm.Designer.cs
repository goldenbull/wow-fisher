namespace FisherUI
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnAction = new System.Windows.Forms.Button();
            this.prgsBar判据 = new System.Windows.Forms.ProgressBar();
            this.tb判据 = new System.Windows.Forms.TextBox();
            this.picBox截屏 = new System.Windows.Forms.PictureBox();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.num阈值 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBox截屏)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num阈值)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAction
            // 
            this.btnAction.Location = new System.Drawing.Point(12, 12);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(131, 96);
            this.btnAction.TabIndex = 0;
            this.btnAction.Text = "开始";
            this.btnAction.UseVisualStyleBackColor = true;
            this.btnAction.Click += new System.EventHandler(this.btnAction_Click);
            // 
            // prgsBar判据
            // 
            this.prgsBar判据.Location = new System.Drawing.Point(12, 190);
            this.prgsBar判据.Maximum = 80;
            this.prgsBar判据.Name = "prgsBar判据";
            this.prgsBar判据.Size = new System.Drawing.Size(268, 23);
            this.prgsBar判据.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgsBar判据.TabIndex = 1;
            this.prgsBar判据.Value = 60;
            // 
            // tb判据
            // 
            this.tb判据.Location = new System.Drawing.Point(12, 163);
            this.tb判据.Name = "tb判据";
            this.tb判据.ReadOnly = true;
            this.tb判据.Size = new System.Drawing.Size(64, 21);
            this.tb判据.TabIndex = 2;
            // 
            // picBox截屏
            // 
            this.picBox截屏.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBox截屏.Location = new System.Drawing.Point(163, 12);
            this.picBox截屏.Name = "picBox截屏";
            this.picBox截屏.Size = new System.Drawing.Size(96, 96);
            this.picBox截屏.TabIndex = 3;
            this.picBox截屏.TabStop = false;
            // 
            // tbStatus
            // 
            this.tbStatus.Location = new System.Drawing.Point(94, 163);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.Size = new System.Drawing.Size(186, 21);
            this.tbStatus.TabIndex = 4;
            // 
            // num阈值
            // 
            this.num阈值.Location = new System.Drawing.Point(107, 136);
            this.num阈值.Name = "num阈值";
            this.num阈值.Size = new System.Drawing.Size(68, 21);
            this.num阈值.TabIndex = 5;
            this.num阈值.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.num阈值.ValueChanged += new System.EventHandler(this.num阈值_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "阈值（百分比）";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 226);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.num阈值);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.picBox截屏);
            this.Controls.Add(this.tb判据);
            this.Controls.Add(this.prgsBar判据);
            this.Controls.Add(this.btnAction);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "我是盗号木马！";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picBox截屏)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num阈值)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnAction;
		private System.Windows.Forms.ProgressBar prgsBar判据;
		private System.Windows.Forms.TextBox tb判据;
		private System.Windows.Forms.PictureBox picBox截屏;
		private System.Windows.Forms.TextBox tbStatus;
		private System.Windows.Forms.NumericUpDown num阈值;
		private System.Windows.Forms.Label label1;
	}
}

