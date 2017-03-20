namespace ScannerDemo
{
	partial class FormGetPort
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
			this.btOK = new System.Windows.Forms.Button();
			this.cmPort = new System.Windows.Forms.ComboBox();
			this.btCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btOK
			// 
			this.btOK.Location = new System.Drawing.Point(12, 54);
			this.btOK.Name = "btOK";
			this.btOK.Size = new System.Drawing.Size(88, 37);
			this.btOK.TabIndex = 0;
			this.btOK.Text = "OK";
			this.btOK.UseVisualStyleBackColor = true;
			this.btOK.Click += new System.EventHandler(this.btOK_Click);
			// 
			// cmPort
			// 
			this.cmPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmPort.FormattingEnabled = true;
			this.cmPort.Location = new System.Drawing.Point(26, 12);
			this.cmPort.Name = "cmPort";
			this.cmPort.Size = new System.Drawing.Size(171, 21);
			this.cmPort.TabIndex = 2;
			this.cmPort.SelectedIndexChanged += new System.EventHandler(this.cmPort_SelectedIndexChanged);
			// 
			// btCancel
			// 
			this.btCancel.Location = new System.Drawing.Point(123, 54);
			this.btCancel.Name = "btCancel";
			this.btCancel.Size = new System.Drawing.Size(88, 37);
			this.btCancel.TabIndex = 1;
			this.btCancel.Text = "Cancel";
			this.btCancel.UseVisualStyleBackColor = true;
			this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
			// 
			// FormGetPort
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(223, 103);
			this.ControlBox = false;
			this.Controls.Add(this.cmPort);
			this.Controls.Add(this.btCancel);
			this.Controls.Add(this.btOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormGetPort";
			this.Text = "Select Scanner Port";
			this.Load += new System.EventHandler(this.FormGetPort_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btOK;
		private System.Windows.Forms.ComboBox cmPort;
		private System.Windows.Forms.Button btCancel;
	}
}