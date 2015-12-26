﻿using System;
using System.Drawing.Text;
using System.IO;

namespace SimpleEpub
{
	public class FontNameFile
	{
		public static String getFontFileName(String fontname)
		{
			string folderFullName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts);
			DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
			foreach (FileInfo NextFile in TheFolder.GetFiles())
			{
				if (NextFile.Exists)
				{
					String result = getFontName(NextFile.FullName);
					//MessageBox.Show(fontname + "\n" + NextFile.FullName + "\n" + result);
					if (fontname == result) return NextFile.FullName;
				}
			}
			return "";
		}

		private static String getFontName(String fontfilename)
		{
			String ext = fontfilename.Substring(fontfilename.LastIndexOf(".") + 1).ToUpper();
			//MessageBox.Show(fontfilename + "\n" + ext);
			if (ext.CompareTo("TTF") == 0)
			{
				PrivateFontCollection pfc = new PrivateFontCollection();
				try
				{
					pfc.AddFontFile(fontfilename);
				}
				catch (Exception)
				{
					// return "";
				}
				return (pfc.Families[0].GetName(0));
			}
			else
				return "";
		}
	}
}
