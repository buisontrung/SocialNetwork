using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Models;
using SocialNetwork.ViewModel;
using System.Text.RegularExpressions;

namespace SocialNetwork.hub
{
	public class ConversionHub: Hub
	{
		public async Task JoinConversionGroup(int Id)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, $"conversion-{Id}");
		}

		// Khi người dùng rời khỏi bài viết
		public async Task LeaveConversionGroup(int Id)
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversion-{Id}");
		}
		public async Task SendMessenger(int Id, MessageViewModel? message)
		{
			// Gửi bình luận mới đến tất cả client đang xem bài viết đó


			await Clients.Group($"conversion-{Id}").SendAsync("ReceiveMessenger", message);


		}
		public async Task SendVideoCall(int Id,string message)
		{
			// Gửi lời mời call video đến tất cả các client khác trong nhóm, trừ người gửi
			await Clients.OthersInGroup($"conversion-{Id}").SendAsync("ReceiveCallVideo", message);
		}
		public async Task SendInvitation(int Id, string message)
		{
			// Gửi lời mời call video đến tất cả các client khác trong nhóm, trừ người gửi
			await Clients.OthersInGroup($"conversion-{Id}").SendAsync("InvitationCallVideo", message);
		}
	}
}
