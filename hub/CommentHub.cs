using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Models;

namespace SocialNetwork.hub
{
	public class CommentHub:Hub

	{
		public async Task JoinPostGroup(int postId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}");
		}

		// Khi người dùng rời khỏi bài viết
		public async Task LeavePostGroup(int postId)
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}");
		}
		public async Task SendComment(int postId, Comment? comment)
		{
			// Gửi bình luận mới đến tất cả client đang xem bài viết đó
			

			await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", comment);
		
		
		}

	}
}
