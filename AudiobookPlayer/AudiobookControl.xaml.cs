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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudiobookPlayer
{
	/// <summary>
	/// Interaction logic for AudiobookControl.xaml
	/// </summary>
	public partial class AudiobookControl : UserControl
	{
		public delegate void FadeOutHandler(object sender, EventArgs e);
		public event FadeOutHandler OnFadeOut;

		Audiobook audiobook;

		private bool isSelected = false;

		public AudiobookControl()
		{
			InitializeComponent();
			dockContent.IsVisibleChanged += dockContent_IsVisibleChanged;
		}

		public AudiobookControl(Audiobook book)
		{
			InitializeComponent();
			dockContent.IsVisibleChanged += dockContent_IsVisibleChanged;
			audiobook = book;
			this.Text = book.Name;
			if (book.Cover != null)
				imgCover.Source = Utilities.BitmapSourceFromImage(book.Cover);
			book.OnCoverSearchFinished += book_OnCoverSearchFinished;
			SetupProgressBar(book.Progress);
		}

		void book_OnCoverSearchFinished(object source, ImageSearchEventArgs e)
		{
			Dispatcher.Invoke(new Action(() => { this.imgCover.Source = Utilities.BitmapSourceFromImage(e.Results[0]); }));
		}

		void SetupProgressBar(double progress)
		{
			pbProgress.Minimum = 0;
			pbProgress.Maximum = 1;
			pbProgress.Value = progress;
		}

		void dockContent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			
		}

		public Audiobook Audiobook
		{ get { return audiobook; } }

		public double Value
		{
			get { return pbProgress.Value; }
			set { pbProgress.Value = Math.Max(Math.Min(100, value), 0); }
		}

		public string Text
		{
			get { return (string)lblName.Content; }
			set { lblName.Content = value; }
		}

		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				if (value)
				{
					lblName.Background = SystemColors.ControlBrush;
					dockContent.Background = SystemColors.ControlBrush;
				}
				else
				{
					lblName.Background = SystemColors.ControlDarkBrush;
					dockContent.Background = SystemColors.ControlDarkBrush;
				}
			}
		}

		private void Storyboard_Completed(object sender, EventArgs e)
		{

		}
	}
}
