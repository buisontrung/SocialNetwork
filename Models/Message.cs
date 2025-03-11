using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	public class Message
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string? SenderId { get; set; }
		public int ConversationId { get; set; }
		public string? Content { get; set; }
		public DateTime CreatedAt { get; set; }
		[JsonIgnore]
		public ApplicationUser? Sender { get; set; }
		[JsonIgnore]
		public Conversation? Conversation { get; set; }
	}
}
