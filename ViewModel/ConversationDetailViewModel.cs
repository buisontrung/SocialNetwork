using SocialNetwork.Models;

namespace SocialNetwork.ViewModel
{
	public class ConversationDetailViewModel
	{
		public int ConversationId { get; set; }
		public IEnumerable<UserViewModel>? Users { get; set; }
		public IEnumerable<MessageViewModel>? RecentMessages { get; set; }
	}

	public class UserViewModel
	{
		public string? UserName { get; set; }
		public string? NickName { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public string? FullName { get; set; }
	}
	public class MessageViewModel
	{
		public int Id { get; set; }
		public string? SenderId { get; set; }
		public int ConversationId { get; set; }
		public string? Content { get; set; }
		public DateTime CreatedAt { get; set; }
	
		public UserViewModel? Sender { get; set; }
		public bool isMine { get; set; }

	}
	public class AddMessageRequest
	{
		public int ConversionId { get; set; }
		public string? Content { get; set; }
	}
}
