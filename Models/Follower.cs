using SocialNetwork.Models;

namespace SocialNetwork.Models
{
	public class Follower
	{
		public string? FollowerId { get; set; }
		public string? FollowingId { get; set; }
		public DateTime? CreatedAt { get; set; }

		public ApplicationUser? FollowerUser { get; set; }
		public ApplicationUser? FollowingUser { get; set; }
	}
}
