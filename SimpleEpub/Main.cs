using Ini;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using VB = Microsoft.VisualBasic;
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
		#region Variables

		private static Settings settingsForm = new Settings();		// Able to get data from Settings form

		private static String tempPath = Path.Combine(Path.GetTempPath(), "SimpleEpub");
		private static String resourcesPath = Path.Combine(Path.GetTempPath(), "SimpleEpub") + "\\Resources";
		private static String defaultStatusText = "SimpleEpub 版本: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		
		//String regex = "^([\\s\t　]{0,20}(正文[\\s\t　]{0,4})?[第【]([——-——一二两三四五六七八九十○零百千0-9０-９]{1,12}).*[章节節回集卷部】].*?$)|(Ui)(第.{1,5}章)|(Ui)(第.{1,5}节)";
		//String regex = "^([\\s\t　]*(正文[\\s\t　]*)?[第【][\\s\t　]*([——-——一二两三四五六七八九十○零百千壹贰叁肆伍陆柒捌玖拾佰仟0-9０-９]*)[\\s\t　]*[章节節回集卷部】][\\s\t　]*.{0,40}?$)|(Ui)(第.{1,5}章)|(Ui)(第.{1,5}节)";
		//String regex = "^([\\s\t　]*([【])?(正文[\\s\t　]*)?[第【][\\s\t　]*([——-——一二两三四五六七八九十○零百千壹贰叁肆伍陆柒捌玖拾佰仟0-9０-９]*)[\\s\t　]*[章节節回集卷部】][\\s\t　]*.{0,40}?$)";
		private static String regex = "^([\\s\t　]*([【])?(正文[\\s\t　]*)?[第【][\\s\t　]*([——-——一二两三四五六七八九十○零百千壹贰叁肆伍陆柒捌玖拾佰仟0-9０-９\\s\t　/\\、、]*)[\\s\t　]*[章节節回集卷部】][\\s\t　]*.{0,40}?$)";

		private List<String> bookAndAuthor;
		private int FSLinesScanned = 0;
		private bool extraLinesInBeginning = false;
		private bool extraLinesNotEmpty = false;
		private List<int> titleLineNumbers = new List<int>();
		private List<Tuple<String, String>> pictureHTMLs = new List<Tuple<String, String>>();
		private String TXTPath;
		private String CoverPath;
		private String CoverPathSlim;
		private String origCover;
		private bool coverChanged = true;
		private String DocName;
		private String Intro;
		private int chapterNumber = 1;
		private bool parchmentNeeded = false;
		private List<String> embedFontPaths = new List<String>();
		private HashSet<Char> ALLTITLETEXT = new HashSet<Char>();
		private HashSet<Char> ALLBODYTEXT = new HashSet<Char>();
		private ICollection<UInt16> IndexT = new List<UInt16>();
		private ICollection<UInt16> IndexB = new List<UInt16>();
		private String titleFontPath;
		private String bodyFontPath;
		private GlyphTypeface glyphTypefaceT;
		private GlyphTypeface glyphTypefaceB;
		private Uri URIT;
		private Uri URIB;

		private String mimetype;
		private String container;
		private StringBuilder css = new StringBuilder();
		private StringBuilder opf = new StringBuilder();
		private StringBuilder ncx = new StringBuilder();
		private StringBuilder coverHtml = new StringBuilder();
		private List<String> picHtmlList = new List<String>();
		private List<String> txtHtmlList = new List<String>();

		private Stopwatch stopWatch;
		private int timerCount;

		private Image toBeShown;

		private bool DONE = false;

		#endregion

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
			// 设置为中文环境
			Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("zh-CN");

			((Control)cover_picturebox).AllowDrop = true;

			this.status_label.Text = defaultStatusText;
			Directory.CreateDirectory(tempPath);
			Directory.CreateDirectory(tempPath + "\\Resources");
			loadINI();
			try
			{
				loadCurSettings(settingsForm);
			}
			catch
			{
				MessageBox.Show("加载设置文件出错，即将导入默认设置！");
				writeDefaultSettings();
				loadCurSettings(settingsForm);
			}

		}

		#region Event Handlers

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

				String TOCPath = getTOCPath();
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
			String TOCPath = getTOCPath();
			if (File.Exists(TOCPath))
			{
				using (FileStream fs = File.Open(TOCPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (BufferedStream bs = new BufferedStream(fs))
				using (StreamReader sr = new StreamReader(TOCPath, Encoding.GetEncoding("GB2312")))
				{
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
					setCellFontColor(System.Drawing.Color.Black, System.Drawing.Color.RoyalBlue);

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
				}

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
			Extract("SimpleEpub", resourcesPath, "Resources", "help.pdf");
			try
			{
				System.Diagnostics.Process.Start(resourcesPath + "\\help.pdf");
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

			String[] TOCGrid = (String[])e.Data.GetData(DataFormats.FileDrop);
			if (TOCGrid.Length != 1)
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("只能拖拽一个文件！");
				e.Effect = DragDropEffects.None;
			}
			else if (!TOCGrid[0].ToLower().EndsWith(".txt"))
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("拖拽进来的不是TXT文件！");
				e.Effect = DragDropEffects.None;
			}
			else
			{
				stopWatch = new Stopwatch();
				stopWatch.Start();

				loadINI();
				if (Convert.ToInt32(ini.IniReadValue("Tab_4", "Drag_Clear_List")) == 1)		//拖入文件时清空列表
				{
					if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[0].Cells[1].Value != null)
					{
						TOC_list.Rows.Clear();
					}
				}

				// Save txt file path
				TXTPath = TOCGrid[0];

				// Set cell font colors
				setCellFontColor(System.Drawing.Color.Black, System.Drawing.Color.RoyalBlue);

				// Get file name
				String filename = Path.GetFileNameWithoutExtension(TXTPath);

				// Get book name and author info to fill the first two rows of TOC_list
				bookAndAuthor = getBookNameAndAuthorInfo(TXTPath, filename);

				// Fill author and bookname textboxes on the right side
				cover_bookname_textbox.Text = bookAndAuthor[0];
				cover_author_textbox.Text = bookAndAuthor[1];

				// Fill intro textbox on the right side
				Intro = getIntroInfo(TXTPath);
				if (Intro.Contains("\n"))
					Intro = Intro.Replace("\n", "\r\n");
				cover_intro_textbox.Text = Intro;

				// Prepare a list of title line numbers
				using (FileStream fs = File.Open(TXTPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (BufferedStream bs = new BufferedStream(fs))
				using (StreamReader sr = new StreamReader(TXTPath, Encoding.GetEncoding("GB2312")))
				{
					String nextLine;
					int lineNumber = 1;
					int rowNumber = 0;
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
				}

				this.status_label.Text = "目录提取完毕！耗时：" + getProcessTime().ToString() + " 秒";
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
				showBalloonTip("温馨提示", "目录提取完毕！");

				// clear CoverPath
				CoverPath = null;
				cover_picturebox.Image = null;
				if (toBeShown != null)
					toBeShown.Dispose();

				// generate temp cover
				bool vertical = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Vertical")));
				String bookNameFont = ini.IniReadValue("Tab_2", "Cover_BookName_Font");
				String authorNameFont = ini.IniReadValue("Tab_2", "Cover_AuthorName_Font");

				CoverPath = tempPath + "\\cover.jpg";
				CoverPathSlim = tempPath + "\\cover~slim.jpg";

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
				if (File.Exists(CoverPathSlim))
				{
					try
					{
						File.Delete(CoverPathSlim);
					}
					catch
					{
						if (File.Exists(CoverPathSlim))
							MessageBox.Show("Deletion failed");
					}
				}

				using (Image cover = DrawText(1536, 2048, vertical, bookNameFont, authorNameFont))
				{
					try
					{
						//cover.Save(CoverPath, System.Drawing.Imaging.ImageFormat.Jpeg);
						SaveJpeg(CoverPath, cover, 100);
					}
					catch
					{
						MessageBox.Show("coverpath: " + CoverPath);
					}
				}
				using (Image coverSlim = DrawText(1080, 1920, vertical, bookNameFont, authorNameFont))
				{
					try
					{
						//coverSlim.Save(CoverPathSlim, System.Drawing.Imaging.ImageFormat.Jpeg);
						SaveJpeg(CoverPathSlim, coverSlim, 100);
					}
					catch
					{
						MessageBox.Show("coverpathslim: " + CoverPathSlim);
					}
				}
				cover_picturebox.SizeMode = PictureBoxSizeMode.Zoom;
				//toBeShown = new Bitmap(CoverPath);
				toBeShown = Image.FromFile(CoverPath);
				cover_picturebox.Image = toBeShown;
			}
			coverChanged = true;
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
			coverChanged = true;

			String[] coverBox = (String[])e.Data.GetData(DataFormats.FileDrop);
			if (coverBox.Length != 1)
			{
				this.status_label.Text = defaultStatusText;
				MessageBox.Show("只能拖拽一个文件！");
				e.Effect = DragDropEffects.None;
			}
			else if (!coverBox[0].ToLower().EndsWith(".jpg") && !coverBox[0].ToLower().EndsWith(".jpeg"))
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
				if (File.Exists(CoverPathSlim))
				{
					try
					{
						File.Delete(CoverPathSlim);
					}
					catch
					{
						if (File.Exists(CoverPathSlim))
							MessageBox.Show("Deletion failed");
					}
				}
				
				// generate cover.jpg and cover~slim.jpg
				origCover = coverBox[0];
				generateCoverFromFilePath();
			}
		}

		private void generate_button_Click(object sender, EventArgs e)
		{
			picHtmlList.Clear();
			txtHtmlList.Clear();
			embedFontPaths.Clear();
			ALLBODYTEXT.Clear();
			ALLTITLETEXT.Clear();
			IndexT.Clear();
			IndexB.Clear();
			glyphTypefaceT = new GlyphTypeface();
			glyphTypefaceB = new GlyphTypeface();
			coverHtml.Clear();
			css.Clear();
			opf.Clear();
			ncx.Clear();

			if (TXTPath == null)
			{
				MessageBox.Show("没有源文件！请拖入TXT文件后重试！");
				return;
			}

			this.status_label.Text = "正在生成...";

			stopWatch = new Stopwatch();
			stopWatch.Start();

			if (DONE)
			{
				if (!coverChanged)
				{
					deleteAllTempFiles();
				}
				DONE = false;
			}
			if (origCover != null && !File.Exists(CoverPath) && !File.Exists(CoverPathSlim))
			{
				generateCoverFromFilePath();
			}

			/*** Load new TOC ***/
			bool validTOC = loadNewTOC();
			if (!validTOC) return;

			/*** Load new Intro ***/
			Intro = cover_intro_textbox.Text;
			if (Intro != "")
			{
				Intro = "&lt;span class=\"Apple-style-span\"&gt;" + Intro + "&lt;/span&gt;";
				if (Intro.Contains("\r\n"))
				{
					Intro = Intro.Replace("\r\n", "&lt;/span&gt;&lt;br/&gt;&lt;span class=\"Apple-style-span\"&gt;");
				}
			}

			/*** Load settings ***/
			loadINI();
			bool coverFirstPage = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Cover_FirstPage")));
			bool coverNoTOC = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Cover_NoTOC")));
			bool vertical = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Vertical")));
			bool replace = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Replace")));
			bool StT = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "StT")));
			bool TtS = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "TtS")));
			if (StT && TtS)
			{
				StT = false;
				TtS = false;
			}
			int translation = translationState(StT, TtS);
			String temp1 = bookAndAuthor[0];
			String temp2 = bookAndAuthor[1];
			bookAndAuthor.Clear();
			bookAndAuthor.Add(translate(temp1, translation));
			bookAndAuthor.Add(translate(temp2, translation));
			bool embedFontSubset = Convert.ToBoolean(Convert.ToInt32(ini.IniReadValue("Tab_1", "Embed_Font_Subset")));
			String bookNameFont = ini.IniReadValue("Tab_2", "Cover_BookName_Font");
			String authorNameFont = ini.IniReadValue("Tab_2", "Cover_AuthorName_Font");
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

			if (embedFontSubset)
			{
				titleFontPath = FontNameFile.getFontFileName(titleFont);
				if (titleFontPath.CompareTo("") == 0)
					MessageBox.Show("指定的标题字体不是TTF字体，无法嵌入！");
				else
				{
					URIT = new Uri(titleFontPath);
					glyphTypefaceT = new GlyphTypeface(URIT);
				}
				bodyFontPath = FontNameFile.getFontFileName(bodyFont);
				if (bodyFontPath.CompareTo("") == 0)
					MessageBox.Show("指定的正文字体不是TTF字体，无法嵌入！");
				else
				{
					URIB = new Uri(bodyFontPath);
					glyphTypefaceB = new GlyphTypeface(URIB);
				}
			}

			/*** Generate temp HTML ***/
			bool HTML = generateHTML(coverFirstPage, translation, vertical, replace, embedFontSubset);
			if (!HTML) return;

			/*** Generate CSS ***/
			generateCSS(vertical, marginL, marginR, marginT, marginB, lineHeight, addParagraphSpacing, titleFont, titleColor, bodyFont, bodyColor, pageColor, embedFontSubset);
			
			/*** Image File ***/
			bool IMG = copyImageFile(vertical, bookNameFont, authorNameFont);
			if (!IMG) return;
			/*if (!coverFirstPage)
				if (File.Exists(getIMGFolderPath() + "\\cover~slim.jpg"))
					File.Delete(getIMGFolderPath() + "\\cover~slim.jpg");*/

			/*** Generate OPF ***/
			generateOPF(coverFirstPage, translation, vertical, embedFontSubset);

			/*** Generate NCX ***/
			generateNCX(coverFirstPage, coverNoTOC, translation);

			/*** Generate other files ***/
			mimetype = "application/epub+zip";
			container = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n\t<rootfiles>\n\t\t<rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\" />\n\t</rootfiles>\n</container>";

			this.status_label.Text = "正在生成... 正在生成Epub文件...";

			/*** ZIP ***/
			DocName = "《" + bookAndAuthor[0] + "》作者：" + bookAndAuthor[1];
			String zipPath = fileLocation + "\\" + DocName + ".epub";
			
			using (ZipFile zip = new ZipFile())
			{
				zip.EmitTimesInWindowsFormatWhenSaving = false;					// Exclude extra attribute (timestamp); same as -X command
				zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;		// Ensure mimetype file is NOT compressed
				zip.Encryption = EncryptionAlgorithm.None;						// Ensure mimetype file is NOT encryped
				zip.AddEntry("mimetype", mimetype, Encoding.ASCII);		// yet another way to generate flawless mimetype file
				zip.Save(zipPath);

				zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
				zip.AddDirectoryByName("META-INF");
				zip.AddEntry("META-INF\\container.xml", container, Encoding.UTF8);
				zip.AddDirectoryByName("OEBPS");
				zip.AddDirectoryByName("OEBPS\\Images");
				zip.AddDirectoryByName("OEBPS\\Styles");
				zip.AddDirectoryByName("OEBPS\\Text");
				zip.AddEntry("OEBPS\\Styles\\main.css", css.ToString(), Encoding.UTF8);
				zip.AddEntry("OEBPS\\content.opf", opf.ToString(), Encoding.UTF8);
				zip.AddEntry("OEBPS\\toc.ncx", ncx.ToString(), Encoding.UTF8);
				if (coverFirstPage) zip.AddEntry("OEBPS\\Text\\coverpage.html", coverHtml.ToString(), Encoding.UTF8);
				for (int i = 1; i <= txtHtmlList.Count; i++) zip.AddEntry("OEBPS\\Text\\chapter" + i + ".html", txtHtmlList[i-1], Encoding.UTF8);
				for (int i = 0; i < picHtmlList.Count; i++) zip.AddEntry("OEBPS\\Text\\picture" + i + ".html", picHtmlList[i], Encoding.UTF8);
				zip.AddFile(CoverPath, "OEBPS\\Images\\");
				zip.AddFile(CoverPathSlim, "OEBPS\\Images\\");
				for (int i = 0; i < pictureHTMLs.Count; i++)
				{
					String origPic = tempPath + "\\picture" + i + ".jpg";
					String origPicSlim = tempPath + "\\picture" + i + "~slim.jpg";
					zip.AddFile(origPic, "OEBPS\\Images\\");
					zip.AddFile(origPicSlim, "OEBPS\\Images\\");
				}
				if (parchmentNeeded) zip.AddFile(tempPath + "\\parchment.jpg", "OEBPS\\Images\\");
				if (embedFontSubset)
				{
					for (int i = 0; i < embedFontPaths.Count; i++)
					{
						zip.AddFile(embedFontPaths[i], "OEBPS\\Fonts\\");
					}
				}
				zip.Save(zipPath);
			}

			this.status_label.Text = "生成完毕！文件：" + zipPath + " ；耗时：" + getProcessTime().ToString() + " 秒";
			notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
			showBalloonTip("温馨提示", DocName + ".epub" + "\n已生成完毕！");

			Image tempImage = new Bitmap(toBeShown);
			cover_picturebox.Image = tempImage;
			toBeShown.Dispose();
			//tempImage.Dispose();
			
			// Somehow when the following block is enabled, there will be an error here!
			// clear CoverPath
			/*CoverPath = null;
			cover_picturebox.Image = null;
			if (toBeShown != null)
				//toBeShown.Dispose();
				toBeShown = null;*/

			/*** Delete temp files ***/
			if (deleteTempFiles)
			{
				deleteAllTempFiles();
			}

			DONE = true;
			coverChanged = false;
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

		private void TOC_button1_Click(object sender, EventArgs e)
		{
			Int32 selectedCellCount = TOC_list.GetCellCount(DataGridViewElementStates.Selected);

			bool selectedValid = true;
			if (selectedCellCount > 0)
			{
				for (int i = 0; i < selectedCellCount; i++)
				{
					if (TOC_list.SelectedCells[i].Value == null)
					{
						MessageBox.Show("操作无效！");
						selectedValid = false;
						break;
					}
					else
					{
						Match validValue = Regex.Match(TOC_list.SelectedCells[i].Value.ToString(), "^[0-9]+$");
						if (validValue.Success)
						{
							MessageBox.Show("操作无效！");
							selectedValid = false;
							break;
						}
					}
				}
				if (selectedValid)
				{
					for (int i = 0; i < selectedCellCount; i++)
					{
						if (TOC_list.SelectedCells[i].Value.ToString().StartsWith(" *** "))
						{
							String temp = TOC_list.SelectedCells[i].Value.ToString();
							temp = temp.Substring(5);
							TOC_list.SelectedCells[i].Value = temp;
						}
						else
						{
							MessageBox.Show("操作无效！");
							return;
						}
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				MessageBox.Show("操作无效！");
				return;
			}
		}

		private void TOC_button2_Click(object sender, EventArgs e)
		{
			Int32 selectedCellCount = TOC_list.GetCellCount(DataGridViewElementStates.Selected);

			bool selectedValid = true;
			if (selectedCellCount > 0)
			{
				for (int i = 0; i < selectedCellCount; i++)
				{
					if (TOC_list.SelectedCells[i].Value == null)
					{
						MessageBox.Show("操作无效！");
						selectedValid = false;
						break;
					}
					else
					{
						Match validValue = Regex.Match(TOC_list.SelectedCells[i].Value.ToString(), "^[0-9]+$");
						if (validValue.Success)
						{
							MessageBox.Show("操作无效！");
							selectedValid = false;
							break;
						}
					}
				}
				if (selectedValid)
				{
					for (int i = 0; i < selectedCellCount; i++)
					{
						if (String.Compare(TOC_list.SelectedCells[i].Value.ToString(), TOC_list.Rows[0].Cells[0].Value.ToString()) == 0)
						{
							MessageBox.Show("操作无效！");
							return;
						}
						if (TOC_list.SelectedCells[i].Value.ToString().StartsWith(" ***  ***  *** "))
						{
							MessageBox.Show("操作无效！");
							return;
						}
						else
						{
							String temp = TOC_list.SelectedCells[i].Value.ToString();
							temp = " *** " + temp;
							TOC_list.SelectedCells[i].Value = temp;
						}
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				MessageBox.Show("操作无效！");
				return;
			}
		}

		#endregion

		#region Main Functions

		private void generateCSS(bool vertical, float marginL, float marginR, float marginT, float marginB, float lineHeight, bool addParagraphSpacing, String titleFont, String titleColor, String bodyFont, String bodyColor, String pageColor, bool embedFontSubset)
		{
			this.status_label.Text = "正在生成... 正在生成其他文件...";

			String background = "";
			if (String.Compare(pageColor, "羊皮纸") == 0)
			{
				Extract("SimpleEpub", tempPath, "Resources", "parchment.jpg");
				background = "\n\tbackground-image:url(\"..\\Images\\parchment.jpg\");\n\tbackground-repeat:repeat;";
				parchmentNeeded = true;
			}
			else
			{
				background = "\n\tbackground-color:" + pageColor + ";";
				if (String.Compare(pageColor, "none") == 0)
					background = "";
			}

			StringBuilder font1 = new StringBuilder();
			font1.Append("@font-face {\n\tfont-family:\"" + titleFont + "\";\n\tsrc:local(\"" + titleFont + "\"),\n\turl(res:///opt/sony/ebook/FONT/" + titleFont + ".ttf),\n\turl(res:///Data/FONT/" + titleFont + ".ttf),\n\turl(res:///opt/sony/ebook/FONT/" + titleFont + ".ttf),\n\turl(res:///fonts/ttf/" + titleFont + ".ttf),\n\turl(res:///../../media/mmcblk0p1/fonts/" + titleFont + ".ttf),\n\turl(res:///DK_System/system/font/" + titleFont + ".ttf),\n\turl(res:///abook/fonts/" + titleFont + ".ttf),\n\turl(res:///system/fonts/" + titleFont + ".ttf),\n\turl(res:///system/media/sdcard/fonts/" + titleFont + ".ttf),\n\turl(res:///media/fonts/" + titleFont + ".ttf),\n\turl(res:///sdcard/fonts/" + titleFont + ".ttf),\n\turl(res:///system/fonts/" + titleFont + ".ttf),\n\turl(res:///mnt/MOVIFAT/font/" + titleFont + ".ttf)");
			if (embedFontSubset && titleFontPath.CompareTo("") != 0 && URIT != null)
			{
				CreateFontSubSetT();
				String font1Name = embedFontPaths[0].Substring(embedFontPaths[0].LastIndexOf("\\") + 1);
				font1.Append(",\n\turl(../Fonts/" + font1Name + ")");
			}
			font1.Append(";\n}\n");

			StringBuilder font2 = new StringBuilder();
			font2.Append("@font-face {\n\tfont-family:\"" + bodyFont + "\";\n\tsrc:local(\"" + bodyFont + "\"),\n\turl(res:///opt/sony/ebook/FONT/" + bodyFont + ".ttf),\n\turl(res:///Data/FONT/" + bodyFont + ".ttf),\n\turl(res:///opt/sony/ebook/FONT/" + bodyFont + ".ttf),\n\turl(res:///fonts/ttf/" + bodyFont + ".ttf),\n\turl(res:///../../media/mmcblk0p1/fonts/" + bodyFont + ".ttf),\n\turl(res:///DK_System/system/font/" + bodyFont + ".ttf),\n\turl(res:///abook/fonts/" + bodyFont + ".ttf),\n\turl(res:///system/fonts/" + bodyFont + ".ttf),\n\turl(res:///system/media/sdcard/fonts/" + bodyFont + ".ttf),\n\turl(res:///media/fonts/" + bodyFont + ".ttf),\n\turl(res:///sdcard/fonts/" + bodyFont + ".ttf),\n\turl(res:///system/fonts/" + bodyFont + ".ttf),\n\turl(res:///mnt/MOVIFAT/font/" + bodyFont + ".ttf)");
			if (embedFontSubset && bodyFontPath.CompareTo("") != 0 && URIB != null)
			{
				CreateFontSubSetB();
				String font2Name = embedFontPaths[1].Substring(embedFontPaths[1].LastIndexOf("\\") + 1);
				font2.Append(",\n\turl(../Fonts/" + font2Name + ")");
			}
			font2.Append(";\n}\n");

			String html = (vertical ? "html {\n\twriting-mode:vertical-rl;\n\t-webkit-writing-mode:vertical-rl;\n\t-epub-writing-mode:vertical-rl;\n\t-epub-line-break:strict;\n\tline-break:strict;\n\t-epub-word-break:normal;\n\tword-break:normal;\n\tmargin:0;\n\tpadding:0;\n}\n" : "");

			String body = "body {\n\tmargin-top:" + marginT + "%;\n\tmargin-bottom:" + marginB + "%;\n\tmargin-left:" + marginL + "%;\n\tmargin-right:" + marginR + "%;\n\tline-height:" + lineHeight + "%;\n\ttext-align:justify;\n\tfont-family:" + bodyFont + ";\n\tcolor:" + bodyColor + ";" + background + "\n}\n";

			String div = "div {\n\tmargin:0px;\n\tline-height:" + lineHeight + "%;\n\ttext-align:justify;\n\tfont-family:" + bodyFont + ";\n\tcolor:" + bodyColor + ";\n}\n";

			String img = "img {\n\tmax-width:100%;\n\tmax-height:100%;\n\tbottom:0;\n\tleft:0;\n\tmargin:auto;\n\toverflow:auto;\n\tposition:fixed;\n\tright:0;\n\ttop:0;\n}\n";
			//String img = "img {\n\tbottom:0;\n\tleft:0;\n\tmargin:auto;\n\toverflow:auto;\n\tposition:fixed;\n\tright:0;\n\ttop:0;\n}\n";
			//String img = "";

			int pMargin = (addParagraphSpacing ? 5 : 0);
			String p = "p {\n\ttext-align:justify;\n\ttext-indent:2em;\n\tline-height:" + lineHeight + "%;\n\tmargin-top:" + pMargin + "pt;\n\tmargin-bottom:" + pMargin + "pt;\n}\n";

			String others = ".cover {\n\twidth:100%;\n}\n.center {\n\ttext-align:center;\n\tmargin-left:0%;\n\tmargin-right:0%;\n}\n.left {\n\ttext-align:left;\n\tmargin-left:0%;\n\tmargin-right:0%;\n}\n.right {\n\ttext-align:right;\n\tmargin-left:0%;\n\tmargin-right:0%;\n}\n.quote {\n\tmargin-top:0%;\n\tmargin-bottom:0%;\n\tmargin-left:1em;\n\tmargin-right:1em;\n\ttext-align:justify;\n\tfont-family:" + bodyFont + ";\n\tcolor:" + bodyColor + ";\n}\n";

			String headers = "h1 {\n\tline-height:" + lineHeight + "%;\n\ttext-align:center;\n\tfont-weight:bold;\n\tfont-size:xx-large;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh2 {\n\tline-height:" + lineHeight + "%;\n\ttext-align:center;\n\tfont-weight:bold;\n\tfont-size:x-large;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh3 {\n\tline-height:" + lineHeight + "%;\n\ttext-align:center;\n\tfont-weight:bold;\n\tfont-size:large;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh4 {\n\tline-height:" + lineHeight + "%;\n\ttext-align:center;\n\tfont-weight:bold;\n\tfont-size:medium;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh5 {\n\tline-height:" + lineHeight + "%;\n\ttext-align:center;\n\tfont-weight:bold;\n\tfont-size:small;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\nh6 {\n\tline-height:" + lineHeight + "%;\n\ttext-align:center;\n\tfont-weight:bold;\n\tfont-size:x-small;\n\tfont-family:" + titleFont + ";\n\tcolor:" + titleColor + ";\n}\n";

			css.Append(font1);
			css.Append(font2);
			css.Append(html);
			css.Append(body);
			css.Append(div);
			css.Append(img);
			css.Append(p);
			css.Append(others);
			css.Append(headers);
		}

		private void generateOPF(bool coverFirstPage, int translation, bool vertical, bool embedFontSubset)
		{
			Intro = translate(Intro, translation);

			String head = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<package version=\"2.0\" unique-identifier=\"BookID\" xmlns=\"http://www.idpf.org/2007/opf\">\n<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">\n<dc:title>" + bookAndAuthor[0] + "</dc:title>\n<dc:identifier id=\"BookID\">urn:uuid:henryxrl@gmail.com</dc:identifier>\n<dc:language>zh-CN</dc:language>\n<dc:creator opf:role=\"aut\">" + bookAndAuthor[1] + "</dc:creator>\n<dc:description>" + Intro + "</dc:description>\n<meta name=\"cover\" content=\"cover-image\" />\n</metadata>\n<manifest>\n<!-- Content Documents -->\n";

			StringBuilder body1 = new StringBuilder();
			if (coverFirstPage)
			{
				body1.Append("<item id=\"coverpage\" href=\"Text/coverpage.html\"  media-type=\"application/xhtml+xml\" />\n");
			}

			int chapterID = 1;
			int picID = 0;
			int iVal = (extraLinesInBeginning ? (chapterNumber + pictureHTMLs.Count - 1) : (chapterNumber + pictureHTMLs.Count));
			if (extraLinesInBeginning)
			{
				if (extraLinesNotEmpty)
				{
					body1.Append("<item id=\"chapter" + chapterID + "\"  href=\"Text/chapter" + chapterID + ".html\"  media-type=\"application/xhtml+xml\" />\n");
					chapterID++;
				}
			}

			// no toc found
			if (TOC_list.Rows[0].Cells[0].Value == null)
			{
				body1.Append("<item id=\"chapter" + chapterID + "\"  href=\"Text/chapter" + chapterID + ".html\"  media-type=\"application/xhtml+xml\" />\n");
			}
			else
			{
				for (int i = 1; i <= iVal; i++)
				{
					String tempTitle = TOC_list.Rows[i - 1].Cells[0].Value.ToString();

					String tempTitleProcessed = "";
					if (tempTitle.Contains(" *** "))
					{
						tempTitleProcessed = tempTitle.Replace(" *** ", "");
					}
					else
					{
						tempTitleProcessed = tempTitle;
					}

					// 加图片页
					if (tempTitle.StartsWith("<"))
					{
						body1.Append("<item id=\"picture" + picID + "\"  href=\"Text/picture" + picID + ".html\"  media-type=\"application/xhtml+xml\" />\n");
						picID++;
					}

					else	// 加文字页
					{
						body1.Append("<item id=\"chapter" + chapterID + "\"  href=\"Text/chapter" + chapterID + ".html\"  media-type=\"application/xhtml+xml\" />\n");
						chapterID++;
					}
				}
			}

			String spine = "";
			if (vertical)
			{
				spine = "<spine toc=\"ncx\" page-progression-direction=\"rtl\">";
			}
			else
			{
				spine = "<spine toc=\"ncx\">";
			}

			StringBuilder otherImages = new StringBuilder();
			for (int i = 0; i < picHtmlList.Count; i++)
			{
				otherImages.Append("\n<item id=\"picture" + i + "-image\" href=\"Images/picture" + i + ".jpg\" media-type=\"image/jpeg\" />\n<item id=\"picture" + i + "-image-slim\" href=\"Images/picture" + i + "~slim.jpg\" media-type=\"image/jpeg\" />");
			}

			StringBuilder embedFonts = new StringBuilder();
			if (embedFontSubset)
			{
				for (int i = 0; i < embedFontPaths.Count; i++)
				{
					String fontPath = embedFontPaths[i];
					String fontName = fontPath.Substring(fontPath.LastIndexOf("\\") + 1);
					embedFonts.Append("\n<item id=\"" + fontName + "\" href=\"Fonts/" + fontName + "\" media-type=\"application/vnd.ms-opentype\" />");
				}
			}

			String body2 = "\n<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\" />\n<item id=\"css\" href=\"Styles/main.css\" media-type=\"text/css\" />\n<item id=\"cover-image\" href=\"Images/cover.jpg\" media-type=\"image/jpeg\" />\n<item id=\"cover-image-slim\" href=\"Images/cover~slim.jpg\" media-type=\"image/jpeg\" />" + otherImages.ToString() + embedFonts.ToString() + "\n</manifest>\n\n" + spine + "\n";

			StringBuilder body3 = new StringBuilder();
			if (coverFirstPage)
			{
				body3.Append("<itemref idref=\"coverpage\" properties=\"duokan-page-fullscreen\" />\n");
			}

			chapterID = 1;
			picID = 0;
			if (extraLinesInBeginning)
			{
				if (extraLinesNotEmpty)
				{
					body3.Append("<itemref idref=\"chapter" + chapterID + "\" />\n");
					chapterID++;
				}
			}

			if (TOC_list.Rows[0].Cells[0].Value == null)
			{
				body3.Append("<itemref idref=\"chapter" + chapterID + "\" />\n");
			}
			else
			{
				for (int i = 1; i <= iVal; i++)
				{
					String tempTitle = TOC_list.Rows[i - 1].Cells[0].Value.ToString();

					String tempTitleProcessed = "";
					if (tempTitle.Contains(" *** "))
					{
						tempTitleProcessed = tempTitle.Replace(" *** ", "");
					}
					else
					{
						tempTitleProcessed = tempTitle;
					}

					// 加图片页
					if (tempTitle.StartsWith("<"))
					{
						body3.Append("<itemref idref=\"picture" + picID + "\" properties=\"duokan-page-fullscreen\" />\n");
						picID++;
					}

					else	// 加文字页
					{
						body3.Append("<itemref idref=\"chapter" + chapterID + "\" />\n");
						chapterID++;
					}
				}
			}

			String foot = "\n</spine>\n<guide>\n\n</guide>\n</package>";

			opf.Append(head);
			opf.Append(body1);
			opf.Append(body2);
			opf.Append(body3);
			opf.Append(foot);
		}

		private void generateNCX(bool coverFirstPage, bool coverNoTOC, int translation)
		{
			/*** head ***/
			String head = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE ncx PUBLIC \"-//NISO//DTD ncx 2005-1//EN\" \"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\">\n<ncx version=\"2005-1\" xml:lang=\"zh-CN\" xmlns=\"http://www.daisy.org/z3986/2005/ncx/\">\n<head>\n\t<!-- The following four metadata items are required for all NCX documents, including those conforming to the relaxed constraints of OPS 2.0 -->\n\t<meta name=\"dtb:uid\" content=\"urn:uuid:henryxrl@gmail.com\" />\n\t<meta name=\"dtb:depth\" content=\"1\" />\n\t<meta name=\"dtb:totalPageCount\" content=\"0\" />\n\t<meta name=\"dtb:maxPageNumber\" content=\"0\" />\n</head>\n<docTitle><text>" + bookAndAuthor[0] + "</text></docTitle>\n<docAuthor><text>" + bookAndAuthor[1] + "</text></docAuthor>\n";

			/*** navMap ***/
			int maxCount = 0;

			List<Tuple<int, navPoint>> TOCTree = new List<Tuple<int, navPoint>>();

			bool addFirstChapter = true;
			int j = 1;
			int picIDX = 0;
			bool fromD = false;
			int iVal = (extraLinesInBeginning ? (chapterNumber + pictureHTMLs.Count - 1) : (chapterNumber + pictureHTMLs.Count));

			if (coverFirstPage && addFirstChapter)
			{
				String title = "封面";
				title = translate(title, translation);		// 简繁转换

				if (!coverNoTOC)
				{
					TOCTree.Add(new Tuple<int, navPoint>(0, new navPoint("coverpage", 1, title, "Text/coverpage.html", null, null)));
				}

				addFirstChapter = false;
			}

			if (extraLinesInBeginning)
			{
				int start = (txtHtmlList[0].IndexOf("<title>") + "<title>".Length);
				int end = (txtHtmlList[0].IndexOf("</title>"));
				int length = end - start;
				String title = txtHtmlList[0].Substring(start, length);
				if (extraLinesNotEmpty)
				{
					if (!coverNoTOC)
						TOCTree.Add(new Tuple<int, navPoint>(0, new navPoint("chapter" + j, (j + 1), title, "Text/chapter" + j + ".html", null, null)));
					else
						TOCTree.Add(new Tuple<int, navPoint>(0, new navPoint("chapter" + j, j, title, "Text/chapter" + j + ".html", null, null)));
					j++;
				}
				extraLinesInBeginning = false;
				extraLinesNotEmpty = false;
			}

			if (TOC_list.Rows[0].Cells[0].Value == null)
			{
				if (!coverNoTOC)
					TOCTree.Add(new Tuple<int, navPoint>(0, new navPoint("chapter" + j, (j + 1), bookAndAuthor[0], "Text/chapter" + j + ".html", null, null)));
				else
					TOCTree.Add(new Tuple<int, navPoint>(0, new navPoint("chapter" + j, j, bookAndAuthor[0], "Text/chapter" + j + ".html", null, null)));
			}
			else
			{
				for (int i = 1; i <= iVal; i++)
				{
					// 删除" *** "标识
					//MessageBox.Show("i: " + i + "\nj: " + j + "\n(i-j): " + (i - j));
					String tempTitle = TOC_list.Rows[i - 1].Cells[0].Value.ToString();

					String tempTitleProcessed = "";
					if (tempTitle.Contains(" *** "))
					{
						tempTitleProcessed = tempTitle.Replace(" *** ", "");
					}
					else
					{
						tempTitleProcessed = tempTitle;
					}
					tempTitleProcessed = tempTitleProcessed.Trim();

					// 目录分级
					int occurCount = CountStringOccurrences(tempTitle, " *** ");
					maxCount = (occurCount >= maxCount) ? occurCount : maxCount;

					// 加页
					if (!tempTitleProcessed.StartsWith("<"))
					{
						if (!fromD)
						{
							tempTitleProcessed = translate(tempTitleProcessed, translation);
							if (!coverNoTOC)
								TOCTree.Add(new Tuple<int, navPoint>(occurCount, new navPoint("chapter" + j, (j + 1), tempTitleProcessed, "Text/chapter" + j + ".html", null, null)));
							else
								TOCTree.Add(new Tuple<int, navPoint>(occurCount, new navPoint("chapter" + j, j, tempTitleProcessed, "Text/chapter" + j + ".html", null, null)));
							j++;
						}
						else
						{
							fromD = false;
						}
					}
					else
					{
						String temp = pictureHTMLs[picIDX].Item1;
						if (temp.StartsWith("U") || temp.StartsWith("u"))
						{
							picIDX++;
						}
						else if (temp.StartsWith("D") || temp.StartsWith("d"))
						{
							temp = temp.Substring(1, temp.Length - 1);
							temp = temp.Trim();

							if (!coverNoTOC)
								TOCTree.Add(new Tuple<int, navPoint>(occurCount, new navPoint("picture" + picIDX, (j + 1), temp, "Text/picture" + picIDX + ".html", null, null)));
							else
								TOCTree.Add(new Tuple<int, navPoint>(occurCount, new navPoint("picture" + picIDX, j, temp, "Text/picture" + picIDX + ".html", null, null)));
							picIDX++;
							j++;

							fromD = true;
						}
						else
						{
							temp = temp.Trim();

							if (!coverNoTOC)
								TOCTree.Add(new Tuple<int, navPoint>(occurCount, new navPoint("picture" + picIDX, (j + 1), temp, "Text/picture" + picIDX + ".html", null, null)));
							else
								TOCTree.Add(new Tuple<int, navPoint>(occurCount, new navPoint("picture" + picIDX, j, temp, "Text/picture" + picIDX + ".html", null, null)));
							picIDX++;
							j++;

						}
					}
				}
			}

			navMap nm = new navMap(TOCTree, maxCount);


			ncx.Append(head);
			ncx.Append(nm.printNM());
			ncx.Append("</ncx>");
		}

		private bool generateHTML(bool coverFirstPage, int translation, bool vertical, bool replace, bool embedFontSubset)
		{
			this.status_label.Text = "正在生成... 正在生成HTML文件...";

			if (!File.Exists(TXTPath))
			{
				MessageBox.Show("找不到" + TXTPath + "文件！\n无法继续生成！");
				return false;
			}

			using (FileStream fs = File.Open(TXTPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (BufferedStream bs = new BufferedStream(fs))
			using (StreamReader sr = new StreamReader(bs, Encoding.GetEncoding("GB2312")))
			{
				String nextLine;
				int TXTlineNumber = 1;
				int TLN_idx = 0;
				int TLN_size = titleLineNumbers.Count;

				bool sameChapter = false;
				String toPrint = "";
				chapterNumber = 1;
				// 制作第一页封面
				if (coverFirstPage)
				{
					String title = "封面";
					title = translate(title, translation);		// 简繁转换

					coverHtml.Append(HTMLHead(title, "", 0));
					coverHtml.Append("\n<img src=\"../Images/cover.jpg\" alt=\"Cover\" />\n</div>\n</body>\n</html>");

					//chapterNumber++;
				}

				// 制作其他图片页
				for (int i = 0; i < pictureHTMLs.Count; i++)
				{
					String title = pictureHTMLs[i].Item1;
					title = title.Trim();

					if (title.StartsWith("D") || title.StartsWith("U") || title.StartsWith("d") || title.StartsWith("u"))
					{
						title = title.Substring(1, title.Length - 1);
						title = title.Trim();
					}

					title = translate(title, translation);		// 简繁转换

					String tempToPrint = HTMLHead(title, "picture" + i, 2) + "\n<img src=\"../Images/" + "picture" + i + ".jpg\" alt=\"Cover\" />\n</div>\n</body>\n</html>";
					picHtmlList.Add(tempToPrint);
				}


				if (FSLinesScanned != 0)
				{
					//FSLinesScanned = 0;
					TXTlineNumber += 2;
				}

				// Read from the first line to the first chapter title defined!
				if (TLN_size != 0 && titleLineNumbers[0] > TXTlineNumber)
				{
					extraLinesInBeginning = true;
					StringBuilder html = new StringBuilder();
					bool firstTime = true;

					while (titleLineNumbers[0] > TXTlineNumber && (nextLine = sr.ReadLine()) != null)
					{
						if (FSLinesScanned != 0)
						{
							FSLinesScanned--;
							continue;
						}

						Match emptyLine = Regex.Match(nextLine, "^\\s*$");
						if (!emptyLine.Success)		// Remove empty lines
						{
							extraLinesNotEmpty = true;

							nextLine = nextLine.Trim();
							nextLine = translate(nextLine, translation);		// 简繁转换

							// 删除" *** "标识
							if (nextLine.Contains(" *** "))
							{
								nextLine = nextLine.Replace(" *** ", "");
							}

							if (vertical)		// 半角字符转全角
							{
								nextLine = ToSBC(nextLine);
							}
							if (embedFontSubset && URIB != null) addStringToUInt16CollectionB(nextLine);
							if (firstTime)
							{
								html.Append(HTMLHead(nextLine, "", 1) + "\n");
								firstTime = false;
							}
							else
								html.Append("<p>" + nextLine + "</p>\n");
						}
						TXTlineNumber++;
					}
					//extraLinesInBeginning = false;
					html.Append("</div>\n</body>\n</html>\n");
					if (extraLinesNotEmpty)
						txtHtmlList.Add(html.ToString());
					chapterNumber++;
				}


				while (true)
				{
					StringBuilder html = new StringBuilder();

					if (toPrint != "")
					{
						html.Append(toPrint + "\n");
						sameChapter = true;
						toPrint = "";
					}

					if (TLN_size == 0)
					{
						html.Append(HTMLHead(bookAndAuthor[0], "", 1) + "\n");
					}
					while ((nextLine = sr.ReadLine()) != null)
					{
						Match emptyLine = Regex.Match(nextLine, "^\\s*$");
						if (!emptyLine.Success)		// Remove empty lines
						{
							nextLine = nextLine.Trim();
							nextLine = translate(nextLine, translation);		// 简繁转换

							// 删除" *** "标识
							if (nextLine.Contains(" *** "))
							{
								nextLine = nextLine.Replace(" *** ", "");
							}

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
								if (embedFontSubset) addStringToUInt16CollectionT(nextLine);
								if (sameChapter)
								{
									sameChapter = false;
									toPrint = HTMLHead(nextLine, "", 1);
									TLN_idx++;
									TXTlineNumber++;
									break;
								}
								else
								{
									html.Append(HTMLHead(nextLine, "", 1) + "\n");
									sameChapter = true;
									TLN_idx++;
								}
							}
							else
							{
								if (embedFontSubset) addStringToUInt16CollectionB(nextLine);
								html.Append("<p>" + nextLine + "</p>\n");
							}
						}
						TXTlineNumber++;
					}

					if (nextLine != null)
					{
						html.Append("</div>\n</body>\n</html>\n");
						txtHtmlList.Add(html.ToString());
						chapterNumber++;
					}
					else
					{
						html.Append("</div>\n</body>\n</html>\n");
						txtHtmlList.Add(html.ToString());
						break;
					}
				}
			}

			return true;
		}

		#endregion

		#region Helper Functions

		private bool loadNewTOC()
		{
			this.status_label.Text = "正在生成... 正在加载设置...";

			bool validTOC = true;
			int tempIDX = 0;
			int picIDX = 0;
			while (TOC_list.Rows[tempIDX].Cells[1].Value != null)
			{
				Match validValue = Regex.Match(TOC_list.Rows[tempIDX].Cells[1].Value.ToString(), "^[0-9]+$");
				if (!validValue.Success)
				{
					if (TOC_list.Rows[tempIDX].Cells[0].Value.ToString().StartsWith("<"))		// insert picture chapter
					{
						String picPath = TOC_list.Rows[tempIDX].Cells[1].Value.ToString();
						if (File.Exists(picPath))
						{
							String newPicPath = tempPath + "\\picture" + picIDX + "temp.jpg";
							File.Copy(picPath, newPicPath, true);
							crop("picture" + picIDX);
							tempIDX++;
							picIDX++;
							continue;
						}
						try
						{
							picPath = Path.Combine(getROOTPath(), picPath);
							if (File.Exists(picPath))
							{
								String newPicPath = tempPath + "\\picture" + picIDX + "temp.jpg";
								File.Copy(picPath, newPicPath, true);
								crop("picture" + picIDX);
								picIDX++;
								tempIDX++;
								continue;
							}
						}
						catch
						{
							MessageBox.Show("无效的目录！行号只能是数字！\n无法继续生成Epub！");
							validTOC = false;
							break;
						}
					}
					else
					{
						MessageBox.Show("无效的目录！行号只能是数字！\n无法继续生成Epub！");
						validTOC = false;
						break;
					}
				}
				tempIDX++;
			}
			if (!validTOC) return false;

			if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[1].Cells[0].Value != null)
			{
				// load new book name and author
				bookAndAuthor.Clear();
				bookAndAuthor.Add(cover_bookname_textbox.Text);
				bookAndAuthor.Add(cover_author_textbox.Text);

				// load new title line number
				if (TOC_list.Rows[0].Cells[0].Value != null && TOC_list.Rows[0].Cells[0].Value != null)
				{
					titleLineNumbers.Clear();
					pictureHTMLs.Clear();
					for (int i = 0; i < TOC_list.Rows.Count; i++)
					{
						if (TOC_list.Rows[i].Cells[0].Value != null && TOC_list.Rows[i].Cells[1].Value != null)
						{
							Match isLineNumber = Regex.Match(TOC_list.Rows[i].Cells[1].Value.ToString(), "^[0-9]+$");
							if (isLineNumber.Success)
							{
								titleLineNumbers.Add(Convert.ToInt32(TOC_list.Rows[i].Cells[1].Value));
							}
							else
							{
								String pictureHTMLTitle = TOC_list.Rows[i].Cells[0].Value.ToString();
								if (pictureHTMLTitle.StartsWith("<"))
								{
									pictureHTMLTitle = pictureHTMLTitle.Trim();
									pictureHTMLTitle = pictureHTMLTitle.Substring(1, pictureHTMLTitle.Length - 1);
									pictureHTMLTitle = pictureHTMLTitle.Trim();

									pictureHTMLs.Add(new Tuple<String, String>(pictureHTMLTitle, TOC_list.Rows[i].Cells[1].Value.ToString().Trim()));
								}
							}
						}
					}
				}
			}

			return true;
		}

		private static String HTMLHead(String chapterTitle, String picName, int flag)		// flag == 0: cover page; flag == 1: chapter page; flag == 2: else
		{
			if (flag == 0)
			{
				return "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n<link rel=\"stylesheet\" type=\"text/css\" href=\"../Styles/main.css\" />\n<script type=\"text/javascript\">\n$(function() {\n\tif(($(window).height() / $(window).width()) >= 1.5) {\n\t\t$(\"img\").each(function() {\n\t\t\t$(this).attr(\"src\", $(this).attr(\"src\").replace(\"../Images/cover.jpg\", \"../Images/cover~slim.jpg\"));\n\t\t});\n\t}\n});\n</script>\n<title>" + chapterTitle + "</title>\n</head>\n<body>\n<div>";
			}
			else if (flag == 1)
			{
				return "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n<link rel=\"stylesheet\" type=\"text/css\" href=\"../Styles/main.css\" />\n<title>" + chapterTitle + "</title>\n</head>\n<body>\n<div>\n<h2>" + chapterTitle + "</h2>";
			}
			else
			{
				return "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n<link rel=\"stylesheet\" type=\"text/css\" href=\"../Styles/main.css\" />\n<script type=\"text/javascript\">\n$(function() {\n\tif(($(window).height() / $(window).width()) >= 1.5) {\n\t\t$(\"img\").each(function() {\n\t\t\t$(this).attr(\"src\", $(this).attr(\"src\").replace(\"../Images/" + picName + ".jpg\", \"../Images/" + picName + "~slim.jpg\"));\n\t\t});\n\t}\n});\n</script>\n<title>" + chapterTitle + "</title>\n</head>\n<body>\n<div>";
			}
		}

		private void generateCoverFromFilePath()
		{
			String coverTemp = tempPath + "\\covertemp.jpg";
			System.IO.File.Copy(origCover, coverTemp, true);
			crop("cover");
			CoverPath = tempPath + "\\cover.jpg";
			CoverPathSlim = tempPath + "\\cover~slim.jpg";

			cover_picturebox.SizeMode = PictureBoxSizeMode.Zoom;
			//toBeShown = new Bitmap(CoverPath);
			toBeShown = Image.FromFile(CoverPath);
			cover_picturebox.Image = toBeShown;
		}

		private bool copyImageFile(bool vertical, String bookfont, String authorfont)
		{
			if (CoverPath == null || !File.Exists(CoverPath))
			{
				toBeShown.Dispose();

				using (Image cover = DrawText(1536, 2048, vertical, bookfont, authorfont))
				{
					try
					{
						CoverPath = tempPath + "\\cover.jpg";
						//cover.Save(CoverPath, System.Drawing.Imaging.ImageFormat.Jpeg);
						SaveJpeg(CoverPath, cover, 100);
					}
					catch
					{
						MessageBox.Show("coverpath: " + CoverPath);
					}
				}

				cover_picturebox.SizeMode = PictureBoxSizeMode.Zoom;
				//toBeShown = new Bitmap(CoverPath);
				toBeShown = Image.FromFile(CoverPath);
				cover_picturebox.Image = toBeShown;
			}
			else
			{
				if (!File.Exists(CoverPath))
				{
					MessageBox.Show("找不到" + CoverPath + "文件！\n无法继续生成！");
					return false;
				}
			}

			if (CoverPathSlim == null || !File.Exists(CoverPathSlim))
			{
				using (Image coverSlim = DrawText(1080, 1920, vertical, bookfont, authorfont))
				{
					try
					{
						CoverPathSlim = tempPath + "\\cover~slim.jpg";
						//coverSlim.Save(CoverPathSlim, System.Drawing.Imaging.ImageFormat.Jpeg);
						SaveJpeg(CoverPathSlim, coverSlim, 100);
					}
					catch
					{
						MessageBox.Show("coverpathslim: " + CoverPathSlim);
					}
				}
			}
			else
			{
				if (!File.Exists(CoverPathSlim))
				{
					MessageBox.Show("找不到" + CoverPathSlim + "文件！\n无法继续生成！");
					return false;
				}
			}

			for (int i = 0; i < pictureHTMLs.Count; i++)
			{
				String origPic = tempPath + "\\picture" + i + ".jpg";
				String origPicSlim = tempPath + "\\picture" + i + "~slim.jpg";

				if (!File.Exists(origPic))
				{
					MessageBox.Show("找不到" + origPic + "文件！\n无法继续生成！");
					return false;
				}
				if (!File.Exists(origPicSlim))
				{
					MessageBox.Show("找不到" + origPicSlim + "文件！\n无法继续生成！");
					return false;
				}
			}
			return true;
		}

		private Image DrawText(int width, int height, bool vertical, String bookfont, String authorfont)
		{
			//String bookname = VB.Strings.StrConv(bookAndAuthor[0], VB.VbStrConv.SimplifiedChinese, 0);
			//String author = VB.Strings.StrConv(bookAndAuthor[1], VB.VbStrConv.SimplifiedChinese, 0);
			String bookname = bookAndAuthor[0];
			String author = bookAndAuthor[1];

			Image img = new Bitmap(width, height);	
			Graphics drawing = Graphics.FromImage(img);

			//paint the background
			drawing.Clear(System.Drawing.Color.FromArgb(50, 70, 110));

			System.Drawing.Brush whiteBrush = new SolidBrush(System.Drawing.Color.White);
			System.Drawing.Brush orangeBrush = new SolidBrush(System.Drawing.Color.FromArgb(228, 89, 23));
			System.Drawing.Brush yellowBrush = new SolidBrush(System.Drawing.Color.Yellow);
			System.Drawing.Brush blackBrush = new SolidBrush(System.Drawing.Color.Black);

			System.Drawing.Pen whitePen = new System.Drawing.Pen(whiteBrush, width / 1000 * 5);

			Rectangle bookNameRec;
			Rectangle authorRec1;
			Rectangle authorRec2;

			if (!vertical)		// 横排书封面
			{
				drawing.DrawLine(whitePen, new Point(width / 15, 0), new Point(width / 15, height));
				drawing.DrawLine(whitePen, new Point(0, height / 10), new Point(width / 15, height / 10));
				drawing.DrawLine(whitePen, new Point(0, height / 10 + height / 3), new Point(width / 15, height / 10 + height / 3));
				drawing.DrawLine(whitePen, new Point(0, height - height / 10 - height / 3), new Point(width / 15, height - height / 10 - height / 3));
				drawing.DrawLine(whitePen, new Point(0, height - height / 10), new Point(width / 15, height - height / 10));

				bookNameRec = Rectangle.FromLTRB(width / 2 + width / 10, height / 10, width - width / 10, height / 2 + height / 10);
				drawing.FillRectangle(whiteBrush, bookNameRec);
				authorRec1 = Rectangle.FromLTRB(width / 2 + width / 10 - width / 20, height / 10, width / 2 + width / 10, height / 2 + height / 10 - height / 40);
				drawing.FillRectangle(orangeBrush, authorRec1);
				authorRec2 = Rectangle.FromLTRB(width / 2 + width / 10 - width / 20, height / 10 + authorRec1.Height, width / 2 + width / 10, height / 2 + height / 10);
				drawing.FillRectangle(orangeBrush, authorRec2);
			}
			else		// 竖排书封面
			{
				drawing.DrawLine(whitePen, new Point(width - width / 15, 0), new Point(width - width / 15, height));
				drawing.DrawLine(whitePen, new Point(width - width / 15, height / 10), new Point(width, height / 10));
				drawing.DrawLine(whitePen, new Point(width - width / 15, height / 10 + height / 3), new Point(width, height / 10 + height / 3));
				drawing.DrawLine(whitePen, new Point(width - width / 15, height - height / 10 - height / 3), new Point(width, height - height / 10 - height / 3));
				drawing.DrawLine(whitePen, new Point(width - width / 15, height - height / 10), new Point(width, height - height / 10));

				bookNameRec = Rectangle.FromLTRB(width / 10, height / 10, width / 2 - width / 10, height / 2 + height / 10);
				drawing.FillRectangle(whiteBrush, bookNameRec);
				authorRec1 = Rectangle.FromLTRB(width / 2 - width / 10, height / 10, width / 2 - width / 10 + width / 20, height / 2 + height / 10 - height / 40);
				drawing.FillRectangle(orangeBrush, authorRec1);
				authorRec2 = Rectangle.FromLTRB(width / 2 - width / 10, height / 10 + authorRec1.Height, width / 2 - width / 10 + width / 20, height / 2 + height / 10);
				drawing.FillRectangle(orangeBrush, authorRec2);
			}

			StringFormat bookDrawFormat = new StringFormat();
			bookDrawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
			//bookDrawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
			bookDrawFormat.Alignment = StringAlignment.Center;
			bookDrawFormat.LineAlignment = StringAlignment.Center;
			bookname = ToSBC(bookname);
			Tuple<String, int> settings = setTitleFontSize(bookname.Length, bookNameRec.Width, bookname, vertical);
			//Font bookFont = new Font(privateFontFamilies[1], settings.Item2, FontStyle.Bold);
			Font bookFont = new Font(bookfont, settings.Item2, FontStyle.Bold);
			drawing.DrawString(settings.Item1, bookFont, blackBrush, bookNameRec, bookDrawFormat);

			StringFormat authorDrawFormat = new StringFormat();
			authorDrawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
			//authorDrawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
			authorDrawFormat.Alignment = StringAlignment.Far;
			authorDrawFormat.LineAlignment = StringAlignment.Center;
			author = ToSBC(author);
			//Font authorFont = new Font(privateFontFamilies[0], authorRec1.Width / 2, FontStyle.Bold);
			Font authorFont = new Font(authorfont, authorRec1.Width / 2, FontStyle.Bold);
			drawing.DrawString(author + " ◆ 著", authorFont, whiteBrush, authorRec1, authorDrawFormat);
			drawing.DrawString(" ◆ 著", authorFont, yellowBrush, authorRec1, authorDrawFormat);
			drawing.DrawString("著", authorFont, whiteBrush, authorRec1, authorDrawFormat);

			whiteBrush.Dispose();
			orangeBrush.Dispose();
			yellowBrush.Dispose();
			blackBrush.Dispose();
			whitePen.Dispose();
			bookFont.Dispose();
			authorFont.Dispose();
			bookDrawFormat.Dispose();
			authorDrawFormat.Dispose();

			drawing.Save();
			drawing.Dispose();

			return img;
		}

		private void deleteAllTempFiles()
		{
			/*if (File.Exists(getTOCPath()))
			{
				File.Delete(getTOCPath());
			}*/
			Array.ForEach(Directory.GetFiles(tempPath), File.Delete);
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

		private String getROOTPath()
		{
			loadINI();
			return ini.IniReadValue("Tab_4", "Generated_File_Location");
		}

		private String getTOCPath()
		{
			return getROOTPath() + "\\目录.txt";
		}

		private void setCellFontColor(System.Drawing.Color a, System.Drawing.Color b)
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
				FSLinesScanned = 2;

				// No complete book name and author info
				notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
				showBalloonTip("文件名中无完整书名和作者信息！", "完整信息范例：“《书名》作者：作者名”`n（文件名中一定要有“作者”二字，书名号可以省略）`n`n开始检测文件第一、二行……`n其中第一行为书名，第二行为作者名。！");

				// Treat first line as book name and second line as author
				using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("GB2312")))
				{
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
				}
				return result;
			}
		}

		private static String getIntroInfo(String path)
		{
			StringBuilder result = new StringBuilder();

			using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (BufferedStream bs = new BufferedStream(fs))
			using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("GB2312")))
			{
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
					if (line != null && (line.Contains("简介") || line.Contains("簡介")))
					{
						for (int j = introLineNumber + 1; j < firstTitleLineNumber; j++)
						{
							line = sr.ReadLine();
							result.Append(line.Trim() + "\n");
						}
					}
					introLineNumber++;
				}
			}
			
			return result.ToString().Trim();
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

		private static int CountStringOccurrences(string text, string pattern)
		{
			int count = 0;
			int i = 0;
			while ((i = text.IndexOf(pattern, i)) != -1)
			{
				i += pattern.Length;
				count++;
			}
			return count;
		}

		private static void crop(String tempIMGNamePart1)
		{
			String tempIMGPath = tempPath + "\\" + tempIMGNamePart1 + "temp.jpg";
			
			Image newImage = Image.FromFile(tempIMGPath);
			String pic = "";
			String picSlim = "";
			if (((float)(newImage.Height) / (float)(newImage.Width)) <= ((float)4 / (float)3))
			{
				// create cover
				pic = tempPath + "\\" + tempIMGNamePart1 + ".jpg";
				int newWidth = 3 * newImage.Height / 4;
				Bitmap croppedBitmap = new Bitmap(newImage);
				croppedBitmap = croppedBitmap.Clone(new Rectangle((newImage.Width / 2 - newWidth / 2), 0, newWidth, newImage.Height), System.Drawing.Imaging.PixelFormat.DontCare);
				//croppedBitmap.Save(pic, System.Drawing.Imaging.ImageFormat.Jpeg);
				SaveJpeg(pic, croppedBitmap, 100);
				croppedBitmap.Dispose();

				// create cover~slim
				picSlim = tempPath + "\\" + tempIMGNamePart1 + "~slim.jpg";
				newWidth = 9 * newImage.Height / 16;
				croppedBitmap = new Bitmap(newImage);
				croppedBitmap = croppedBitmap.Clone(new Rectangle((newImage.Width / 2 - newWidth / 2), 0, newWidth, newImage.Height), System.Drawing.Imaging.PixelFormat.DontCare);
				//croppedBitmap.Save(picSlim, System.Drawing.Imaging.ImageFormat.Jpeg);
				SaveJpeg(picSlim, croppedBitmap, 100);
				croppedBitmap.Dispose();
				newImage.Dispose();

				// delete covertemp.jpg
				if (File.Exists(tempIMGPath))
					File.Delete(tempIMGPath);
			}
			else if ((((float)(newImage.Height) / (float)(newImage.Width)) > ((float)4 / (float)3)) && (((float)(newImage.Height) / (float)(newImage.Width)) < ((float)16 / (float)9)))
			{
				// create cover
				pic = tempPath + "\\" + tempIMGNamePart1 + ".jpg";
				int newHeight = 4 * newImage.Width / 3;
				Bitmap croppedBitmap = new Bitmap(newImage);
				croppedBitmap = croppedBitmap.Clone(new Rectangle(0, (newImage.Height / 2 - newHeight / 2), newImage.Width, newHeight), System.Drawing.Imaging.PixelFormat.DontCare);
				//croppedBitmap.Save(pic, System.Drawing.Imaging.ImageFormat.Jpeg);
				SaveJpeg(pic, croppedBitmap, 100);
				croppedBitmap.Dispose();

				// create cover~slim
				picSlim = tempPath + "\\" + tempIMGNamePart1 + "~slim.jpg";
				int newWidth = 9 * newImage.Height / 16;
				croppedBitmap = new Bitmap(newImage);
				croppedBitmap = croppedBitmap.Clone(new Rectangle((newImage.Width / 2 - newWidth / 2), 0, newWidth, newImage.Height), System.Drawing.Imaging.PixelFormat.DontCare);
				//croppedBitmap.Save(picSlim, System.Drawing.Imaging.ImageFormat.Jpeg);
				SaveJpeg(picSlim, croppedBitmap, 100);
				croppedBitmap.Dispose();
				newImage.Dispose();

				// delete covertemp.jpg
				if (File.Exists(tempIMGPath))
					File.Delete(tempIMGPath);
			}
			else		// ((float)(newImage.Height) / (float)(newImage.Width)) >= ((float)16 / (float)9)
			{
				// create cover
				pic = tempPath + "\\" + tempIMGNamePart1 + ".jpg";
				int newHeight = 4 * newImage.Width / 3;
				Bitmap croppedBitmap = new Bitmap(newImage);
				croppedBitmap = croppedBitmap.Clone(new Rectangle(0, (newImage.Height / 2 - newHeight / 2), newImage.Width, newHeight), System.Drawing.Imaging.PixelFormat.DontCare);
				//croppedBitmap.Save(pic, System.Drawing.Imaging.ImageFormat.Jpeg);
				SaveJpeg(pic, croppedBitmap, 100);
				croppedBitmap.Dispose();

				// create cover~slim
				picSlim = tempPath + "\\" + tempIMGNamePart1 + "~slim.jpg";
				newHeight = 16 * newImage.Width / 9;
				croppedBitmap = new Bitmap(newImage);
				croppedBitmap = croppedBitmap.Clone(new Rectangle(0, (newImage.Height / 2 - newHeight / 2), newImage.Width, newHeight), System.Drawing.Imaging.PixelFormat.DontCare);
				//croppedBitmap.Save(picSlim, System.Drawing.Imaging.ImageFormat.Jpeg);
				SaveJpeg(picSlim, croppedBitmap, 100);
				croppedBitmap.Dispose();
				newImage.Dispose();

				// delete covertemp.jpg
				if (File.Exists(tempIMGPath))
					File.Delete(tempIMGPath);
			}
		}

		private static String manualTextWrap(String s, int length, bool vertical)
		{
			List<String> wordList = new List<String>();
			for (int i = 0; i < s.Length; i += length)
			{
				if ((i + length) > s.Length)
				{
					wordList.Add(s.Substring(i, s.Length-i));
				}
				else
				{
					wordList.Add(s.Substring(i, length));
				}
			}

			if (!vertical)
				wordList.Reverse();

			StringBuilder result = new StringBuilder();
			for (int i = 0; i < wordList.Count; i++)
			{
				result.Append(wordList[i] + "\n");
			}
			return result.ToString().Trim();
		}

		private static Tuple<String, int> setTitleFontSize(int len, int width, String s, bool vertical)
		{
			if (len == 1) return new Tuple<String, int>(s, width / 50 * 33);
			else if (len == 2) return new Tuple<String, int>(s, width / 50 * 27);
			else if (len == 3) return new Tuple<String, int>(s, width / 50 * 22);
			else if (len == 4) return new Tuple<String, int>(manualTextWrap(s, 4, vertical), width / 50 * 17);
			else if (len == 5) return new Tuple<String, int>(manualTextWrap(s, 5, vertical), width / 50 * 15);
			else if (len == 6) return new Tuple<String, int>(manualTextWrap(s, 6, vertical), width / 50 * 12);
			else if (len == 7) return new Tuple<String, int>(manualTextWrap(s, 7, vertical), width / 50 * 11);
			else return setTitleFontSize((int)Math.Ceiling((double)len / 2), width, s, vertical);
		}

		private static int translationState(bool StT, bool TtS)
		{
			// return 0: 不转; 1: 简转繁; 2: 繁转简
			if (StT && TtS)
			{
				return 0;
			}
			else if (StT && !TtS)
			{
				return 1;
			}
			else if (!StT && TtS)
			{
				return 2;
			}
			else return 0;
		}

		private static String translate(String s, int translation)
		{
			VB.VbStrConv vbTranslation = VB.VbStrConv.None;
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
			return VB.Strings.StrConv(s, vbTranslation, 0);
		}

		private static void SaveJpeg(string path, Image img, int quality)
		{
			if (quality < 0 || quality > 100)
				throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");


			// Encoder parameter for image quality 
			EncoderParameter qualityParam =
				new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
			// Jpeg image codec 
			ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

			EncoderParameters encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = qualityParam;
			img.Save(path, jpegCodec, encoderParams);
		}

		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for (j = 0; j < encoders.Length; j++)
			{
				if (encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

		private void CreateFontSubSetT()
		{
			if (titleFontPath.CompareTo("") != 0 && URIT != null)
			{
				int wordCount = IndexT.Count;
				if (wordCount <= 0)
				{
					MessageBox.Show("不需要内嵌字体！");
					return;
				}
				else if (wordCount > 65535)
				{
					MessageBox.Show("字符太多！将用字体全集！");
					File.Copy(URIT.AbsolutePath, (tempPath + "\\title.ttf"));
					return;
				}
				else
				{
					byte[] filebytes = glyphTypefaceT.ComputeSubset(IndexT);
					String newFontPath = tempPath + "\\title.ttf";
					embedFontPaths.Add(newFontPath);
					using (FileStream fileStream = new FileStream(newFontPath, FileMode.Create))
					{
						fileStream.Write(filebytes, 0, filebytes.Length);
					}
				}
			}
		}

		private void CreateFontSubSetB()
		{
			if (bodyFontPath.CompareTo("") != 0 && URIB != null)
			{
				int wordCount = IndexB.Count;
				if (wordCount <= 0)
				{
					MessageBox.Show("不需要内嵌字体！");
					return;
				}
				else if (wordCount > 65535)
				{
					MessageBox.Show("字符太多！将用字体全集！");
					File.Copy(URIB.AbsolutePath, (tempPath + "\\body.ttf"));
					return;
				}
				else
				{
					byte[] filebytes = glyphTypefaceB.ComputeSubset(IndexB);
					String newFontPath = tempPath + "\\body.ttf";
					embedFontPaths.Add(newFontPath);
					using (FileStream fileStream = new FileStream(newFontPath, FileMode.Create))
					{
						fileStream.Write(filebytes, 0, filebytes.Length);
					}
				}
			}
		}
		
		private void addStringToUInt16CollectionT(String s)
		{
			Char[] temp = s.ToCharArray();
			// Remove duplicated chars
			// Only add unique chars to Index
			for (int i = 0; i < temp.Length; i++)
			{
				if (ALLTITLETEXT.Add(temp[i]))
				{
					try { IndexT.Add((UInt16)(glyphTypefaceT.CharacterToGlyphMap[Convert.ToInt32(temp[i])])); }
					catch { /* character not in CharacterToGlyphMap! */ }
				}
			}
		}

		private void addStringToUInt16CollectionB(String s)
		{
			Char[] temp = s.ToCharArray();
			// Remove duplicated chars
			// Only add unique chars to Index
			for (int i = 0; i < temp.Length; i++)
			{
				if (ALLBODYTEXT.Add(temp[i]))
				{
					try { IndexB.Add((UInt16)(glyphTypefaceB.CharacterToGlyphMap[Convert.ToInt32(temp[i])])); }
					catch { /* character not in CharacterToGlyphMap! */ }
				}
			}
		}

		#endregion
	}
}
