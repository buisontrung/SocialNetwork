using SocialNetwork.Models;

namespace SocialNetwork.ViewModel
{
	public class SearchViewModel
	{
		public IEnumerable<ApplicationUser>? users { get; set; }
		public IEnumerable<Post>? posts { get; set; }
	}
}
