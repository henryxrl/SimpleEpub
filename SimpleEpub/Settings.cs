using Ini;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using Ini;
*/

namespace SimpleEpub
{
	public partial class Settings : Tools
    {
        public Settings()
        {
            InitializeComponent();
        }

		private void Settings_Load(object sender, EventArgs e)
		{
			loadINI();
			try
			{
				loadCurSettings(this);
			}
			catch
			{
				MessageBox.Show("加载设置文件出错，即将导入默认设置！");
				writeDefaultSettings();
				loadCurSettings(this);
			}

			InstalledFontCollection installedFontCollection = new InstalledFontCollection();
			FontFamily[] fontFamilies = installedFontCollection.Families;

			for (int i = 0; i < fontFamilies.Length; i++)
			{
				String fontName = fontFamilies[i].Name.ToString();

				Regex r = new Regex(@"[\u4e00-\u9fa5]+");
				Match mc = r.Match(fontName);
				if (mc.Length != 0 && !fontName.Contains("Adobe"))
				{
					settings2_3_booknamefont_combobox.Items.Add(fontName);
					settings2_3_authornamefont_combobox.Items.Add(fontName);
					settings3_1_tfont_combobox.Items.Add(fontName);
					settings3_2_bfont_combobox.Items.Add(fontName);
				}
			}
		}

		private void settings4_3_reset_button_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult = MessageBox.Show("确认还原默认设置吗？", "确认", MessageBoxButtons.YesNo);
			if (dialogResult == DialogResult.Yes)
			{
				saveSettings(this, 0);
			}
		}

		private void settings_done_button_Click(object sender, EventArgs e)
		{
			saveSettings(this, 1);
		}

		private void settings4_1_filelocation_button_Click(object sender, EventArgs e)
		{
			if (settings4_1_filelocation_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				settings4_1_filelocation_textbox.Text = settings4_1_filelocation_dialog.SelectedPath;
			}
		}

		private void settings1_3_StT_checkbox_CheckedChanged(object sender, EventArgs e)
		{
			this.settings1_3_TtS_checkbox.Enabled = !this.settings1_3_StT_checkbox.Checked;

			if (!this.settings1_3_TtS_checkbox.Enabled)
			{
				this.settings1_3_TtS_checkbox.Checked = false;
				this.settings1_3_TtS_checkbox.Enabled = false;
			}
			else
			{
				this.settings1_3_TtS_checkbox.Checked = false;
				this.settings1_3_TtS_checkbox.Enabled = true;
			}
		}

		private void settings1_3_TtS_checkbox_CheckedChanged(object sender, EventArgs e)
		{
			this.settings1_3_StT_checkbox.Enabled = !this.settings1_3_TtS_checkbox.Checked;

			if (!this.settings1_3_StT_checkbox.Enabled)
			{
				this.settings1_3_StT_checkbox.Checked = false;
				this.settings1_3_StT_checkbox.Enabled = false;
			}
			else
			{
				this.settings1_3_StT_checkbox.Checked = false;
				this.settings1_3_StT_checkbox.Enabled = true;
			}
		}

		private void settings1_3_coverfirstpage_checkbox_CheckedChanged(object sender, EventArgs e)
		{
			this.settings1_3_covernoTOC_checkbox.Enabled = this.settings1_3_coverfirstpage_checkbox.Checked;

			if (!this.settings1_3_covernoTOC_checkbox.Enabled)
			{
				this.settings1_3_covernoTOC_checkbox.Checked = false;
			}
			else
			{
				this.settings1_3_covernoTOC_checkbox.Checked = false;
			}
		}

		private void settings2_3_booknamefont_combobox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			Font objFonts = new Font(settings2_3_booknamefont_combobox.Items[e.Index].ToString(), 14);
			e.ItemHeight = objFonts.Height;
		}

		private void settings2_3_booknamefont_combobox_DrawItem(object sender, DrawItemEventArgs e)
		{
			System.Drawing.Font objFonts = new Font(settings2_3_booknamefont_combobox.Items[e.Index].ToString(), 14);
			e.DrawBackground();

			e.Graphics.DrawString(settings2_3_booknamefont_combobox.Items[e.Index].ToString(), objFonts, new SolidBrush(e.ForeColor), new Point(e.Bounds.Left, e.Bounds.Top));
		}

		private void settings2_3_authornamefont_combobox_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
		{
			Font objFonts = new Font(settings2_3_authornamefont_combobox.Items[e.Index].ToString(), 14);
			e.ItemHeight = objFonts.Height;
		}

		private void settings2_3_authornamefont_combobox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			System.Drawing.Font objFonts = new Font(settings2_3_authornamefont_combobox.Items[e.Index].ToString(), 14);
			e.DrawBackground();

			e.Graphics.DrawString(settings2_3_authornamefont_combobox.Items[e.Index].ToString(), objFonts, new SolidBrush(e.ForeColor), new Point(e.Bounds.Left, e.Bounds.Top));
		}

		private void settings3_1_tfont_combobox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			Font objFonts = new Font(settings3_1_tfont_combobox.Items[e.Index].ToString(), 14);
			e.ItemHeight = objFonts.Height;
		}

		private void settings3_1_tfont_combobox_DrawItem(object sender, DrawItemEventArgs e)
		{
			System.Drawing.Font objFonts = new Font(settings3_1_tfont_combobox.Items[e.Index].ToString(), 14);
			e.DrawBackground();

			e.Graphics.DrawString(settings3_1_tfont_combobox.Items[e.Index].ToString(), objFonts, new SolidBrush(e.ForeColor), new Point(e.Bounds.Left, e.Bounds.Top));
		}

		private void settings3_2_bfont_combobox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			Font objFonts = new Font(settings3_2_bfont_combobox.Items[e.Index].ToString(), 14);
			e.ItemHeight = objFonts.Height;
		}

		private void settings3_2_bfont_combobox_DrawItem(object sender, DrawItemEventArgs e)
		{
			System.Drawing.Font objFonts = new Font(settings3_2_bfont_combobox.Items[e.Index].ToString(), 14);
			e.DrawBackground();

			e.Graphics.DrawString(settings3_2_bfont_combobox.Items[e.Index].ToString(), objFonts, new SolidBrush(e.ForeColor), new Point(e.Bounds.Left, e.Bounds.Top));
		}

	}
}
