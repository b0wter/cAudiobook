using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudiobookPlayer
{
	public class TimeSpanValidationRule : ValidationRule
	{
		TimeSpan maximum;
		string error_message;

		public TimeSpan Maximum
		{
			get { return maximum; }
			set { maximum = value; }
		}

		public string MaximumAsString
		{
			get { return maximum.ToString(); }
			set
			{
				TimeSpan timespan = TimeSpan.Parse(value);
				maximum = timespan;
			}
		}

		public string ErrorMessage
		{
			get { return error_message; }
			set { error_message = value; }
		}

		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			string input = (value ?? string.Empty).ToString();
			TimeSpan timespan = TimeSpan.Zero;
			if (TimeSpan.TryParse(input, out timespan))
				return new ValidationResult(true, null);
			else
				return new ValidationResult(false, "Could not convert input to TimeSpan.");
		}

	}
}
