using Microsoft.AspNetCore.Identity;
using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	[Table("AspNetUsers")]
	public class ApplicationUser : IdentityUser
	{
		public string? FullName { get; set; }
		public string? Bio { get; set; }
		public string? ProfilePictureUrl { get; set; } = "default-avatar.png";
		public string? Location { get; set; }
		[NotMapped]
		public int? CommonFriendsCount { get; set; }
		[NotMapped]
		public bool? IsFriend { get; set; }
		public ICollection<Post>? Posts { get; set; }
		[JsonIgnore]
		public ICollection<Comment>? Comments { get; set; }
		public ICollection<Like>? Likes { get; set; }
		public ICollection<Message>? SentMessages { get; set; } // Tin nhắn đã gửi
		public ICollection<Follower>? Followers { get; set; }
		public ICollection<Follower>? Following { get; set; }
		[JsonIgnore]
		public ICollection<ConversationUser>? ConversationUsers { get; set; }
	}
}
