using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	public class Conversation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string? Name { get; set; }
		public DateTime CreatedAt { get; set; }
		[JsonIgnore]
		public ICollection<ConversationUser>? ConversationUsers { get; set; }
		[JsonIgnore]
		public ICollection<Message>? Messages { get; set; }
	}
}
