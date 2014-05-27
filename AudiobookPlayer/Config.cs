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
	public class Config : IDataErrorInfo, INotifyPropertyChanged
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
			SetDefaultValues();
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
			ConfigurationManager.AppSettings[UPDATE_INTERVALL_IDENT] = audiobook_update_intervall.ToString();
			ConfigurationManager.AppSettings[SMALL_SKIP_SECONDS_IDENT] = small_skip_seconds.ToString();
			ConfigurationManager.AppSettings[LARGE_SKIP_SECONDS_IDENT] = large_skip_seconds.ToString();
			ConfigurationManager.AppSettings[NO_OF_THREADS_IDENT] = no_of_background_threads.ToString();
			ConfigurationManager.AppSettings[AUDIOBOOK_PATH] = audiobook_path;
		}

		private void SetDefaultValues()
		{
			audiobook_update_intervall = 1.0;
			small_skip_seconds = 60.0;
			large_skip_seconds = 600;
			no_of_background_threads = 2;
			audiobook_path = "..\\..\\..\\..\\Audiobooks";
		}

		private void ReadConfigValues()
		{
			//TODO: Wenn ein Key nicht gefunden wird, wird abgebrochen! Dies sollte umgangen werden.
			bool init_failed = false;

			init_failed = init_failed || !double.TryParse(ConfigurationManager.AppSettings[UPDATE_INTERVALL_IDENT],out audiobook_update_intervall);
			init_failed = init_failed || !double.TryParse(ConfigurationManager.AppSettings[SMALL_SKIP_SECONDS_IDENT], out small_skip_seconds);
			init_failed = init_failed || !double.TryParse(ConfigurationManager.AppSettings[LARGE_SKIP_SECONDS_IDENT], out large_skip_seconds);
			init_failed = init_failed || !int.TryParse(ConfigurationManager.AppSettings[NO_OF_THREADS_IDENT], out no_of_background_threads);
			audiobook_path = ConfigurationManager.AppSettings[AUDIOBOOK_PATH];

			if (init_failed)
				System.Windows.Forms.MessageBox.Show("At least one config value if of the wrong format. Using default value.", System.Windows.Forms.Application.ProductName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
		}

		private T GetConfigSetting<T>(string identifier)
		{
			string setting = ConfigurationManager.AppSettings[identifier];
			if(typeof(T).IsSubclassOf(typeof(Enum)))
			{
				return (T)Enum.Parse(typeof(T), setting, true);
			}

			if (!String.IsNullOrEmpty(setting))
				return (T)Convert.ChangeType(setting, typeof(T));

			return default(T);
		}

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, e);
		}
		#endregion

		#region IDataErrorInfo Members
		public string Error
		{ get { return this[null]; } }

		public string this[string property_name]
		{
			get
			{
				string result = string.Empty;
				property_name = property_name ?? string.Empty;
				if(property_name == string.Empty || property_name == "AudiobookPath")
				{
					if(string.IsNullOrEmpty(this.AudiobookPath))
					{
						result += "Audiobook path cannot be empty." + Environment.NewLine;
					}
				}

				if(property_name == string.Empty || property_name == "AudiobookUpdateIntervall")
				{
					if(AudiobookUpdateIntervall < 1)
					{
						result += "Audiobook update intervall cannot be less than one." + Environment.NewLine;
					}
				}

				return result.TrimEnd();
			}
		}
		#endregion

		#region Properties
		public double AudiobookUpdateIntervall
		{
			get { return audiobook_update_intervall; }
			set 
			{ 
				audiobook_update_intervall = value;
				this.OnPropertyChanged(new PropertyChangedEventArgs("AudiobookUpdateIntervall"));
			}
		}

		public string AudiobookUpdateIntervallAsString
		{
			get { return audiobook_update_intervall.ToString(); }
			set 
			{ 
				double new_value = -1.0;
				if(!double.TryParse(value, out new_value))
				{
					throw new ApplicationException("Cannot convert string to double.");
				}
				AudiobookUpdateIntervall = new_value;
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
