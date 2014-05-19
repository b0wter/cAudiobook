using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NAudio;

namespace AudiobookPlayer
{
	[Serializable]
	public class Audiobook 
	{		
		string name;
		double length;
		double position;
		string path;
		SortedList<string, double> files;
		System.Drawing.Image image = null;

		[NonSerialized]
		KeyValuePair<string, double> current_file;
		[NonSerialized]
		AudioPlayer audioPlayer;
		[NonSerialized]
		bool isPlaying = false;

		public event SearchEventHandler OnCoverSearchFinished;

		#region Constructors / Audiobook Creation Process
		private Audiobook()
		{
			name = "unnamed";
			length = 0;
			position = 0;
			path = "";
			files = new SortedList<string, double>();
		}

		public static Audiobook FromFolder(string path)
		{
			if (FolderContainsStateFile(path))
				return FromSerializedFile(GetStateFile(path));
			else
				return FromFolderFiles(path);
		}

		public static Audiobook FromFolderFiles(string path)
		{
			Audiobook book = new Audiobook(); 
			List<string> raw_files = book.ReadFilesFromFolder(path, "*.mp3");
			book.files = book.ReadMediaLength(raw_files);
			book.length = book.ComputeTotalLength();
			book.position = 0;
			book.path = path;
			book.name = new System.IO.DirectoryInfo(path).Name;
			book.SetCoverImage();
			return book;
		}

		public static Audiobook FromSerializedFile(string filename)
		{
			IFormatter formatter = new BinaryFormatter();
			using(Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{ 
				Audiobook book = (Audiobook)formatter.Deserialize(stream); 
				return book; 
			}
			throw new SerializationException("There was a problem deserializing the state file.");
		}

		private void SetCoverImage()
		{
			ImageSearch image_search = new ImageSearch(this.Name, 1, 10);
			image_search.OnFinished += Image_Search_OnFinished;
			System.Threading.ThreadPool.QueueUserWorkItem(image_search.Start);
		}

		void Image_Search_OnFinished(object source, ImageSearchEventArgs e)
		{
			image = e.Results[0];
			if(OnCoverSearchFinished != null)
			{
				List<System.Drawing.Image> temp_list = new List<System.Drawing.Image>(1);
				temp_list.Add(image);
				OnCoverSearchFinished(this, new ImageSearchEventArgs(temp_list));
			}
		}

        private static bool FolderContainsStateFile(string path)
        { return System.IO.File.Exists(path + System.IO.Path.DirectorySeparatorChar.ToString() + "state.bin"); }

		private void InitFromSerializationFile(string filename)
		{
			filename = GetStateFile(filename);
		}

        private static string GetStateFile(string path)
        {
            if ((System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                return path + System.IO.Path.DirectorySeparatorChar.ToString() + "state.bin";
            else if (System.IO.Path.GetFileName(path) == "state.bin")
                return path;
            else
                throw new ArgumentException("Given path does not point to a directory or a serialization file.");
        }

		private List<string> ReadFilesFromFolder(string path, string pattern)
		{
			if (System.IO.Directory.Exists(path) == false)
				throw new System.IO.FileNotFoundException("Could not find folder " + path);
			return Utilities.GetFilesInFolder(path, pattern, true);
		}

		private SortedList<string, double> ReadMediaLength(List<string> files)
		{
			SortedList<string, double> audiobook_files = new SortedList<string, double>();
			files.Sort();
			foreach (string s in files)
			{
				Debug.WriteLine("Trying to get media information from " + s);
				NAudio.Wave.AudioFileReader reader = new NAudio.Wave.AudioFileReader(s);
				audiobook_files.Add(s, reader.TotalTime.TotalSeconds);
			}
			return audiobook_files;
		}

		public void Serialize(string filename = "")
		{
			// in case the filename was not set generate a default one "$AUDIOBOOK_PATH\state.bin"
			string target_file = (filename == "" ? this.Path + System.IO.Path.DirectorySeparatorChar.ToString() + "state.bin" : filename);
			IFormatter formatter = new BinaryFormatter();
			OnCoverSearchFinished = null;
			using (Stream stream = new FileStream(target_file, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				formatter.Serialize(stream, this);
			}
		}

		private double ComputeTotalLength()
		{
			double total_time = 0;
			foreach (KeyValuePair<string, double> pair in files)
				total_time += pair.Value;
			return total_time;
		}
		#endregion

		#region Playback
		/// <summary>
		/// Start playback with the assumption that the user wants to continue where he stopped. Uses the position as reference.
		/// </summary>
		public void Play()
		{
			if (!isPlaying)
				PlayFile(AbsolutePositionToFileAndPosition(position));
		}

		private void PlayFile(KeyValuePair<string, double> file)
		{
			StopPlayback();
			EnsureAudioPlayer();
			current_file = file;
			audioPlayer.Play(current_file);
			isPlaying = true;
		}

		private void StopPlayback()
		{
			if (audioPlayer != null)
				audioPlayer.Stop();
		}

		private void EnsureAudioPlayer()
		{
			if (audioPlayer == null)
			{
				audioPlayer = new AudioPlayer();
				audioPlayer.OnFinished += audioPlayer_OnFinished;
			}
		}

		void audioPlayer_OnFinished(object source, PlaybackEventArgs e)
		{
			//TODO: beim Pollen der Position den Filewechsel berücksichtigen mit einem negativen Wert!
			int nextIndex = files.IndexOfKey(e.CurrentFile) + 1;
			PlayFile(new KeyValuePair<string, double>(files.ElementAt(nextIndex).Key, 0));
		}

		public void Stop()
		{
			if(audioPlayer != null)
			{
				audioPlayer.Pause();
				isPlaying = false;
			}
		}

		private double FileAndPositionToAbsolutePosition(string file, double position)
		{
			double length = 0;
			int i = 0;
			while(files.ElementAt(i).Key != file)
			{
				length += files.ElementAt(i).Value;
				i++;
			}
			return length + position;
		}

		/// <summary>
		/// Computes the file and position in the file for a given absolute position.
		/// </summary>
		/// <remarks>If the absolute position is a position outside of the audiobook the function will return either 0 or a position at the very end.</remarks>
		/// <param name="absolute_position"></param>
		/// <returns></returns>
		private KeyValuePair<string, double> AbsolutePositionToFileAndPosition(double absolute_position)
		{
			if (absolute_position <= 0)
				return new KeyValuePair<string, double>(files.First().Key, 0);

			double current_position = 0;
			for (int i = 0; i < files.Count; i++)
			{
				if (current_position + files.ElementAt(i).Value > absolute_position)
					return new KeyValuePair<string, double>(files.ElementAt(i).Key, absolute_position - current_position);
				else
					current_position += files.ElementAt(i).Value;
			}
			return files.Last();
			//throw new ArgumentException("Absolute position is not inside the audiobook.");
		}

		public void UpdateStats(double seconds_passed)
		{
			position += seconds_passed;
		}

		#endregion
		
		#region Properties
		public string Name
		{ get { return name; } }

		public double Progress
		{ get { return position / length; } }

		public SortedList<string, double> Files
		{ get { return files; } }

		public string Path
		{ get { return path; } }

		public double Position
		{ 
			get { return position; }
			set
			{
				position = Math.Min(Math.Max(0, value), length);
				KeyValuePair<string, double> pair;
				pair = AbsolutePositionToFileAndPosition(value);
				current_file = pair;
				if (audioPlayer != null && isPlaying)
				{
					audioPlayer.Stop();
					audioPlayer.Play(pair.Key, pair.Value);
				}
			}
		}

		public TimeSpan PositionAsTimeSpan
		{ get { return new TimeSpan(0, 0, (int)position); } }

		public double Length
		{ get { return length; } }

		public TimeSpan LengthAsTimeSpan
		{ get { return new TimeSpan(0, 0, (int)length); } }

		public System.Drawing.Image Cover
		{ get { return image; } }

		public bool IsPlaying
		{ get { return isPlaying; } }
		#endregion
	}
}
