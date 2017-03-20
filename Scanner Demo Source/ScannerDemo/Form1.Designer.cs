namespace ScannerDemo
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.btStart = new System.Windows.Forms.Button();
			this.btStop = new System.Windows.Forms.Button();
			this.pbCapture = new System.Windows.Forms.PictureBox();
			this.cbPreview = new System.Windows.Forms.CheckBox();
			this.cbIllumPreview = new System.Windows.Forms.CheckBox();
			this.tbResponse = new System.Windows.Forms.TextBox();
			this.lbInstructions = new System.Windows.Forms.Label();
			this.btSave = new System.Windows.Forms.Button();
			this.cbIllumCapture = new System.Windows.Forms.CheckBox();
			this.lbIllumination = new System.Windows.Forms.Label();
			this.btCapture = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pbCapture)).BeginInit();
			this.SuspendLayout();
			// 
			// btStart
			// 
			this.btStart.Location = new System.Drawing.Point(16, 13);
			this.btStart.Name = "btStart";
			this.btStart.Size = new System.Drawing.Size(80, 49);
			this.btStart.TabIndex = 0;
			this.btStart.Text = "Start";
			this.btStart.UseVisualStyleBackColor = true;
			this.btStart.Click += new System.EventHandler(this.btStart_Click);
			// 
			// btStop
			// 
			this.btStop.Enabled = false;
			this.btStop.Location = new System.Drawing.Point(188, 13);
			this.btStop.Name = "btStop";
			this.btStop.Size = new System.Drawing.Size(80, 49);
			this.btStop.TabIndex = 4;
			this.btStop.Text = "Stop";
			this.btStop.UseVisualStyleBackColor = true;
			this.btStop.Click += new System.EventHandler(this.btStop_Click);
			// 
			// pbCapture
			// 
			this.pbCapture.Location = new System.Drawing.Point(16, 103);
			this.pbCapture.Name = "pbCapture";
			this.pbCapture.Size = new System.Drawing.Size(864, 640);
			this.pbCapture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbCapture.TabIndex = 1;
			this.pbCapture.TabStop = false;
			// 
			// cbPreview
			// 
			this.cbPreview.AutoSize = true;
			this.cbPreview.Checked = true;
			this.cbPreview.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbPreview.Location = new System.Drawing.Point(16, 75);
			this.cbPreview.Name = "cbPreview";
			this.cbPreview.Size = new System.Drawing.Size(93, 17);
			this.cbPreview.TabIndex = 2;
			this.cbPreview.Text = "Show preview";
			this.cbPreview.UseVisualStyleBackColor = true;
			this.cbPreview.CheckedChanged += new System.EventHandler(this.cbPreview_CheckedChanged);
			// 
			// cbIllumPreview
			// 
			this.cbIllumPreview.AutoSize = true;
			this.cbIllumPreview.Location = new System.Drawing.Point(259, 75);
			this.cbIllumPreview.Name = "cbIllumPreview";
			this.cbIllumPreview.Size = new System.Drawing.Size(64, 17);
			this.cbIllumPreview.TabIndex = 3;
			this.cbIllumPreview.Text = "Preview";
			this.cbIllumPreview.UseVisualStyleBackColor = true;
			this.cbIllumPreview.CheckedChanged += new System.EventHandler(this.cbIllumPreview_CheckedChanged);
			// 
			// tbResponse
			// 
			this.tbResponse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tbResponse.Location = new System.Drawing.Point(360, 13);
			this.tbResponse.Multiline = true;
			this.tbResponse.Name = "tbResponse";
			this.tbResponse.ReadOnly = true;
			this.tbResponse.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbResponse.Size = new System.Drawing.Size(520, 49);
			this.tbResponse.TabIndex = 5;
			// 
			// lbInstructions
			// 
			this.lbInstructions.AutoSize = true;
			this.lbInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbInstructions.Location = new System.Drawing.Point(331, 71);
			this.lbInstructions.Name = "lbInstructions";
			this.lbInstructions.Size = new System.Drawing.Size(170, 20);
			this.lbInstructions.TabIndex = 7;
			this.lbInstructions.Text = "Instructions go here";
			// 
			// btSave
			// 
			this.btSave.Location = new System.Drawing.Point(274, 13);
			this.btSave.Name = "btSave";
			this.btSave.Size = new System.Drawing.Size(80, 49);
			this.btSave.TabIndex = 5;
			this.btSave.Text = "Save";
			this.btSave.UseVisualStyleBackColor = true;
			this.btSave.Visible = false;
			this.btSave.Click += new System.EventHandler(this.btSave_Click);
			// 
			// cbIllumCapture
			// 
			this.cbIllumCapture.AutoSize = true;
			this.cbIllumCapture.Location = new System.Drawing.Point(190, 75);
			this.cbIllumCapture.Name = "cbIllumCapture";
			this.cbIllumCapture.Size = new System.Drawing.Size(63, 17);
			this.cbIllumCapture.TabIndex = 3;
			this.cbIllumCapture.Text = "Capture";
			this.cbIllumCapture.UseVisualStyleBackColor = true;
			this.cbIllumCapture.CheckedChanged += new System.EventHandler(this.cbIllumCapture_CheckedChanged);
			// 
			// lbIllumination
			// 
			this.lbIllumination.AutoSize = true;
			this.lbIllumination.Location = new System.Drawing.Point(121, 76);
			this.lbIllumination.Name = "lbIllumination";
			this.lbIllumination.Size = new System.Drawing.Size(62, 13);
			this.lbIllumination.TabIndex = 8;
			this.lbIllumination.Text = "Illumination:";
			// 
			// btCapture
			// 
			this.btCapture.Enabled = false;
			this.btCapture.Location = new System.Drawing.Point(102, 13);
			this.btCapture.Name = "btCapture";
			this.btCapture.Size = new System.Drawing.Size(80, 49);
			this.btCapture.TabIndex = 9;
			this.btCapture.Text = "Capture";
			this.btCapture.UseVisualStyleBackColor = true;
			this.btCapture.Click += new System.EventHandler(this.btCapture_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(897, 760);
			this.Controls.Add(this.btCapture);
			this.Controls.Add(this.lbIllumination);
			this.Controls.Add(this.lbInstructions);
			this.Controls.Add(this.tbResponse);
			this.Controls.Add(this.cbIllumCapture);
			this.Controls.Add(this.cbIllumPreview);
			this.Controls.Add(this.cbPreview);
			this.Controls.Add(this.pbCapture);
			this.Controls.Add(this.btSave);
			this.Controls.Add(this.btStop);
			this.Controls.Add(this.btStart);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "Scanner Demo V1.10";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.pbCapture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btStart;
		private System.Windows.Forms.Button btStop;
		private System.Windows.Forms.PictureBox pbCapture;
		private System.Windows.Forms.CheckBox cbPreview;
		private System.Windows.Forms.CheckBox cbIllumPreview;
		private System.Windows.Forms.TextBox tbResponse;
		private System.Windows.Forms.Label lbInstructions;
		private System.Windows.Forms.Button btSave;
		private System.Windows.Forms.CheckBox cbIllumCapture;
		private System.Windows.Forms.Label lbIllumination;
		private System.Windows.Forms.Button btCapture;
	}
}

