using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.Models
{
	public class UserSettings
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string? UserId { get; set; }
		public bool NotificationEnabled { get; set; }
		public string? PrivacySetting { get; set; }  // 'Public', 'Private', 'FriendsOnly'

		public ApplicationUser? User { get; set; }
	}
}
