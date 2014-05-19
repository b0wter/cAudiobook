using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace AudiobookPlayer
{
	static class Utilities
	{
		public static List<string> GetFilesInFolder(string path, string pattern = "*", bool recursive = true)
		{
			SearchOption sopt;
			sopt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			return Directory.GetFiles(path, pattern, sopt).ToList<string>();
		}

		public static Dictionary<string, List<string>> GetFilesAndRootDirectories(string path, string pattern = "*")
		{
			Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
			List<string> folders = Directory.GetDirectories(path).ToList<string>();

			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				path += Path.DirectorySeparatorChar.ToString();

			foreach (string s in folders)
			{
				List<string> files = GetFilesInFolder(s, pattern, true);
				result.Add(s.Replace(path,""), files);
			}
			return result;
		}

		public static List<string> GetFoldersInFolder(string path)
		{
			if (!Directory.Exists(path))
				throw new FileNotFoundException("Could not open " + path);

			return Directory.GetDirectories(path).ToList<string>();
		}

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteObject(IntPtr value);

		public static BitmapSource BitmapSourceFromImage(System.Drawing.Image image)
		{
			var bitmap = new System.Drawing.Bitmap(image);
			IntPtr bmpPtr = bitmap.GetHbitmap();
			BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPtr, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			bitmapSource.Freeze();
			DeleteObject(bmpPtr);
			return bitmapSource;
		}

		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();
	}
}
