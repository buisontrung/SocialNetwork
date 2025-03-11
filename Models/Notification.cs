using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models
{
	public class Notification
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string? UserId { get; set; } // Người nhận thông báo
		public bool IsRead { get; set; } = false; // Trạng thái đã đọc
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public int ConversationId { get; set; }     // Chứa ConversationId 
		public Conversation? Conversation { get; set; }
	}
}
