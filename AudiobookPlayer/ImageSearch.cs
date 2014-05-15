using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using Google.API.Search;

namespace AudiobookPlayer
{
	public delegate void SearchEventHandler(object source, ImageSearchEventArgs e);

	public class ImageSearch
	{
		public event SearchEventHandler OnFinished;

		string search_term;
		int no_of_results;
		int no_of_results_to_order;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="search_term">Phrase that is searched.</param>
		/// <param name="no_of_results">The number of results given as a result.</param>
		/// <param name="no_of_results_to_order">The size of the pool that is searched for the results with the highest resolution.</param>
		public ImageSearch(string search_term, int no_of_results, int no_of_results_to_order)
		{
			this.search_term = search_term;
			this.no_of_results = no_of_results;
			this.no_of_results_to_order = no_of_results_to_order;
		}

		public ImageSearch(string search_term, int no_of_results)
		{
			this.search_term = search_term;
			this.no_of_results = no_of_results;
			this.no_of_results_to_order = 2 * no_of_results;
		}

		// Fetches double the number of wanted results and selects the ones with the highest resolution.
		public void Start(Object thread_context)
		{
			var image_results = QueryForImages(search_term, no_of_results_to_order);
			var ordered_images = OrderImagesBySize(image_results);
			ordered_images = ordered_images.Take(no_of_results);
			var images = ImageResultsToImages(ordered_images);
			if (OnFinished != null)
				OnFinished(this, new ImageSearchEventArgs(images));
		}

		private IList<IImageResult> QueryForImages(string search_term, int no_of_results)
		{
			var image_client = new GimageSearchClient("http://mysite.com");
			var results = image_client.Search(search_term + " book", no_of_results);
			return results;
		}

		private IEnumerable<IImageResult> OrderImagesBySize(IList<IImageResult> images)
		{
			IEnumerable<IImageResult> ordered_images = images.OrderBy((i1, i2) => i1.Area().CompareTo(i1.Area()));
			return ordered_images;
		}

		private List<Image> ImageResultsToImages(IEnumerable<IImageResult> image_results)
		{
			List<Image> images = new List<Image>(image_results.Count());
			foreach(IImageResult image_result in image_results)
			{
				Image image = DownloadImage(image_result.Url);
				images.Add(image);
			}
			return images;
		}

		private Image DownloadImage(string url)
		{
			WebRequest request = WebRequest.Create(url);
			WebResponse response = request.GetResponse();
			Image image = Image.FromStream(response.GetResponseStream());
			return image;
		}
	}

	public class ImageSearchEventArgs : EventArgs
	{
		List<Image> results;

		public ImageSearchEventArgs(List<Image> results)
		{
			this.results = results;
		}

		public List<Image> Results
		{ get { return results; } }
	}
}
