namespace SimpleEpub
{
    partial class About
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
			this.About_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.logo_picturebox = new System.Windows.Forms.PictureBox();
			this.name_label = new System.Windows.Forms.Label();
			this.version_label = new System.Windows.Forms.Label();
			this.author_label = new System.Windows.Forms.Label();
			this.email_label = new System.Windows.Forms.Label();
			this.description_textbox = new System.Windows.Forms.TextBox();
			this.ok_button = new System.Windows.Forms.Button();
			this.About_tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.logo_picturebox)).BeginInit();
			this.SuspendLayout();
			// 
			// About_tableLayoutPanel
			// 
			this.About_tableLayoutPanel.ColumnCount = 2;
			this.About_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.About_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67F));
			this.About_tableLayoutPanel.Controls.Add(this.logo_picturebox, 0, 0);
			this.About_tableLayoutPanel.Controls.Add(this.name_label, 1, 0);
			this.About_tableLayoutPanel.Controls.Add(this.version_label, 1, 1);
			this.About_tableLayoutPanel.Controls.Add(this.author_label, 1, 2);
			this.About_tableLayoutPanel.Controls.Add(this.email_label, 1, 3);
			this.About_tableLayoutPanel.Controls.Add(this.description_textbox, 1, 4);
			this.About_tableLayoutPanel.Controls.Add(this.ok_button, 1, 5);
			this.About_tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.About_tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
			this.About_tableLayoutPanel.Name = "About_tableLayoutPanel";
			this.About_tableLayoutPanel.RowCount = 6;
			this.About_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.About_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.About_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.About_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.About_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.About_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.About_tableLayoutPanel.Size = new System.Drawing.Size(417, 265);
			this.About_tableLayoutPanel.TabIndex = 0;
			// 
			// logo_picturebox
			// 
			this.logo_picturebox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logo_picturebox.Image = ((System.Drawing.Image)(resources.GetObject("logo_picturebox.Image")));
			this.logo_picturebox.Location = new System.Drawing.Point(3, 3);
			this.logo_picturebox.Name = "logo_picturebox";
			this.About_tableLayoutPanel.SetRowSpan(this.logo_picturebox, 6);
			this.logo_picturebox.Size = new System.Drawing.Size(131, 259);
			this.logo_picturebox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.logo_picturebox.TabIndex = 12;
			this.logo_picturebox.TabStop = false;
			// 
			// name_label
			// 
			this.name_label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.name_label.Location = new System.Drawing.Point(143, 0);
			this.name_label.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.name_label.MaximumSize = new System.Drawing.Size(0, 17);
			this.name_label.Name = "name_label";
			this.name_label.Size = new System.Drawing.Size(271, 17);
			this.name_label.TabIndex = 19;
			this.name_label.Text = "SimpleEpub";
			this.name_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// version_label
			// 
			this.version_label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.version_label.Location = new System.Drawing.Point(143, 26);
			this.version_label.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.version_label.MaximumSize = new System.Drawing.Size(0, 17);
			this.version_label.Name = "version_label";
			this.version_label.Size = new System.Drawing.Size(271, 17);
			this.version_label.TabIndex = 0;
			this.version_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// author_label
			// 
			this.author_label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.author_label.Location = new System.Drawing.Point(143, 52);
			this.author_label.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.author_label.MaximumSize = new System.Drawing.Size(0, 17);
			this.author_label.Name = "author_label";
			this.author_label.Size = new System.Drawing.Size(271, 17);
			this.author_label.TabIndex = 21;
			this.author_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// email_label
			// 
			this.email_label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.email_label.Location = new System.Drawing.Point(143, 78);
			this.email_label.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.email_label.MaximumSize = new System.Drawing.Size(0, 17);
			this.email_label.Name = "email_label";
			this.email_label.Size = new System.Drawing.Size(271, 17);
			this.email_label.TabIndex = 22;
			this.email_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// description_textbox
			// 
			this.description_textbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.description_textbox.Location = new System.Drawing.Point(143, 107);
			this.description_textbox.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.description_textbox.Multiline = true;
			this.description_textbox.Name = "description_textbox";
			this.description_textbox.ReadOnly = true;
			this.description_textbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.description_textbox.Size = new System.Drawing.Size(271, 126);
			this.description_textbox.TabIndex = 23;
			this.description_textbox.TabStop = false;
			// 
			// ok_button
			// 
			this.ok_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ok_button.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ok_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ok_button.Location = new System.Drawing.Point(339, 239);
			this.ok_button.Name = "ok_button";
			this.ok_button.Size = new System.Drawing.Size(75, 23);
			this.ok_button.TabIndex = 24;
			this.ok_button.Text = "确定(&O)";
			this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
			// 
			// About
			// 
			this.AcceptButton = this.ok_button;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(435, 283);
			this.Controls.Add(this.About_tableLayoutPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "About";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "关于…";
			this.About_tableLayoutPanel.ResumeLayout(false);
			this.About_tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.logo_picturebox)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel About_tableLayoutPanel;
        private System.Windows.Forms.PictureBox logo_picturebox;
        private System.Windows.Forms.Label name_label;
        private System.Windows.Forms.Label version_label;
        private System.Windows.Forms.Label author_label;
        private System.Windows.Forms.Label email_label;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.TextBox description_textbox;
    }
}
