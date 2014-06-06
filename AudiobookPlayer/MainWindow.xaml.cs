using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Interop;

namespace AudiobookPlayer
{
	public partial class MainWindow : Window
	{
        public delegate void AudiobookEventHandler(object source, AudioBookArgs e);

		volatile int running_threads = 0;
		volatile List<Audiobook> audiobooks = new List<Audiobook>();
		Audiobook current_audiobook;
		System.Windows.Threading.DispatcherTimer audiobook_update_timer;
		Config config = new Config();
		Size window_size;

		public MainWindow()
		{
			InitializeComponent();
			ThreadPool.SetMaxThreads(config.NoOfThreads, config.NoOfThreads);
            ReadAudiobookFolder();
			ToggleSidebar();
		}

        void ReadAudiobookFolder()
        {
			string path = "";
			try
			{ path = GetAudiobookFolder(); }
			catch (FileNotFoundException) { MessageBox.Show("Audiobook folder could not be found. Please enter the correct path in the settings menu.", "Audiobook Player", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            List<string> sub_folders = Utilities.GetFoldersInFolder(path);
			foreach (string s in sub_folders)
			{
				AudiobookScan scan = new AudiobookScan(s);
				scan.OnScanFinished += AudiobookScan_OnScanFinished;
				ThreadPool.QueueUserWorkItem(scan.Start);
				Running_Threads++;
			}
        }

		void SetupAudiobookUpdateTimer(int interval = 1)
		{
			audiobook_update_timer = new System.Windows.Threading.DispatcherTimer();
			audiobook_update_timer.Tick += audiobook_update_timer_Tick;
			audiobook_update_timer.Interval = new TimeSpan(0, 0, interval);
		}

		void StartAudiobookUpdateTimer()
		{
			if (audiobook_update_timer == null)
				SetupAudiobookUpdateTimer();
			audiobook_update_timer.Start();
		}

		void StopAudiobookUpdateTimer()
		{
			if (audiobook_update_timer != null)
				audiobook_update_timer.Stop();
		}

		void UpdateCurrentAudiobook()
		{
			current_audiobook.UpdateStats(config.AudiobookUpdateIntervall);
			UpdateAudiobookControls();
		}

		void UpdateAudiobookControls()
		{
			GetControlForAudiobook(current_audiobook).pbProgress.Value = current_audiobook.Progress;
			pbNowPlayingProgress.Value = current_audiobook.Progress;
			lblProgress.Content = current_audiobook.PositionAsTimeSpan.ToString() + " / " + current_audiobook.LengthAsTimeSpan.ToString() + " (" + current_audiobook.Name + ")";
		}

		void AudiobookScan_OnScanFinished(object source, AudioBookArgs e)
		{
			Dispatcher.Invoke(new Action(() => { var control = new AudiobookControl(e.Audiobook); control.Margin = new Thickness(5); wpAudiobooks.Children.Add(control); control.MouseDoubleClick += AudiobookControl_MouseDoubleClick; control.ContextMenu = (ContextMenu)this.Resources["Audiobook_Context_Menu"]; }));
			audiobooks.Add(e.Audiobook);
			Running_Threads--;
		}

        string GetAudiobookFolder()
        {
            string audiobook_path = config.AudiobookPath;
            if (Directory.Exists(audiobook_path))
                return audiobook_path;
            else
                throw new FileNotFoundException("Unable to find audiobook folder.");
        }

		void SelectAudiobook(Audiobook audiobook)
		{
			if (current_audiobook != null)
			{
				current_audiobook.Stop();
				DeselectCurrentAudiobook();
			}
			this.DataContext = audiobook;
			DeselectAllAudiobookControls();
			current_audiobook = audiobook;
			GetControlForAudiobook(audiobook).IsSelected = true;
			lstBookmarks.ItemsSource = current_audiobook.Bookmarks;
			UpdateAudiobookControls();
		}

		void DeselectCurrentAudiobook()
		{
			if (current_audiobook != null)
			{
				current_audiobook.Stop();
				cmdPlay.IsChecked = false;
			}

			if (audiobook_update_timer != null)
				audiobook_update_timer.IsEnabled = false;
		}

		void DeselectAllAudiobookControls()
		{
			foreach (AudiobookControl control in wpAudiobooks.Children)
				control.IsSelected = false;
		}

		AudiobookControl GetControlForAudiobook(Audiobook book)
		{
			foreach (AudiobookControl control in wpAudiobooks.Children)
				if (control.Audiobook == book)
					return control;
			throw new ArgumentException("Cannot find an audiobook control matching the audiobook.");
		}

		private void Audiobook_Play()
		{
			if (current_audiobook != null)
			{
				if (current_audiobook.IsPlaying)
				{
					current_audiobook.Stop();
					StopAudiobookUpdateTimer();
				}
				else
				{
					current_audiobook.Play();
					StartAudiobookUpdateTimer();
				}
			}
			else
			{
				// in case the button was clicked the ui changes it's state to checked so we have to cancel that
				if (cmdPlay.IsChecked == true)
					cmdPlay.IsChecked = false;
			}
		}

		private void Audiobook_Stop()
		{
			if (current_audiobook != null)
				current_audiobook.Stop();
			StopAudiobookUpdateTimer();
		}

		private int Running_Threads
		{ 
			get { return running_threads; }
			set
			{
				int old_number = running_threads;
				running_threads = value;
				if (old_number == 0 && running_threads > 0)
				{
					pbCurrentActivity.IsIndeterminate = true;
					Dispatcher.Invoke(new Action(() => { sbStatus.Visibility = System.Windows.Visibility.Visible; }));
				}
				if (running_threads == 0)
					Dispatcher.Invoke(new Action(() => { pbCurrentActivity.IsIndeterminate = false; sbStatus.Visibility = System.Windows.Visibility.Collapsed; }));
				Dispatcher.Invoke(new Action(() => { lblStatusBar.Content = "Current job is running on " + running_threads + " threads."; }));
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Shutdown();
		}

		private void Shutdown()
		{
			foreach (Audiobook book in audiobooks)
				book.Serialize();
			config.SaveConfig();
		}

		private void Skip(double seconds)
		{
			if (current_audiobook != null)
			{
				current_audiobook.Position += seconds;
				UpdateAudiobookControls();
			}
		}

		private void CollapsePlayer()
		{
			svAudiobooks.Visibility = System.Windows.Visibility.Collapsed;
			window_size = new Size(this.Width, this.Height);
			if (Utilities.DwmIsCompositionEnabled())
			{
				this.MinHeight = 70;
				this.MaxHeight = 70;
				this.MinWidth = 585;
				this.MaxWidth = 585;
			}
			else
			{
				this.MinHeight = 59;
				this.MaxHeight = 59;
				this.MinWidth = 585;
				this.MaxWidth = 585;
			}
		}

		private void EnlargePlayer()
		{
			svAudiobooks.Visibility = System.Windows.Visibility.Visible;
			this.MinHeight = 485;
			this.MaxHeight = 485;
			this.MinWidth = 585;
			this.MaxWidth = Int32.MaxValue;
			//Application.Current.MainWindow.Width = window_size.Width;
			this.Width = window_size.Width;
		}

		private void SelectCover(Audiobook book)
		{
			var selector = new ImageSelector(book);
			selector.ShowDialog();

			if (selector.DialogResult.HasValue && selector.DialogResult.Value && selector.SelectedImage != null)
				book.Cover = selector.SelectedImage;
		}

		private void ToggleSidebar()
		{
			if (dpSidebar.Visibility == System.Windows.Visibility.Visible)
			{
				dpSidebar.Visibility = System.Windows.Visibility.Collapsed;
				cmdSidebar.IsChecked = false;
			}
			else
			{
				dpSidebar.Visibility = System.Windows.Visibility.Visible;
				cmdSidebar.IsChecked = true;
			}
		}

		private void AddBookmark()
		{
			if(current_audiobook != null)
				current_audiobook.Bookmarks.Add(new Bookmark(current_audiobook.Position));
		}
		
		private void RemoveBookmark()
		{

		}

		private void RenameBookmark()
		{

		}

		private void ShowGotoDialog(Audiobook audiobook)
		{
			GotoDialog dialog = new GotoDialog(audiobook);
			dialog.ShowDialog();
		}
		#region Control Events

		private void RefreshAudiobooks_Click(object sender, RoutedEventArgs e)
		{
			Audiobook_Stop();
			current_audiobook = null;
			audiobooks.Clear();
			wpAudiobooks.Children.Clear();
			ReadAudiobookFolder();
		}

		private void cmdPlay_Click(object sender, RoutedEventArgs e)
		{
			Audiobook_Play();
		}

		private void cmdPause_Click(object sender, RoutedEventArgs e)
		{
			Audiobook_Stop();
		}

		void AudiobookControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			AudiobookControl control = (AudiobookControl)sender;
			Audiobook book = audiobooks.Single(o => o.Name == control.Text);
			SelectAudiobook(book);
		}

		void audiobook_update_timer_Tick(object sender, EventArgs e)
		{
			UpdateCurrentAudiobook();
		}

		private void cmdBigSkipBackward_Click(object sender, RoutedEventArgs e)
		{
			Skip(-600);
		}

		private void cmdSmallSkipBackward_Click(object sender, RoutedEventArgs e)
		{
			Skip(-60);
		}

		private void cmdSmallSkipForward_Click(object sender, RoutedEventArgs e)
		{
			Skip(60);
		}

		private void cmdBigSkipForward_Click(object sender, RoutedEventArgs e)
		{
			Skip(600);
		}

		private void cmdSettingsDialog_Click(object sender, RoutedEventArgs e)
		{
			SettingsDialog dialog = new SettingsDialog(new Config(config));
			dialog.ShowDialog();
			if(dialog.DialogResult.HasValue && dialog.DialogResult.Value)
			{
				this.config = dialog.Config;
			}
		}

		private void cmdMicroPlayer_Click(object sender, RoutedEventArgs e)
		{
			// remember that the state of the checked property is changed before this method is called!
			if (cmdMicroPlayer.IsChecked == true)
				CollapsePlayer();
			else
				EnlargePlayer();
		}

		private void cmSelectAudiobook_Click(object sender, RoutedEventArgs e)
		{
			var control = GetAudiobookControlFromContextMenuClick(sender);
			SelectAudiobook(control.Audiobook);
		}

		private void cmSearchAudiobookCover_Click(object sender, RoutedEventArgs e)
		{
			var control = GetAudiobookControlFromContextMenuClick(sender);
			SelectCover(control.Audiobook);
		}

		private void cmdSidebar_Click(object sender, RoutedEventArgs e)
		{
			ToggleSidebar();
		}

		private AudiobookControl GetAudiobookControlFromContextMenuClick(object sender)
		{
			MenuItem menu_item = sender as MenuItem;
			if (menu_item != null)
			{
				ContextMenu context_menu = menu_item.CommandParameter as ContextMenu;
				if (context_menu != null)
				{
					return context_menu.PlacementTarget as AudiobookControl;
				}
			}
			return null;
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F9)
				ToggleSidebar();
		}
		#endregion

		private void cmdRenameBookmark_Click(object sender, RoutedEventArgs e)
		{

		}

		private void cmdDeleteBookmark_Click(object sender, RoutedEventArgs e)
		{

		}

		private void cmdAddBookmark_Click(object sender, RoutedEventArgs e)
		{
			AddBookmark();
		}

		private void cmdRemoveBookmark_Click(object sender, RoutedEventArgs e)
		{
			RemoveBookmark();
		}

		private void cmdGoTo_Click(object sender, RoutedEventArgs e)
		{
			ShowGotoDialog(current_audiobook);
		}
	}

	class AudiobookScan
	{
		public delegate void ScanCompleteHandler(object source, AudioBookArgs e);
		public event ScanCompleteHandler OnScanFinished;
		string folder = "";

		public AudiobookScan(string folder)
		{
			this.folder = folder;
		}

		public void Start(Object thread_context)
		{
			ScanFolder(folder);
		}

		void ScanFolder(string path)
		{
			Audiobook new_book = Audiobook.FromFolder(path);
			OnScanFinished(this, new AudioBookArgs(new_book, Thread.CurrentThread));
		}
	}

	public class AudioBookArgs : EventArgs
	{
		private Audiobook audiobook;

		public AudioBookArgs(Audiobook audiobook, Thread thread = null)
		{ this.audiobook = audiobook; }

		public Audiobook Audiobook
		{ get { return this.audiobook; } }
	}
}
