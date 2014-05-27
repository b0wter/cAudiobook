using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudiobookPlayer
{
	public class IntRangeValidationRule : ValidationRule
	{
		private int minimum = 1;
		private int maximum = int.MaxValue;
		private string error_message;

		public int Minimum
		{
			get { return minimum; }
			set { minimum = value; }
		}

		public string MinimumAsString
		{
			get { return minimum.ToString(); }
			set
			{
				if (!int.TryParse(value, out minimum))
					minimum = 1;
			}
		}

		public int Maxmimum
		{
			get { return maximum; }
			set { maximum = value; }
		}

		public string MaximumAsString
		{
			get { return maximum.ToString(); }
			set
			{
				if (!int.TryParse(value, out maximum))
					maximum = int.MaxValue;
			}
		}

		public string ErrorMessage
		{
			get { return error_message; }
			set { error_message = value; }
		}

		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			ValidationResult result = new ValidationResult(true, null);
			string input = (value ?? string.Empty).ToString();
			int converted_input;

			if (!int.TryParse(input, out converted_input))
				return new ValidationResult(false, "Cannot convert text to integer. User numbers only.");

			if (converted_input < Minimum || converted_input > Maxmimum)
				return new ValidationResult(false, "Number needs to be between " + Minimum.ToString() + " and " + Maxmimum.ToString() + ".");

			return result;
		}
	}
}
