using Ini;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VB = Microsoft.VisualBasic;
using Word = Microsoft.Office.Interop.Word;
using Ionic.Zip;
using System.Reflection;
/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Diagnostics;
using System.Threading;
using Ini;
using Word = Microsoft.Office.Interop.Word;
using VB = Microsoft.VisualBasic;
*/

namespace SimpleEpub
{
    public partial class Main : Tools
    {
		private Settings settingsForm = new Settings();		// Able to get data from Settings form

		//String regex = "^([\\s\t　]{0,20}(正文[\\s\t　]{0,4})?[第【]([——-——一二两三四五六七八九十○零百千0-9０-９]{1,12}).*[章节節回集卷部】].*?$)|(Ui)(第.{1,5}章)|(Ui)(第.{1,5}节)";
		//String regex = "^([\\s\t　]*(正文[\\s\t　]*)?[第【][\\s\t　]*([——-——一二两三四五六七八九十○零百千壹贰叁肆伍陆柒捌玖拾佰仟0-9０-９]*)[\\s\t　]*[章节節回集卷部】][\\s\t　]*.{0,40}?$)|(Ui)(第.{1,5}章)|(Ui)(第.{1,5}节)";
		String regex = "^([\\s\t　]*([【])?(正文[\\s\t　]*)?[第【][\\s\t　]*([——-——一二两三四五六七八九十○零百千壹贰叁肆伍陆柒捌玖拾佰仟0-9０-９]*)[\\s\t　]*[章节節回集卷部】][\\s\t　]*.{0,40}?$)";

		List<String> bookAndAuthor;
		List<int> titleLineNumbers = new List<int>();
		String TXTPath;
		String CoverPath;
		//String HTMLPath;
		String HTMLFolderPath;
		String CSSPath;
		String DocName;
		String Intro;
		int chapterNumber = 1;
		String tempPath = Path.Combine(Path.GetTempPath(), "SimpleEpub");
		String iniPath = Path.Combine(Path.GetTempPath(), "SimpleEpub") + "\\settings.ini";		// Application.StartupPath + "\\settings.ini"

		String defaultStatusText = "SimpleEpub 版本: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

		Stopwatch stopWatch;
		int timerCount;

		Image toBeShown;

        public Main()
        {
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				string resourceName = new AssemblyName(args.Name).Name + ".dll";
				string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
				{
					Byte[] assemblyData = new Byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			};

            InitializeComponent();
        }

		private void Main_Load(object sender, EventArgs e)
		{
			((Control)cover_picturebox).AllowDrop = true;

			this.status_label.Text = defaultStatusText;
			Directory.CreateDirectory(tempPath);
			IniFile ini = loadINI(iniPath);
			try
			{
				loadCurSettings(settingsForm, ini);
			}
			catch
			{
				MessageBox.Show("加载设置文件出错，即将导入默认设置！");
				writeDefaultSettings(ini);
				loadCurSettings(settingsForm, ini);
			}
		}

        private void menu1_switchAOT_Click(object sender, EventArgs e)
        {
            if (this.TopMost)
            {
                this.TopMost = false;
				this.status_label.Text = "已取消置顶";
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
				showBalloonTip("温馨提示", "已取消置顶");
            }
            else
            {
                this.TopMost = true;
				this.status_label.Text = "已置顶";
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
				showBalloonTip("温馨提示", "已置顶");
            }
        }

		private void menu2_settings_Click(object sender, EventArgs e)
		{
			//Settings settingsForm = new Settings();
			settingsForm = new Settings();
			settingsForm.ShowDialog();
		}

		private void menu3_export_Click(object sender, EventArgs e)
		{
			if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[0].Cells[1].Value != null)
			{
				this.status_label.Text = "正在导出目录...";

				String TOCPath = getTOCPath(iniPath);
				StreamWriter sw = new StreamWriter(TOCPath, false, Encoding.GetEncoding("GB2312"));

				foreach (DataGridViewRow row in TOC_list.Rows)
				{
					if (row.Cells[0].Value != null && row.Cells[1].Value != null)
					{
						String toPrint = row.Cells[0].Value.ToString() + ">" + row.Cells[1].Value.ToString();
						sw.WriteLine(toPrint);
					}
				}
				sw.Close();

				this.status_label.Text = "目录导出完毕！";
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
				showBalloonTip("温馨提示", "目录导出完毕！\n目录位置：" + TOCPath);
			}
			else
			{
				MessageBox.Show("目录是空的，无法导出！");
			}
		}

		private void menu4_import_Click(object sender, EventArgs e)
		{
			String TOCPath = getTOCPath(iniPath);
			if (File.Exists(TOCPath))
			{
				StreamReader sr = new StreamReader(TOCPath, Encoding.GetEncoding("GB2312"));

				if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[0].Cells[1].Value != null)
				{
					DialogResult dialogResult = MessageBox.Show("目录已存在！\n是否先清空目录？", "确认", MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						TOC_list.Rows.Clear();
					}
				}

				this.status_label.Text = "正在导入目录...";
				stopWatch = new Stopwatch();
				stopWatch.Start();

				// Set cell font colors
				setCellFontColor(Color.Black, Color.RoyalBlue);

				String nextLine;
				String[] separators = { ">" };
				while ((nextLine = sr.ReadLine()) != null)
				{
					String[] data = nextLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);
					DataGridViewRow row = (DataGridViewRow)TOC_list.Rows[0].Clone();
					row.Cells[0].Value = data[0];
					row.Cells[1].Value = data[1];
					TOC_list.Rows.Add(row);
				}
				sr.Close();

				this.status_label.Text = "目录导入完毕！耗时：" + getProcessTime().ToString() + " 秒";
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
				showBalloonTip("温馨提示", "目录导入完毕！");
			}
			else
			{
				MessageBox.Show("目录文件不存在，无法导入目录！");
			}
		}

		private void menu5_clear_Click(object sender, EventArgs e)
		{
			if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[0].Cells[1].Value != null)
			{
				DialogResult dialogResult = MessageBox.Show("建议你按导出目录菜单以保存列表框中的数据\n你确定要清空列表框中的数据？", "确认", MessageBoxButtons.YesNo);
				if (dialogResult == DialogResult.Yes)
				{
					this.status_label.Text = "正在清空目录...";

					TOC_list.Rows.Clear();

					this.status_label.Text = "目录清空完毕！";
					notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
					showBalloonTip("温馨提示", "目录清空完毕！");
				}
			}
			else
			{
				MessageBox.Show("目录是空的，无法清空！");
			}
		}

		private void menu6_1_helpfile_menuitem_Click(object sender, EventArgs e)
		{
			Extract("SimpleEpub", tempPath, "Resources", "help.pdf");
			try
			{
				System.Diagnostics.Process.Start(tempPath + "\\help.pdf");
			}
			catch
			{
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Error;
				showBalloonTip("错误！", "帮助文件不存在！");
			}
		}

		private void menu6_3_about_menuitem_Click(object sender, EventArgs e)
		{
			About aboutForm = new About();
			aboutForm.ShowDialog();
		}

		private void toostripmenu1_show_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Normal;
		}

		private void toostripmenu2_exit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void notifyIcon1_DoubleClick(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Normal;
		}

		private void TOC_list_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.All;
			else
				e.Effect = DragDropEffects.None;
		}

		private void TOC_list_DragDrop(object sender, DragEventArgs e)
		{
			this.status_label.Text = "正在提取目录...";

			String[] test = (String[])e.Data.GetData(DataFormats.FileDrop);
			if (test.Length != 1)
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("只能拖拽一个文件！");
				e.Effect = DragDropEffects.None;
			}
			else if (!test[0].ToLower().EndsWith(".txt"))
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("拖拽进来的不是TXT文件！");
				e.Effect = DragDropEffects.None;
			}
			else
			{
				stopWatch = new Stopwatch();
				stopWatch.Start();

				IniFile ini = loadINI(iniPath);
				if (Convert.ToInt32(ini.IniReadValue("Tab_4", "Drag_Clear_List")) == 1)		//拖入文件时清空列表
				{
					if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[0].Cells[1].Value != null)
					{
						TOC_list.Rows.Clear();
					}
				}

				// Save txt file path
				TXTPath = test[0];

				// Set cell font colors
				setCellFontColor(Color.Black, Color.RoyalBlue);

				// Get file name
				String filename = Path.GetFileNameWithoutExtension(TXTPath);

				// Get book name and author info to fill the first two rows of TOC_list
				bookAndAuthor = getBookNameAndAuthorInfo(TXTPath, filename);
				DataGridViewRow tempRow1 = (DataGridViewRow)TOC_list.Rows[0].Clone();
				tempRow1.Cells[0].Value = bookAndAuthor[0];
				tempRow1.Cells[1].Value = "★书名，勿删此行★";
				TOC_list.Rows.Add(tempRow1);
				DataGridViewRow tempRow2 = (DataGridViewRow)TOC_list.Rows[0].Clone();
				tempRow2.Cells[0].Value = bookAndAuthor[1];
				tempRow2.Cells[1].Value = "★作者，勿删此行★";
				TOC_list.Rows.Add(tempRow2);

				// Fill author and bookname textboxes on the right side
				cover_bookname_textbox.Text = bookAndAuthor[0];
				cover_author_textbox.Text = bookAndAuthor[1];

				// Fill intro textbox on the right side
				Intro = getIntroInfo(TXTPath);
				cover_intro_textbox.Text = Intro;

				// Prepare a list of title line numbers
				StreamReader sr = new StreamReader(TXTPath, Encoding.GetEncoding("GB2312"));
				String nextLine;
				int lineNumber = 1;
				int rowNumber = 2;
				while ((nextLine = sr.ReadLine()) != null)
				{
					Match title = Regex.Match(nextLine, regex);

					// Chapter title (with its line number) found!
					if (title.Success)
					{
						DataGridViewRow row = (DataGridViewRow)TOC_list.Rows[rowNumber].Clone();
						row.Cells[0].Value = title.ToString().Trim();
						row.Cells[1].Value = lineNumber;
						TOC_list.Rows.Add(row);
						rowNumber++;

						titleLineNumbers.Add(lineNumber);
					}
					lineNumber++;
				}
				sr.Close();

				this.status_label.Text = "目录提取完毕！耗时：" + getProcessTime().ToString() + " 秒";
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
				showBalloonTip("温馨提示", "目录提取完毕！");

				// clear CoverPath
				CoverPath = null;
				cover_picturebox.Image = null;
				if (toBeShown != null)
					toBeShown.Dispose();

				// generate temp cover
				String pageColor = ini.IniReadValue("Tab_2", "Page_Color");
				String titleFont = ini.IniReadValue("Tab_3", "Title_Font");
				float titleSize = float.Parse(ini.IniReadValue("Tab_3", "Title_Size"));
				String titleColor = ini.IniReadValue("Tab_3", "Title_Color");
				String bodyFont = ini.IniReadValue("Tab_3", "Body_Font");
				float bodySize = float.Parse(ini.IniReadValue("Tab_3", "Body_Size"));
				String bodyColor = ini.IniReadValue("Tab_3", "Body_Color");
				CoverPath = tempPath + "\\cover.jpg";

				if (File.Exists(CoverPath))
				{
					try
					{
						File.Delete(CoverPath);
					}
					catch
					{
						if (File.Exists(CoverPath))
							MessageBox.Show("Deletion failed");
					}
				}

				using (Image cover = DrawText(bookAndAuthor[0], titleFont, titleSize, titleColor, bookAndAuthor[1], bodyFont, bodySize, bodyColor, pageColor, 1080, 1920))
				{
					try
					{
						cover.Save(CoverPath, System.Drawing.Imaging.ImageFormat.Jpeg);
					}
					catch
					{
						MessageBox.Show("coverpath: " + CoverPath);
					}
				}
				cover_picturebox.SizeMode = PictureBoxSizeMode.Zoom;
				toBeShown = new Bitmap(CoverPath);
				cover_picturebox.Image = toBeShown;
			}
		}

		private void cover_picturebox_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.All;
			else
				e.Effect = DragDropEffects.None;
		}

		private void cover_picturebox_DragDrop(object sender, DragEventArgs e)
		{
			String[] test = (String[])e.Data.GetData(DataFormats.FileDrop);
			if (test.Length != 1)
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("只能拖拽一个文件！");
				e.Effect = DragDropEffects.None;
			}
			else if (!test[0].ToLower().EndsWith(".jpg") && !test[0].ToLower().EndsWith(".jpeg"))
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("拖拽进来的不是JPG/JPEG文件！");
				e.Effect = DragDropEffects.None;
			}
			else
			{
				// clear CoverPath
				CoverPath = null;
				cover_picturebox.Image = null;
				if (toBeShown != null)
					toBeShown.Dispose();

				if (File.Exists(CoverPath))
				{
					try
					{
						File.Delete(CoverPath);
					}
					catch
					{
						if (File.Exists(CoverPath))
							MessageBox.Show("Deletion failed");
					}
				}

				// Save picture file path
				CoverPath = test[0];
				cover_picturebox.SizeMode = PictureBoxSizeMode.Zoom;
				toBeShown = new Bitmap(CoverPath);
				cover_picturebox.Image = toBeShown;
			}
		}

		private void generate_button_Click(object sender, EventArgs e)
		{
			if (TXTPath == null)
			{
				MessageBox.Show("没有源文件！请拖入TXT文件后重试！");
				return;
			}

			stopWatch = new Stopwatch();
			stopWatch.Start();
			
			/*** Load new TOC ***/
			loadNewTOC();

			/*** Load new Intro ***/
			Intro = cover_intro_textbox.Text;

			/*** Load settings ***/
			IniFile ini = loadINI(iniPath);
			bool vertical = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Vertical")));
			bool replace = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Replace")));
			bool StT = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "StT")));
			bool TtS = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "TtS")));
			if (StT && TtS)
			{
				StT = false;
				TtS = false;
			}
			int translation;		// 0: 不转; 1: 简转繁; 2: 繁转简
			if (StT && TtS)
			{
				translation = 0;
			}
			else if (StT && !TtS)
			{
				translation = 1;
				// 书名作者名简转繁
				String temp1 = bookAndAuthor[0];
				String temp2 = bookAndAuthor[1];
				bookAndAuthor.Clear();
				bookAndAuthor.Add(VB.Strings.StrConv(temp1, VB.VbStrConv.TraditionalChinese, 0));
				bookAndAuthor.Add(VB.Strings.StrConv(temp2, VB.VbStrConv.TraditionalChinese, 0));
			}
			else if (!StT && TtS)
			{
				translation = 2;
				// 书名作者名繁转简
				String temp1 = bookAndAuthor[0];
				String temp2 = bookAndAuthor[1];
				bookAndAuthor.Clear();
				bookAndAuthor.Add(VB.Strings.StrConv(temp1, VB.VbStrConv.SimplifiedChinese, 0));
				bookAndAuthor.Add(VB.Strings.StrConv(temp2, VB.VbStrConv.SimplifiedChinese, 0));
			}
			else translation = 0;
			String pageColor = ini.IniReadValue("Tab_2", "Page_Color");
			float marginT = float.Parse(ini.IniReadValue("Tab_2", "Page_Margin_Up"));
			float marginB = float.Parse(ini.IniReadValue("Tab_2", "Page_Margin_Down"));
			float marginL = float.Parse(ini.IniReadValue("Tab_2", "Page_Margin_Left"));
			float marginR = float.Parse(ini.IniReadValue("Tab_2", "Page_Margin_Right"));
			String titleFont = ini.IniReadValue("Tab_3", "Title_Font");
			float titleSize = float.Parse(ini.IniReadValue("Tab_3", "Title_Size"));
			String titleColor = ini.IniReadValue("Tab_3", "Title_Color");
			String bodyFont = ini.IniReadValue("Tab_3", "Body_Font");
			float bodySize = float.Parse(ini.IniReadValue("Tab_3", "Body_Size"));
			String bodyColor = ini.IniReadValue("Tab_3", "Body_Color");
			float lineHeight = float.Parse(ini.IniReadValue("Tab_3", "Line_Spacing"));
			bool addParagraphSpacing = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_3", "Add_Paragraph_Spacing")));
			String fileLocation = ini.IniReadValue("Tab_4", "Generated_File_Location");
			bool deleteTempFiles = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_4", "Delete_Temp_Files")));

			DocName = "《" + bookAndAuthor[0] + "》作者：" + bookAndAuthor[1];
			//HTMLPath = getHTMLPath();
			HTMLFolderPath = getHTMLFolderPath();
			CSSPath = getCSSPath();

			/*** Generate temp HTML ***/
			generateHTML(translation, vertical, replace, marginL, marginR, marginT, marginB, lineHeight, titleFont, titleColor, bodyFont, bodyColor);

			/*** Generate CSS ***/
			generateCSS(marginL, marginR, marginT, marginB, lineHeight, addParagraphSpacing, titleFont, titleColor, bodyFont, bodyColor, pageColor);

			/*** Image File ***/
			copyImageFile(bookAndAuthor[0], titleFont, titleSize, titleColor, bookAndAuthor[1], bodyFont, bodySize, bodyColor, pageColor, 1080, 1920);

			/*** Generate OPF ***/
			generateOPF();

			/*** Generate NCX ***/
			generateNCX();

			/*** Generate other files ***/
			generateOtherFiles();

			/*** ZIP ***/
			String startPath = getDocNameFolderPath();
			String zipPath = fileLocation + "\\" + DocName + ".epub";
			//ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.NoCompression, false);

			ZipFile zip = new ZipFile();
			zip.CompressionMethod = CompressionMethod.None;
			zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
			zip.AddFiles((System.IO.Directory.EnumerateFiles(getDocNameFolderPath())), false, "");
			zip.Save(zipPath);

			zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
			zip.AddDirectory(getHTMLFolderPath(), "OPS");
			zip.AddDirectory(getMETAFolderPath(), "META-INF");
			zip.Save(zipPath);

			//Extract("SimpleEpub", tempPath, "Resources", "zip.exe");

			this.status_label.Text = "生成完毕！文件：" + zipPath + " ；耗时：" + getProcessTime().ToString() + " 秒";
			notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
			showBalloonTip("温馨提示", DocName + ".epub" + "\n已生成完毕！");

			/*** Delete temp files ***/
			if (deleteTempFiles)
			{
				deletTempFiles();
			}
		}

		private void status_label_TextChanged(object sender, EventArgs e)
		{
			if (!this.status_label.Text.Contains("正在"))
			{
				timer.Enabled = true;
				timerCount = 15;
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			timerCount--;
			if (timerCount == 0) 
			{
				this.status_label.Text = defaultStatusText;
				timer.Stop();
			}
		}

		/*** Helper functions ***/
		private void loadNewTOC()
		{
			this.status_label.Text = "正在生成... 正在加载设置...";

			if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[1].Cells[0].Value != null)
			{
				// load new book name and author
				bookAndAuthor.Clear();
				bookAndAuthor.Add(TOC_list.Rows[0].Cells[0].Value.ToString());
				bookAndAuthor.Add(TOC_list.Rows[1].Cells[0].Value.ToString());

				// load new title line number
				if (TOC_list.Rows[2].Cells[0].Value != null && TOC_list.Rows[2].Cells[0].Value != null)
				{
					titleLineNumbers.Clear();
					for (int i = 2; i < TOC_list.Rows.Count; i++)
					{
						if (TOC_list.Rows[i].Cells[0].Value != null && TOC_list.Rows[i].Cells[1].Value != null)
						{
							titleLineNumbers.Add(Convert.ToInt32(TOC_list.Rows[i].Cells[1].Value));
						}
					}
				}
			}
		}

		private void generateCSS(float marginL, float marginR, float marginT, float marginB, float lineHeight, bool addParagraphSpacing, String titleFont, String titleColor, String bodyFont, String bodyColor, String pageColor)
		{
			StreamWriter sw = new StreamWriter(CSSPath, false, Encoding.UTF8);

			String background = "";
			if (String.Compare(pageColor, "羊皮纸") == 0)
			{
				Extract("SimpleEpub", getIMGFolderPath(), "Resources", "parchment.jpg");
				background = "background-image:url(\"..\\images\\parchment.jpg\");\n\tbackground-repeat: repeat;";
			}
			else
			{
				background = "background-color:" + pageColor + ";";
			}

			String body = "body {\n\tpadding: 0%;\n\tmargin-top: " + marginT + "%;\n\tmargin-bottom: " + marginB + "%;\n\tmargin-left: " + marginL + "%;\n\tmargin-right: " + marginR + "%;\n\tline-height:" + lineHeight + "%;\n\ttext-align: justify;\n\tfont-family:" + bodyFont + ";\n\tcolor:" + bodyColor + "\n\t" + background + "\n}\n";

			String div = "div {\n\tmargin:0px;\n\tpadding:0px;\n\tline-height:" + lineHeight + "%;\n\ttext-align: justify;\n\tfont-family:" + bodyFont + ";\n\tcolor:" + bodyColor + "\n}\n";

			int pMargin = (addParagraphSpacing ? 5 : 0);
			String p = "p {\n\ttext-align: justify;\n\ttext-indent: 2em;\n\tline-height:" + lineHeight + "%;\n\tmargin-top: " + pMargin + "pt;\n\tmargin-bottom: " + pMargin + "pt;\n}\n";

			String others = ".cover {\n\twidth:100%;\n\tpadding:0px;\n\t}\n.center {\n\ttext-align: center;\n\tmargin-left: 0%;\n\tmargin-right: 0%;\n}\n.left {\n\ttext-align: left;\n\tmargin-left: 0%;\n\tmargin-right: 0%;\n}\n.right {\n\ttext-align: right;\n\tmargin-left: 0%;\n\tmargin-right: 0%;\n}\n.quote {\n\tmargin-top: 0%;\n\tmargin-bottom: 0%;\n\tmargin-left: 1em;\n\tmargin-right: 1em;\n\ttext-align: justify;\n\tfont-family:" + bodyFont + ";\n\tcolor:" + bodyColor + "\n}\n";

			String headers = "h1 {\n\tline-height:" + lineHeight + "%;\n\ttext-align: center;\n\tfont-weight:bold;\n\tfont-size:xx-large;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh2 {\n\tline-height:" + lineHeight + "%;\n\ttext-align: center;\n\tfont-weight:bold;\n\tfont-size:x-large;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh3 {\n\tline-height:" + lineHeight + "%;\n\ttext-align: center;\n\tfont-weight:bold;\n\tfont-size:large;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh4 {\n\tline-height:" + lineHeight + "%;\n\ttext-align: center;\n\tfont-weight:bold;\n\tfont-size:medium;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh5 {\n\tline-height:" + lineHeight + "%;\n\ttext-align: center;\n\tfont-weight:bold;\n\tfont-size:small;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh6 {\n\tline-height:" + lineHeight + "%;\n\ttext-align: center;\n\tfont-weight:bold;\n\tfont-size:x-small;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\n";

			sw.WriteLine(body + div + p + others + headers);
			sw.Close();
		}

		private void generateOPF()
		{
			StreamWriter sw = new StreamWriter(getOPFPath(), false, Encoding.UTF8);

			String head = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<package version=\"2.0\" unique-identifier=\"PrimaryID\" xmlns=\"http://www.idpf.org/2007/opf\">\n<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">\n<dc:title>" + bookAndAuthor[0] + "</dc:title>\n<dc:identifier opf:scheme=\"ISBN\"></dc:identifier>\n<dc:language>zh-CN</dc:language>\n<dc:creator opf:role=\"aut\">" + bookAndAuthor[1] + "</dc:creator>\n<dc:publisher></dc:publisher>\n<dc:description>" + Intro + "</dc:description>\n<dc:coverage></dc:coverage>\n<dc:source></dc:source>\n<dc:date></dc:date>\n<dc:rights></dc:rights>\n<dc:subject></dc:subject>\n<dc:contributor></dc:contributor>\n<dc:type>[type]</dc:type>\n<dc:format></dc:format>\n<dc:relation></dc:relation>\n<dc:builder></dc:builder>\n<dc:builder_version></dc:builder_version>\n<meta name=\"cover\" content=\"cover-image\"/>\n</metadata>\n<manifest>\n<!-- Content Documents -->\n<item id=\"main-css\" href=\"css/main.css\" media-type=\"text/css\"/>\n";

			String body1 = "";

			for (int i = 1; i <= chapterNumber; i ++)
			{
				String temp1 = "<item id=\"chapter" + i + "\"  href=\"chapter" + i + ".html\"  media-type=\"application/xhtml+xml\"/>\n";
				body1 += temp1;
			}

			// <spine page-progression-direction="rtl">
			String body2 = "\n<item id=\"ncx\"  href=\"fb.ncx\" media-type=\"application/x-dtbncx+xml\"/>\n<item id=\"css\" href=\"css/main.css\" media-type=\"text/css\"/>\n<item id=\"cover-image\" href=\"images/cover.jpg\" media-type=\"image/jpeg\"/>\n</manifest>\n\n<spine toc=\"ncx\">\n";

			String body3 = "";
			for (int i = 1; i <= chapterNumber; i ++)
			{
				// <itemref idref="chapter1" linear="yes" properties="duokan-page-fullscreen"/>
				String temp3 = "<itemref idref=\"chapter" + i + "\" linear=\"yes\"/>\n";
				body3 += temp3;
			}

			String foot = "\n</spine>\n<guide>\n\n</guide>\n</package>";

			sw.WriteLine(head + body1 + body2 + body3 + foot);
			sw.Close();
		}

		private void generateNCX()
		{
			StreamWriter sw = new StreamWriter(getNCXPath(), false, Encoding.UTF8);

			String head = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE ncx PUBLIC\"-//NISO//DTD ncx 2005-1//EN\"\"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\">\n<ncx version=\"2005-1\"xml:lang=\"en-US\"xmlns=\"http://www.daisy.org/z3986/2005/ncx/\">\n<head>\n\t<!-- The following four metadata items are required for all NCX documents, including those conforming to the relaxed constraints of OPS 2.0 -->\n\t<meta name=\"dtb:uid\" content=\" \"/>\n\t<meta name=\"dtb:depth\" content=\"1\"/>\n\t<meta name=\"dtb:totalPageCount\" content=\"0\"/>\n\t<meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n</head>\n<docTitle><text>" + bookAndAuthor[0] + "</text></docTitle>\n<docAuthor><text>" + bookAndAuthor[1] + "</text></docAuthor>\n<navMap>\n";

			String body = "";
			MessageBox.Show(chapterNumber.ToString());
			for (int i = 1; i <= chapterNumber; i ++)
			{
				String temp = "<navPoint id=\"chapter" + i + "\" playOrder=\"" + i + "\">\n<navLabel><text>" + TOC_list.Rows[i+1].Cells[0].Value + "</text></navLabel>\n<content src=\"chapter" + i + ".html\"/>\n</navPoint>\n";
				body += temp;
			}

			String foot = "</navMap>\n</ncx>";

			sw.WriteLine(head + body + foot);
			sw.Close();
		}

		private void generateHTML(int translation, bool vertical, bool replace, float marginL, float marginR, float marginT, float marginB, float lineHeight, String titleFont, String titleColor, String bodyFont, String bodyColor)
		{
			this.status_label.Text = "正在生成... 正在生成临时HTML文件...";

			StreamReader sr = new StreamReader(TXTPath, Encoding.GetEncoding("GB2312"));
			StreamWriter sw;

			String nextLine;
			int TXTlineNumber = 1;
			int TLN_idx = 0;
			int TLN_size = titleLineNumbers.Count;

			var vbTranslation = VB.VbStrConv.None;
			switch (translation)
			{
				case 0:
					vbTranslation = VB.VbStrConv.None;
					break;
				case 1:
					vbTranslation = VB.VbStrConv.TraditionalChinese;
					break;
				case 2:
					vbTranslation = VB.VbStrConv.SimplifiedChinese;
					break;
				default:
					vbTranslation = VB.VbStrConv.None;
					break;
			}

			bool sameChapter = false;
			String toPrint = "";
			chapterNumber = 1;
			while (true)
			{
				sw = new StreamWriter(getHTMLPath(chapterNumber), false, Encoding.UTF8);

				if (toPrint != "")
				{
					sw.WriteLine(toPrint);
					sameChapter = true;
					toPrint = "";
				}

				while ((nextLine = sr.ReadLine()) != null)
				{
					Match emptyLine = Regex.Match(nextLine, "^\\s*$");
					if (!emptyLine.Success)		// Remove empty lines
					{
						nextLine = nextLine.Trim();
						nextLine = VB.Strings.StrConv(nextLine, vbTranslation, 0);		// 简繁转换

						if (vertical)		// 半角字符转全角
						{
							nextLine = ToSBC(nextLine);
							//nextLine = VB.Strings.StrConv(nextLine, VB.VbStrConv.Wide);
							//nextLine = nextLine.Replace("—", "<span lang=EN-US style='font-family:\"Times New Roman\"'>—</span>");
						}

						if (TLN_idx < TLN_size && TXTlineNumber == titleLineNumbers[TLN_idx])		// Chapter titles!
						{
							if (replace)		// 替换标题中的数字为汉字
							{
								nextLine = numberToHan(nextLine);
							}
							
							if (sameChapter)
							{
								sameChapter = false;
								toPrint = (HTMLHead(nextLine));
								TLN_idx++;
								TXTlineNumber++;
								break;
							}
							else
							{
								sw.WriteLine(HTMLHead(nextLine));
								sameChapter = true;
								TLN_idx++;
							}
						}
						else
						{
							sw.WriteLine("<p>" + nextLine + "</p>");
						}
					}
					TXTlineNumber++;
				}

				if (nextLine != null)
				{
					sw.WriteLine("</div>\n</body>\n</html>");
					sw.Close();
					chapterNumber++;
				}
				else
				{
					sw.Close();
					break;
				}
			}
			
			sr.Close();
		}

		private String HTMLHead(String chapterTitle)
		{
			return "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n<link rel=\"stylesheet\" type=\"text/css\" href=\"css/main.css\"/>\n<title>" + chapterTitle +"</title>\n</head>\n<body>\n<div>\n<h2>" + chapterTitle + "</h2>";
		}

		private void copyImageFile(String bookname, String bookfont, float bookfontsize, String bookcolor, String author, String authorfont, float authorfontsize, String authorcolor, String pagecolor, int width, int height)
		{
			if (CoverPath == null)
			{
				Image cover = DrawText(bookname, bookfont, bookfontsize, bookcolor, author, authorfont, authorfontsize, authorcolor, pagecolor, width, height);
				CoverPath = tempPath + "\\cover.jpg";
				cover.Save(CoverPath, System.Drawing.Imaging.ImageFormat.Jpeg);
				cover.Dispose();
				String IMGFolderPath = getIMGFolderPath();
				String newPath = IMGFolderPath + "\\cover.jpg";
				if (File.Exists(newPath)) System.IO.File.Delete(newPath);
				System.IO.File.Move(CoverPath, newPath);
				CoverPath = null;
			}
			else
			{
				String IMGFolderPath = getIMGFolderPath();
				if (IMGFolderPath.Contains(tempPath))
				{
					System.IO.File.Move(CoverPath, IMGFolderPath + "\\cover.jpg");
				}
				else
				{
					System.IO.File.Copy(CoverPath, IMGFolderPath + "\\cover.jpg", true);
				}
			}
		}

		private Image DrawText(String bookname, String bookfont, float bookfontsize, String bookcolor, String author, String authorfont, float authorfontsize, String authorcolor, String pagecolor, int width, int height)
		{
			//first, create a dummy bitmap just to get a graphics object
			Image img = new Bitmap(width, height);
			
			Graphics drawing = Graphics.FromImage(img);

			//paint the background
			drawing.Clear(convertHTMLColorToDrawColor(pagecolor, 0));

			//create a brush for the text
			//float booknameWidth = (float)bookname.Length * bookfontsize * 5;
			float booknameWidth = TextRenderer.MeasureText(bookname, new Font(bookfont, bookfontsize * 5, FontStyle.Bold)).Width;
			float booknameHeight = TextRenderer.MeasureText(bookname, new Font(bookfont, bookfontsize * 5, FontStyle.Bold)).Height;
			float booknamePosX = 1080 / 2 - booknameWidth / 2;
			float booknamePosY = 1920 / (float)3.5 - booknameHeight / 2;
			Brush textBrush1 = new SolidBrush(convertHTMLColorToDrawColor(bookcolor, 1));
			drawing.DrawString(bookname, new Font(bookfont, bookfontsize * 5, FontStyle.Bold), textBrush1, booknamePosX, booknamePosY);

			Brush lineBrush = new SolidBrush(convertHTMLColorToDrawColor(bookcolor, 1));
			drawing.DrawLine(new Pen(lineBrush), new Point(0, (int)(booknamePosY + booknameHeight)), new Point(1080, (int)(booknamePosY + booknameHeight)));

			float authorWidth = TextRenderer.MeasureText(author, new Font(authorfont, authorfontsize * 4, FontStyle.Bold)).Width;
			float authorHeight = TextRenderer.MeasureText(author, new Font(authorfont, authorfontsize * 4, FontStyle.Bold)).Height;
			float authorPosX = 1080 / 2 + authorWidth / 3;
			float authorPosY = 1920 / 3 + authorHeight;
			Brush textBrush2 = new SolidBrush(convertHTMLColorToDrawColor(authorcolor, 2));
			drawing.DrawString(author, new Font(authorfont, authorfontsize * 4, FontStyle.Bold), textBrush2, authorPosX, authorPosY);

			drawing.Save();

			textBrush1.Dispose();
			lineBrush.Dispose();
			textBrush2.Dispose();
			drawing.Dispose();

			return img;
		}

		private void generateOtherFiles()
		{
			// container.xml
			StreamWriter sw = new StreamWriter(getContainerPath(), false, Encoding.UTF8);
			sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n\t<rootfiles>\n\t\t<rootfile full-path=\"OPS/fb.opf\" media-type=\"application/oebps-package+xml\"/>\n\t</rootfiles>\n</container>");
			sw.Close();
			
			// mimetype
			sw = new StreamWriter(getMIMEPath(), false, Encoding.UTF8);
			sw.WriteLine("application/epub+zip");
			sw.Close();
		}

		private void deletTempFiles()
		{
			if (File.Exists(getTOCPath(iniPath)))
			{
				File.Delete(getTOCPath(iniPath));
			}
			if (File.Exists(tempPath + "\\zip.exe"))
			{
				File.Delete(tempPath + "\\zip.exe");
			}
			if (Directory.Exists(getDocNameFolderPath()))
			{
				Directory.Delete(getDocNameFolderPath(), true);
			}
		}

		private double getProcessTime()
		{
			stopWatch.Stop();
			long duration = stopWatch.ElapsedMilliseconds;
			return (duration / 1000d);
		}

		private void showBalloonTip(String title, String text)
		{
			notifyIcon1.BalloonTipTitle = title;
			notifyIcon1.BalloonTipText = text;
			notifyIcon1.ShowBalloonTip(1000);
		}

		private String getTOCPath(String iniPath)
		{
			IniFile ini = loadINI(iniPath);
			return ini.IniReadValue("Tab_4", "Generated_File_Location") + "\\目录.txt";
		}

		private String getDocNameFolderPath()
		{
			IniFile ini = loadINI(iniPath);

			string subPath = ini.IniReadValue("Tab_4", "Generated_File_Location") + "\\" +DocName;
			bool isExists = System.IO.Directory.Exists(subPath);
			if (!isExists)
				System.IO.Directory.CreateDirectory(subPath);

			return subPath;
		}

		private String getHTMLPath(int flag)
		{
			IniFile ini = loadINI(iniPath);
			return getHTMLFolderPath() + "\\chapter" + flag.ToString() + ".html";
		}

		private String getHTMLFolderPath()
		{
			IniFile ini = loadINI(iniPath);

			string subPath = getDocNameFolderPath() + "\\OPS";
			bool isExists = System.IO.Directory.Exists(subPath);
			if (!isExists)
				System.IO.Directory.CreateDirectory(subPath);

			return subPath;
		}

		private String getOPFPath()
		{
			return getHTMLFolderPath() + "\\fb.opf";
		}

		private String getNCXPath()
		{
			return getHTMLFolderPath() + "\\fb.ncx";
		}

		private String getCSSPath()
		{
			IniFile ini = loadINI(iniPath);

			string subPath = getDocNameFolderPath() + "\\OPS\\css";
			bool isExists = System.IO.Directory.Exists(subPath);
			if (!isExists)
				System.IO.Directory.CreateDirectory(subPath);

			return subPath + "\\main.css";
		}

		private String getIMGFolderPath()
		{
			IniFile ini = loadINI(iniPath);

			string subPath = getDocNameFolderPath() + "\\OPS\\images";
			bool isExists = System.IO.Directory.Exists(subPath);
			if (!isExists)
				System.IO.Directory.CreateDirectory(subPath);

			return subPath;
		}

		private String getMETAFolderPath()
		{
			IniFile ini = loadINI(iniPath);

			string subPath = getDocNameFolderPath() + "\\META-INF";
			bool isExists = System.IO.Directory.Exists(subPath);
			if (!isExists)
				System.IO.Directory.CreateDirectory(subPath);

			return subPath;
		}

		private String getContainerPath()
		{
			return getMETAFolderPath() + "\\container.xml";
		}

		private String getMIMEPath()
		{
			IniFile ini = loadINI(iniPath);
			return getDocNameFolderPath() + "\\mimetype";
		}

		private void setCellFontColor(Color a, Color b)
		{
			TOC_list.Columns[0].DefaultCellStyle.ForeColor = a;
			TOC_list.Columns[1].DefaultCellStyle.ForeColor = b;
		}

		private List<String> getBookNameAndAuthorInfo(String path, String filename)
		{
			List<String> result = new List<String>();

			filename = filename.Trim();
			// 1. get info from file name
			int pos = filename.IndexOf("作者：");
			if (pos == -1)
				pos = filename.IndexOf("作者:");
			if (pos != -1)
			{
				String bookname = filename.Substring(0, pos);
				char[] charsToTrim = { '《', '》' };
				bookname = bookname.Trim(charsToTrim);
				bookname = bookname.Replace("书名：", "");
				bookname = bookname.Replace("书名:", "");
				bookname = bookname.Trim();
				String author = filename.Substring(filename.IndexOf("作者：") + 3, filename.Length - filename.IndexOf("作者：") - 3);
				author = author.Trim();

				result.Add(bookname);
				result.Add(author);

				return result;
			}
			else
			{
				// No complete book name and author info
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
				showBalloonTip("文件名中无完整书名和作者信息！", "完整信息范例：“《书名》作者：作者名”`n（文件名中一定要有“作者”二字，书名号可以省略）`n`n开始检测文件第一、二行……`n其中第一行为书名，第二行为作者名。！");

				// Treat first line as book name and second line as author
				StreamReader sr = new StreamReader(path, Encoding.GetEncoding("GB2312"));
				String bookname = sr.ReadLine();
				if (bookname == null) bookname = filename;
				else
				{
					char[] charsToTrim = { '《', '》' };
					bookname = bookname.Trim(charsToTrim);
					bookname = bookname.Replace("书名：", "");
					bookname = bookname.Replace("书名:", "");
					bookname = bookname.Trim();
				}

				String author = sr.ReadLine();
				if (author == null) author = "东皇";
				else
				{
					author = author.Replace("作者：", "");
					author = author.Replace("作者:", "");
					author = author.Trim();
				}

				result.Add(bookname);
				result.Add(author);

				return result;
			}
		}

		private String getIntroInfo(String path)
		{
			String result = "";

			StreamReader sr = new StreamReader(path, Encoding.GetEncoding("GB2312"));
			String line = "";

			// Find first title line number
			int firstTitleLineNumber = 1;
			while ((line = sr.ReadLine()) != null)
			{
				Match title = Regex.Match(line, regex);

				// First chapter title found!
				if (title.Success)
				{
					break;
				}
				firstTitleLineNumber++;
			}

			sr.DiscardBufferedData(); 
			sr.BaseStream.Seek(0, SeekOrigin.Begin); 
			sr.BaseStream.Position = 0;

			int introLineNumber = 1;
			for (int i = 0; i < firstTitleLineNumber; i++)
			{
				line = sr.ReadLine();
				if (line != null && line.Contains("简介"))
				{
					for (int j = introLineNumber+1; j < firstTitleLineNumber; j++)
					{
						line = sr.ReadLine();
						result += line.Trim();
					}
				}
				introLineNumber++;
			}

			return result.Trim();
		}

		private static String numberToHan(String nextLine)
		{
			nextLine = nextLine.Replace("0", "零");
			nextLine = nextLine.Replace("1", "一");
			nextLine = nextLine.Replace("2", "二");
			nextLine = nextLine.Replace("3", "三");
			nextLine = nextLine.Replace("4", "四");
			nextLine = nextLine.Replace("5", "五");
			nextLine = nextLine.Replace("6", "六");
			nextLine = nextLine.Replace("7", "七");
			nextLine = nextLine.Replace("8", "八");
			nextLine = nextLine.Replace("9", "九");
			nextLine = nextLine.Replace("０", "零");
			nextLine = nextLine.Replace("１", "一");
			nextLine = nextLine.Replace("２", "二");
			nextLine = nextLine.Replace("３", "三");
			nextLine = nextLine.Replace("４", "四");
			nextLine = nextLine.Replace("５", "五");
			nextLine = nextLine.Replace("６", "六");
			nextLine = nextLine.Replace("７", "七");
			nextLine = nextLine.Replace("８", "八");
			nextLine = nextLine.Replace("９", "九");
			return nextLine;
		}

		private System.Drawing.Color convertHTMLColorToDrawColor(String HTMLColor, int flag)
		{
			System.Drawing.Color color = new Color();
			try
			{
				color = ColorTranslator.FromHtml(HTMLColor);
			}
			catch
			{

				IniFile ini = loadINI(iniPath);
				if (flag == 0)
				{
					color = ColorTranslator.FromHtml("white");
					ini.IniWriteValue("Tab_2", "Page_Color", "white");
				}
				else if (flag == 1)
				{
					color = ColorTranslator.FromHtml("black");
					ini.IniWriteValue("Tab_3", "Title_Color", "black");
				}
				else if (flag == 2)
				{
					color = ColorTranslator.FromHtml("black");
					ini.IniWriteValue("Tab_3", "Body_Color", "black");
				}
				else
					MessageBox.Show("Wrong position flag!");
			}
			return color;
		}

		private Word.WdColor convertHTMLColorToWdColor(String HTMLColor, int flag)		// flag == 0: page color; flag == 1: title color; flag == 2: body color
		{
			System.Drawing.Color color = convertHTMLColorToDrawColor(HTMLColor, flag);
			int rgbColor = VB.Information.RGB(color.R, color.G, color.B);
			Word.WdColor wdColor = (Word.WdColor)rgbColor;
			return wdColor;
		}

		private static String ToSBC(String input)
		{
			// 半角转全角：
			char[] c = input.ToCharArray();
			for (int i = 0; i < c.Length; i++)
			{
				if (c[i] == 32)
				{
					c[i] = (char)12288;
					continue;
				}
				if (c[i] < 127)
					c[i] = (char)(c[i] + 65248);
			}
			return new String(c);
		}

		private static void Extract(String nameSpace, String outDirectory, String internalFilePath, String resourceName)
		{
			Assembly assembly = Assembly.GetCallingAssembly();

			using (Stream s = assembly.GetManifestResourceStream(nameSpace + "." + (internalFilePath == "" ? "" : internalFilePath + ".") + resourceName))
				using (BinaryReader r = new BinaryReader(s))
					using (FileStream fs = new FileStream(outDirectory + "\\" + resourceName, FileMode.OpenOrCreate))
						using (BinaryWriter w = new BinaryWriter(fs))
							w.Write(r.ReadBytes((int)s.Length));
		}

    }
}
