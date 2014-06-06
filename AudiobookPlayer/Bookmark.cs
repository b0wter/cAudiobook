using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AudiobookPlayer
{
	[Serializable]
	public class Bookmark
	{
		string name;
		double position;

		public Bookmark(double position)
		{
			this.position = position;
			this.name = "unnamed";
		}

		public Bookmark(string name, double position)
		{
			this.name = name;
			this.position = position;
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public double Position
		{
			get { return position; }
			set { position = value; }
		}

		public string Description
		{
			get { return this.ToString(); }
		}

		public override string ToString()
		{
			return Name + " (" + DoubleToTimespan(Position).ToString() + ")";
		}

		private TimeSpan DoubleToTimespan(double position)
		{
			return new TimeSpan(0, 0, (int)position);
		}
	}
}
