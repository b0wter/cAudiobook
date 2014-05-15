using System;
using System.Globalization;
using System.Windows.Controls;

namespace AudiobookPlayer
{
	public class ThreadRangeRule : ValidationRule
	{
		private int min;

		public ThreadRangeRule()
		{
			min = 0;
		}

		public int Min
		{
			get { return min; }
			set { min = value; }
		}

		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			int threads = 0;
			try
			{
				if (((string)value).Length > 0)
					threads = int.Parse((string)value);
			}
			catch (FormatException)
			{
				return new ValidationResult(false, "invalid formatting");
			}
			if (threads < 1)
				return new ValidationResult(false, "cannot be smaller than 1");
			else
				return new ValidationResult(true, null);
		}
	}
}
