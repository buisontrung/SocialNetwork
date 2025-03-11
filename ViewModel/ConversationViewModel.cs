

using SocialNetwork.Models;

namespace SocialNetwork.ViewModel
{
	public class ConversationViewModel
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public bool? IsRead { get; set; }
		public Message? NewMessage { get; set; }
	}
}
