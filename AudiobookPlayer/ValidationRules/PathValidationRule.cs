using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;

namespace AudiobookPlayer
{
	public class PathValidationRule : ValidationRule
	{
		private string path;
		private string error_message;

		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		public string ErrorMessage
		{
			get { return error_message; }
			set { error_message = value; }
		}

		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			string input = (value ?? string.Empty).ToString();
			if (Directory.Exists(input))
				return new ValidationResult(true, null);
			else
				return new ValidationResult(false, "Path is not a valid directory.");
		}
	}
}
