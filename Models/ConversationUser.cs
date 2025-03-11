using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	public class ConversationUser
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; } // Cột Id duy nhất
		public string? UserId { get; set; }
		public int ConversationId { get; set; }
		public string? NickName { get; set; }
		[JsonIgnore]
		public ApplicationUser? User { get; set; }
		[JsonIgnore]
		public Conversation? Conversation { get; set; }
	}
}
