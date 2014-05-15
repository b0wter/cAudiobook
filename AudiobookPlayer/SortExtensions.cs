using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookPlayer
{
	// using this in method signature to declare an extension method, see:
	// http://msdn.microsoft.com/en-us/library/bb383977.aspx
	//
	public static class SortExtensions
	{
		public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			ArrayList.Adapter((IList)list).Sort(new ComparisonComparer<T>(comparison));
		}

		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Comparison<T> comparison)
		{
			return list.OrderBy(t => t, new ComparisonComparer<T>(comparison));
		}
	}

	public class ComparisonComparer<T> : IComparer<T>, IComparer
	{
		private readonly Comparison<T> comparison;

		public ComparisonComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public int Compare(T x, T y)
		{
			return comparison(x, y);
		}

		public int Compare(object o1, object o2)
		{
			return comparison((T)o1, (T)o2);
		}
	}

	/// <summary>
	/// Extension for the IImageResult to offer better ordering of sizes.
	/// </summary>
	public static class IImageResultExtension
	{
		public static int Area(this Google.API.Search.IImageResult image_result)
		{
			return image_result.Height * image_result.Width;
		}
	}
}
