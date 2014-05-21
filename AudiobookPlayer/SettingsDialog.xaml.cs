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

namespace AudiobookPlayer
{
	/// <summary>
	/// Interaction logic for SettingsDialog.xaml
	/// </summary>
	public partial class SettingsDialog : Window
	{
		public static readonly DependencyProperty BackgroundThreadsProperty = DependencyProperty.Register("BackgroundThreads", typeof(int), typeof(SettingsDialog), new UIPropertyMetadata(2));
		Config config;

		public SettingsDialog(Config current_config)
		{
			InitializeComponent();
			config = current_config;
			this.DataContext = config;
			SetupControls();
		}

		private void SetupControls()
		{
			//txtAudiobookPath.Text = config.AudiobookPath;
			//txtBackgroundThreads.Text = config.NoOfThreads.ToString();
			//txtLargeSkipSeconds.Text = config.LargeSkipSeconds.ToString();
			//txtSmallSkipSeconds.Text = config.SmallSkipSeconds.ToString();
			//txtUpdateIntervallSeconds.Text = config.AudiobookUpdateIntervall.ToString();
		}

		private bool ValidateTextFields()
		{
			bool has_errors = false;
			has_errors = has_errors && Validation.GetHasError(txtAudiobookPath);
			has_errors = has_errors && Validation.GetHasError(txtSmallSkipSeconds);
			has_errors = has_errors && Validation.GetHasError(txtLargeSkipSeconds);
			has_errors = has_errors && Validation.GetHasError(txtUpdateIntervallSeconds);
			has_errors = has_errors && Validation.GetHasError(txtBackgroundThreads);
			return has_errors;
		}

		private void cmdOk_Click(object sender, RoutedEventArgs e)
		{
			if (ValidateTextFields())
			{
				MessageBox.Show("At least one text fields contains errors.");
				return;
			}
			else
				DialogResult = true;
		}
	}


}
