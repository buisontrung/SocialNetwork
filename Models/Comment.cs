using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	public class Comment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int? PostId { get; set; }
		public string? UserId { get; set; }
		public string? Content { get; set; }
		public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
		[JsonIgnore]
		public Post? Post { get; set; }
		public ApplicationUser? User { get; set; }
	}
}
