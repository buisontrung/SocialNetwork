using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models
{
	public class FriendRequest
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string? SenderId { get; set; }
		public string? ReceiverId { get; set; }
		public string? Status { get; set; }  // 'Pending', 'Accepted', 'Rejected'
		public DateTime? CreatedAt { get; set; }
		[JsonIgnore]
		public ApplicationUser? Sender { get; set; }
		[JsonIgnore]
		public ApplicationUser? Receiver { get; set; }
	}
}
