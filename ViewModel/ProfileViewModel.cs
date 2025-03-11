using SocialNetwork.Models;

namespace SocialNetwork.ViewModel
{
	public class ProfileViewModel
	{
		public ApplicationUser? User { get; set; }
		public List<ApplicationUser>? Friends { get; set; }
		public int TotalFriendsCount { get; set; }
		public bool? IsFriend { get; set; }
	}
}
