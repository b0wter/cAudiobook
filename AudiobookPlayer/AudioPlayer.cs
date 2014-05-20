using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio;
using NAudio.Wave;

namespace AudiobookPlayer
{
	public class PlaybackEventArgs : EventArgs
	{
		private string current_file;

		public PlaybackEventArgs(string current_file)
		{ this.current_file = current_file; }

		public string CurrentFile
		{ get { return current_file; } }
	}

	class AudioPlayer
	{
		IWavePlayer waveOutDevice;
		AudioFileReader audioFileReader;
		string current_file = "";

		public delegate void PlaybackEventHandler(object source, PlaybackEventArgs e);
		public event PlaybackEventHandler OnFinished;

		public AudioPlayer()
		{
			waveOutDevice = new WaveOut();
		}

		~AudioPlayer()
		{
			if (waveOutDevice != null)
				waveOutDevice.Stop();
			if(audioFileReader != null)
				audioFileReader.Close();
		}

		public void Play(KeyValuePair<string, double> file)
		{
			Play(file.Key, file.Value);
		}

		public void Play(string filename, double position)
		{
			if (current_file == filename && waveOutDevice != null)
			{
				audioFileReader.Position = (long)(position * 1000);
				if (waveOutDevice.PlaybackState != PlaybackState.Playing)
					waveOutDevice.Play();
			}
			else
			{
				current_file = filename;
				audioFileReader = new AudioFileReader(filename);
				audioFileReader.CurrentTime = new TimeSpan(0, 0, 0, 0, (int)(position * 1000));
				if (waveOutDevice == null)
					waveOutDevice = new WaveOut();
				waveOutDevice.Init(audioFileReader);
				waveOutDevice.Play();
				waveOutDevice.PlaybackStopped += waveOutDevice_PlaybackStopped;
			}
		}

		void waveOutDevice_PlaybackStopped(object sender, StoppedEventArgs e)
		{
			if (e.Exception != null)
				Debug.WriteLine(e.Exception.ToString());
			if(OnFinished != null && audioFileReader.Position == audioFileReader.Length)
				OnFinished(this, new PlaybackEventArgs(current_file));
		}

		public void Pause()
		{
			if (waveOutDevice != null)
				waveOutDevice.Pause();
		}

		public void Stop()
		{
			if (waveOutDevice != null)
			{
				waveOutDevice.Stop();
				waveOutDevice = null;
				audioFileReader.Close();
				audioFileReader = null;
			}
		}

		public double Position
		{ get { return ((double)audioFileReader.Position)/1000.0;} }
	}
}
