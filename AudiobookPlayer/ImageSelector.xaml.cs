using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace AudiobookPlayer
{
	/// <summary>
	/// Interaction logic for ImageSelector.xaml
	/// </summary>
	public partial class ImageSelector : Window
	{


		private ImageSource selected_image = null;

		public ImageSelector(System.Drawing.Image image)
		{
			InitializeComponent();
			List<System.Drawing.Image> images = new List<System.Drawing.Image>();
			images.Add(image);
			ShowImages(images);
		}

		public ImageSelector(IEnumerable<System.Drawing.Image> images)
		{
			InitializeComponent();
			ShowImages(images);
		}
		
		private void ShowImages(IEnumerable<System.Drawing.Image> images)
		{
			foreach(System.Drawing.Image image in images)
			{
				BitmapSource bitmap_source = Utilities.BitmapSourceFromImage(image);
				var image_control = CreateImageControl(bitmap_source);
				wpImages.Children.Add(image_control);
			}
		}

		private Image CreateImageControl(BitmapSource image)
		{
			Image image_control = new Image();
			image_control.Source = image;
			image_control.Margin = new Thickness(10);
			image_control.MouseDown += imageControl_MouseDown;
			return image_control;
		}

		void imageControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if(e.ClickCount >= 2)
			{
				selected_image = ((Image)sender).Source;
				DialogResult = true;
			}
		}

		//private BitmapSource BitmapSourceFromImage(System.Drawing.Image image)
		//{
		//	var bitmap = new System.Drawing.Bitmap(image);
		//	IntPtr bmpPtr = bitmap.GetHbitmap();
		//	BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		//	bitmapSource.Freeze();
		//	DeleteObject(bmpPtr);
		//	return bitmapSource;
		//}

		public ImageSource SelectedImage
		{ get { return selected_image; } }
	}
}
