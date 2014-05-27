using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;

namespace AudiobookPlayer
{
	//TODO: Klasse noch einmal hübscher machen. Das Auslesen der Werte ist so nicht sehr elegant und kann Fehler nach sich ziehen, siehe entsprechenden Kommentar.
	//
	public class Config //: IDataErrorInfo, INotifyPropertyChanged
	{
		const string UPDATE_INTERVALL_IDENT = "update_intervall_seconds";
		const string SMALL_SKIP_SECONDS_IDENT = "small_skip_seconds";
		const string LARGE_SKIP_SECONDS_IDENT = "big_skip_seconds";
		const string NO_OF_THREADS_IDENT = "max_background_threads";
		const string AUDIOBOOK_PATH = "audio_book_path";

		double audiobook_update_intervall;
		double small_skip_seconds;
		double large_skip_seconds;
		int no_of_background_threads;
		string audiobook_path;

		public Config()
		{
			ReadConfigValues();
		}

		/// <summary>
		/// Creates a copy of the class.
		/// </summary>
		/// <param name="config">Class instance that will be cloned.</param>
		public Config(Config config)
		{
			this.AudiobookPath = config.AudiobookPath;
			this.AudiobookUpdateIntervall = config.AudiobookUpdateIntervall;
			this.LargeSkipSeconds = config.LargeSkipSeconds;
			this.NoOfThreads = config.NoOfThreads;
			this.SmallSkipSeconds = config.SmallSkipSeconds;
		}

		public void SaveConfig()
		{
			Properties.Settings.Default.AudiobookUpdateIntervall = audiobook_update_intervall;
			Properties.Settings.Default.SmallSkipSize = small_skip_seconds;
			Properties.Settings.Default.LargeSkipSize = large_skip_seconds;
			Properties.Settings.Default.BackgroundThreads = no_of_background_threads;
			Properties.Settings.Default.AudiobookPath = audiobook_path;
			Properties.Settings.Default.Save();
		}

		private void ReadConfigValues()
		{
			audiobook_update_intervall = Properties.Settings.Default.AudiobookUpdateIntervall;
			small_skip_seconds = Properties.Settings.Default.SmallSkipSize;
			large_skip_seconds = Properties.Settings.Default.LargeSkipSize;
			no_of_background_threads = Properties.Settings.Default.BackgroundThreads;
			audiobook_path = Properties.Settings.Default.AudiobookPath;
		}

		#region Properties
		public double AudiobookUpdateIntervall
		{
			get { return audiobook_update_intervall; }
			set 
			{ 
				audiobook_update_intervall = value;
			}
		}

		public double SmallSkipSeconds
		{
			get { return small_skip_seconds; }
			set { small_skip_seconds = value; }
		}

		public double LargeSkipSeconds
		{
			get { return large_skip_seconds; }
			set { large_skip_seconds = value; }
		}

		public int NoOfThreads
		{ 
			get { return no_of_background_threads; }
			set { no_of_background_threads = value; }
		}

		public string AudiobookPath
		{
			get { return audiobook_path; }
			set 
			{
				if (Directory.Exists(value))
					audiobook_path = value;
				else
					throw new ApplicationException("Audiobook path does not point to an existing directory.");
			}
		}
		#endregion
	}
}
