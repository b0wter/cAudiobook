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
using System.Threading;

namespace AudiobookPlayer
{
	/// <summary>
	/// Interaction logic for ImageSelector.xaml
	/// </summary>
	public partial class ImageSelector : Window
	{
		private System.Drawing.Image selected_image = null;
		private List<System.Drawing.Image> images = null;

		/// <summary>
		/// Creates a new instance of the ImageSelector that will perform it's own search.
		/// </summary>
		/// <param name="book">Book that will be searched for</param>
		/// <param name="no_of_results">Number of results that the user will be presented.</param>
		public ImageSelector(Audiobook book, int no_of_results = 8)
		{
			InitializeComponent();
			SearchImages(book.Name, no_of_results);
		}

		/// <summary>
		/// Creates a new instance of the ImageSelector that will give the user a choice of predefined images.
		/// </summary>
		/// <param name="images"></param>
		public ImageSelector(IEnumerable<System.Drawing.Image> images)
		{
			InitializeComponent();
			ShowImages(images);
		}

		private void SearchImages(string name, int no_of_results)
		{
			ImageSearch image_search = new ImageSearch(name, no_of_results, no_of_results * 4);
			image_search.OnFinished += image_search_OnFinished;
			ThreadPool.QueueUserWorkItem(image_search.Start);
		}

		void image_search_OnFinished(object source, ImageSearchEventArgs e)
		{
			images = e.Results;
			Dispatcher.Invoke(new Action(() => { ShowImages(e.Results); }));
		}
		
		private void ShowImages(IEnumerable<System.Drawing.Image> images)
		{
			foreach(System.Drawing.Image image in images)
			{
				BitmapSource bitmap_source = Utilities.BitmapSourceFromImage(image);
				var image_control = CreateImageControl(bitmap_source);
				wpImages.Children.Add(image_control);
			}
			imgWaiting.Visibility = System.Windows.Visibility.Collapsed;
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
				selected_image = images[wpImages.Children.IndexOf((UIElement)sender)];
				DialogResult = true;
			}
		}

		public System.Drawing.Image SelectedImage
		{ get { return selected_image; } }
	}
}
