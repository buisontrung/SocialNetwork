using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	public class Post
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string? UserId { get; set; }
		public string? Content { get; set; }
		public string? ImageUrl { get; set; }
		public DateTime? CreatedAt { get; set; }
		public int LikesCount { get; set; }
		public int CommentsCount { get; set; }
		[NotMapped]
		public bool IsLiked { get; set; }
		public ApplicationUser? User { get; set; } 
		[JsonIgnore]
		public virtual ICollection<Comment>? Comments { get; set; } 
		public ICollection<Like>? Likes { get; set; }
	}
}
