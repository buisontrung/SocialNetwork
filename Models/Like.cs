using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.Models
{
	public class Like
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]	
		public int Id { get; set; }
		public int? PostId { get; set; }
		public string? UserId { get; set; }
		public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

		public Post? Post { get; set; }
		public ApplicationUser? User { get; set; }
	}
}
