using Ini;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using Ini;
*/

namespace SimpleEpub
{
	public class Tools : Form
	{
		public static String iniPath = Path.Combine(Path.GetTempPath(), "SimpleEpub") + "\\Resources\\settings.ini";
		public IniFile ini = new IniFile(iniPath);

		public void loadINI()
		{
			if (!File.Exists(iniPath))
			{
				writeDefaultSettings();
			}
		}

		public void saveSettings(Settings settingsForm, int flag)		// flag == 0: restore default; flag == 1: save current
		{
			FileInfo iniInfo = new FileInfo(iniPath);
			if (!File.Exists(iniPath) || !iniInfo.IsReadOnly)
			{
				if (flag == 0) writeDefaultSettings();
				else writeCurSettings(settingsForm);
				loadCurSettings(settingsForm);
				this.Close();
			}
			else
			{
				MessageBox.Show("写入设置文件出错，可能是设置文件被设为只读。\n请取消其只读状态或删除设置文件，并点击“确认”键重试！");
			}
		}

		public void loadCurSettings(Settings settingsForm)
		{
			settingsForm.settings1_3_coverfirstpage_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Cover_FirstPage")));
			settingsForm.settings1_3_covernoTOC_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Cover_NoTOC")));
			if (!settingsForm.settings1_3_coverfirstpage_checkbox.Checked)
			{
				settingsForm.settings1_3_covernoTOC_checkbox.Checked = false;
				settingsForm.settings1_3_covernoTOC_checkbox.Enabled = false;
			}
			settingsForm.settings1_3_vertical_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Vertical")));
			settingsForm.settings1_3_replace_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Replace")));
			settingsForm.settings1_3_StT_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "StT")));
			settingsForm.settings1_3_TtS_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "TtS")));
			if (settingsForm.settings1_3_StT_checkbox.Checked)
			{
				settingsForm.settings1_3_TtS_checkbox.Checked = false;
				settingsForm.settings1_3_TtS_checkbox.Enabled = false;
			}
			if (settingsForm.settings1_3_TtS_checkbox.Checked)
			{
				settingsForm.settings1_3_StT_checkbox.Checked = false;
				settingsForm.settings1_3_StT_checkbox.Enabled = false;
			}
			settingsForm.settings1_3_embedFontSubset_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Embed_Font_Subset")));
			settingsForm.settings2_3_booknamefont_combobox.Text = ini.IniReadValue("Tab_2", "Cover_BookName_Font");
			settingsForm.settings2_3_authornamefont_combobox.Text = ini.IniReadValue("Tab_2", "Cover_AuthorName_Font");
			settingsForm.settings2_1_pc_combobox.Text = convertCodeToColor(ini.IniReadValue("Tab_2", "Page_Color"));
			settingsForm.settings2_2_pmU_textbox.Text = ini.IniReadValue("Tab_2", "Page_Margin_Up");
			settingsForm.settings2_2_pmD_textbox.Text = ini.IniReadValue("Tab_2", "Page_Margin_Down");
			settingsForm.settings2_2_pmL_textbox.Text = ini.IniReadValue("Tab_2", "Page_Margin_Left");
			settingsForm.settings2_2_pmR_textbox.Text = ini.IniReadValue("Tab_2", "Page_Margin_Right");
			settingsForm.settings3_1_tfont_combobox.Text = ini.IniReadValue("Tab_3", "Title_Font");
			settingsForm.settings3_1_tsize_textbox.Text = ini.IniReadValue("Tab_3", "Title_Size");
			settingsForm.settings3_1_tcolor_combobox.Text = convertCodeToColor(ini.IniReadValue("Tab_3", "Title_Color"));
			settingsForm.settings3_2_bfont_combobox.Text = ini.IniReadValue("Tab_3", "Body_Font");
			settingsForm.settings3_2_bsize_textbox.Text = ini.IniReadValue("Tab_3", "Body_Size");
			settingsForm.settings3_2_bcolor_combobox.Text = convertCodeToColor(ini.IniReadValue("Tab_3", "Body_Color"));
			settingsForm.settings3_3_linespacing_textbox.Text = ini.IniReadValue("Tab_3", "Line_Spacing");
			settingsForm.settings3_3_addparagraphspacing_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_3", "Add_Paragraph_Spacing")));
			settingsForm.settings4_1_filelocation_textbox.Text = ini.IniReadValue("Tab_4", "Generated_File_Location");
			settingsForm.settings4_2_dragclearlist_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_4", "Drag_Clear_List")));
			settingsForm.settings4_2_deletetempfiles_checkbox.Checked = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_4", "Delete_Temp_Files")));
		}

		public void writeCurSettings(Settings settingsForm)
		{
			ini.IniWriteValue("Tab_1", "Cover_FirstPage", (Convert.ToInt32(settingsForm.settings1_3_coverfirstpage_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_1", "Cover_NoTOC", (Convert.ToInt32(settingsForm.settings1_3_covernoTOC_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_1", "Vertical", (Convert.ToInt32(settingsForm.settings1_3_vertical_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_1", "Replace", (Convert.ToInt32(settingsForm.settings1_3_replace_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_1", "StT", (Convert.ToInt32(settingsForm.settings1_3_StT_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_1", "TtS", (Convert.ToInt32(settingsForm.settings1_3_TtS_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_1", "Embed_Font_Subset", (Convert.ToInt32(settingsForm.settings1_3_embedFontSubset_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_2", "Cover_BookName_Font", settingsForm.settings2_3_booknamefont_combobox.Text);
			ini.IniWriteValue("Tab_2", "Cover_AuthorName_Font", settingsForm.settings2_3_authornamefont_combobox.Text);
			ini.IniWriteValue("Tab_2", "Page_Color", convertColorToCode(settingsForm.settings2_1_pc_combobox.Text, 0));
			ini.IniWriteValue("Tab_2", "Page_Margin_Up", settingsForm.settings2_2_pmU_textbox.Text);
			ini.IniWriteValue("Tab_2", "Page_Margin_Down", settingsForm.settings2_2_pmD_textbox.Text);
			ini.IniWriteValue("Tab_2", "Page_Margin_Left", settingsForm.settings2_2_pmL_textbox.Text);
			ini.IniWriteValue("Tab_2", "Page_Margin_Right", settingsForm.settings2_2_pmR_textbox.Text);
			ini.IniWriteValue("Tab_3", "Title_Font", settingsForm.settings3_1_tfont_combobox.Text);
			ini.IniWriteValue("Tab_3", "Title_Size", settingsForm.settings3_1_tsize_textbox.Text);
			ini.IniWriteValue("Tab_3", "Title_Color", convertColorToCode(settingsForm.settings3_1_tcolor_combobox.Text, 1));
			ini.IniWriteValue("Tab_3", "Body_Font", settingsForm.settings3_2_bfont_combobox.Text);
			ini.IniWriteValue("Tab_3", "Body_Size", settingsForm.settings3_2_bsize_textbox.Text);
			ini.IniWriteValue("Tab_3", "Body_Color", convertColorToCode(settingsForm.settings3_2_bcolor_combobox.Text, 1));
			ini.IniWriteValue("Tab_3", "Line_Spacing", settingsForm.settings3_3_linespacing_textbox.Text);
			ini.IniWriteValue("Tab_3", "Add_Paragraph_Spacing", (Convert.ToInt32(settingsForm.settings3_3_addparagraphspacing_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_4", "Generated_File_Location", settingsForm.settings4_1_filelocation_textbox.Text);
			ini.IniWriteValue("Tab_4", "Drag_Clear_List", (Convert.ToInt32(settingsForm.settings4_2_dragclearlist_checkbox.Checked)).ToString());
			ini.IniWriteValue("Tab_4", "Delete_Temp_Files", (Convert.ToInt32(settingsForm.settings4_2_deletetempfiles_checkbox.Checked)).ToString());
		}

		public void writeDefaultSettings()
		{
			/*
			settings1_3_coverfirstpage_checkbox = False;
			settings1_3_covernoTOC_checkbox = False;
			settings1_3_vertical_checkbox = False;
			settings1_3_replace_checkbox = False;
			settings1_3_StT_checkbox = False;
			settings1_3_TtS_checkbox = False;
			settings1_3_embedFontSubset_checkbox = False;
			settings2_3_booknamefont_combobox = 微软雅黑;
			settings2_3_authornamefont_combobox = 微软雅黑;
			settings2_1_pc_combobox = 无;
			settings2_2_pmU_textbox = 0;
			settings2_2_pmD_textbox = 0;
			settings2_2_pmL_textbox = 1;
			settings2_2_pmR_textbox = 1;
			settings3_1_tfont_combobox = 微软雅黑;
			settings3_1_tsize_textbox = 24;
			settings3_1_tcolor_combobox = 深蓝;
			settings3_2_bfont_combobox = 微软雅黑;
			settings3_2_bsize_textbox = 14;
			settings3_2_bcolor_combobox = 黑色;
			settings3_3_linespacing_textbox = 130;
			settings3_3_addparagraphspacing_checkbox = False;
			settings4_1_filelocation_textbox = Application.StartupPath;
			settings4_2_dragclearlist_checkbox = True;
			settings4_2_deletetempfiles_checkbox = True;
			*/

			ini.IniWriteValue("Tab_1", "Cover_FirstPage", "0");
			ini.IniWriteValue("Tab_1", "Cover_NoTOC", "0");
			ini.IniWriteValue("Tab_1", "Vertical", "0");
			ini.IniWriteValue("Tab_1", "Replace", "0");
			ini.IniWriteValue("Tab_1", "StT", "0");
			ini.IniWriteValue("Tab_1", "TtS", "0");
			ini.IniWriteValue("Tab_1", "Embed_Font_Subset", "0");
			ini.IniWriteValue("Tab_2", "Cover_BookName_Font", "微软雅黑");
			ini.IniWriteValue("Tab_2", "Cover_AuthorName_Font", "微软雅黑");
			ini.IniWriteValue("Tab_2", "Page_Color", "none");
			ini.IniWriteValue("Tab_2", "Page_Margin_Up", "0");
			ini.IniWriteValue("Tab_2", "Page_Margin_Down", "0");
			ini.IniWriteValue("Tab_2", "Page_Margin_Left", "1");
			ini.IniWriteValue("Tab_2", "Page_Margin_Right", "1");
			ini.IniWriteValue("Tab_3", "Title_Font", "微软雅黑");
			ini.IniWriteValue("Tab_3", "Title_Size", "24");
			ini.IniWriteValue("Tab_3", "Title_Color", "#365F91");
			ini.IniWriteValue("Tab_3", "Body_Font", "微软雅黑");
			ini.IniWriteValue("Tab_3", "Body_Size", "14");
			ini.IniWriteValue("Tab_3", "Body_Color", "black");
			ini.IniWriteValue("Tab_3", "Line_Spacing", "130");
			ini.IniWriteValue("Tab_3", "Add_Paragraph_Spacing", "0");
			ini.IniWriteValue("Tab_4", "Generated_File_Location", Application.StartupPath);
			ini.IniWriteValue("Tab_4", "Drag_Clear_List", "1");
			ini.IniWriteValue("Tab_4", "Delete_Temp_Files", "1");
		}

		public static String convertColorToCode(String input, int flag)		// flag == 0: from settingsForm.settings2_1_pc_combobox.Text; else otherwise
		{
			String text = input.Trim();
			if (String.Compare(text, "白色") == 0)
			{
				return "white";
			}
			else if (String.Compare(text, "黑色") == 0)
			{
				return "black";
			}
			else if (String.Compare(text, "红色") == 0)
			{
				return "red";
			}
			else if (String.Compare(text, "黄色") == 0)
			{
				return "yellow";
			}
			else if (String.Compare(text, "蓝色") == 0)
			{
				return "blue";
			}
			else if (String.Compare(text, "绿色") == 0)
			{
				return "green";
			}
			else if (String.Compare(text, "紫色") == 0)
			{
				return "purple";
			}
			else if (String.Compare(text, "橙色") == 0)
			{
				return "orange";
			}
			else if (String.Compare(text, "护眼绿") == 0)
			{
				return "#B2DEC2";
			}
			else if (String.Compare(text, "深蓝") == 0)
			{
				return "#365F91";
			}
			else if (String.Compare(text, "浅蓝") == 0)
			{
				return "#4F81BD";
			}
			else if (String.Compare(text, "羊皮纸") == 0)
			{
				return "羊皮纸";
			}
			else if (String.Compare(text, "无") == 0)
			{
				return "none";
			}
			else
			{
				if (containChineseChar(text))
				{
					if (flag == 0) return "white";
					else return "black";
				}
				else
				{
					// if input doesn't contain Chinese character
					// treat it as HTML code
					return text;
				}
			}
		}

		private static bool containChineseChar(String text)
		{
			bool containUnicode = false;
			for (int x = 0; x < text.Length; x++)
			{
				if (char.GetUnicodeCategory(text[x]) == UnicodeCategory.OtherLetter)
				{
					containUnicode = true;
					break;
				}
			}
			return containUnicode;
		}

		public static String convertCodeToColor(String input)
		{
			String text = input.Trim();
			if (String.Compare(text, "white") == 0)
			{
				return "白色";
			}
			else if (String.Compare(text, "black") == 0)
			{
				return "黑色";
			}
			else if (String.Compare(text, "red") == 0)
			{
				return "红色";
			}
			else if (String.Compare(text, "yellow") == 0)
			{
				return "黄色";
			}
			else if (String.Compare(text, "blue") == 0)
			{
				return "蓝色";
			}
			else if (String.Compare(text, "green") == 0)
			{
				return "绿色";
			}
			else if (String.Compare(text, "purple") == 0)
			{
				return "紫色";
			}
			else if (String.Compare(text, "orange") == 0)
			{
				return "橙色";
			}
			else if (String.Compare(text, "#B2DEC2") == 0)
			{
				return "护眼绿";
			}
			else if (String.Compare(text, "#365F91") == 0)
			{
				return "深蓝";
			}
			else if (String.Compare(text, "#4F81BD") == 0)
			{
				return "浅蓝";
			}
			else if (String.Compare(text, "none") == 0)
			{
				return "无";
			}
			else return text;
		}

	}
}
