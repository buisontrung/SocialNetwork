using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.ViewModel;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SocialNetwork.hub
{
	public class FriendHub: Hub
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IMemoryCache _cache;
		public FriendHub(ApplicationDbContext dbContext, IMemoryCache cache) {
			_cache = cache;
			_dbContext = dbContext;
		}




		public override async Task OnConnectedAsync()
		{
			var claimIdentity = Context.User?.Identity as ClaimsIdentity;
			var userIdClaim = claimIdentity?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

			if (userIdClaim == null)
			{
				await base.OnConnectedAsync();
				return;
			}

			var userId = userIdClaim.Value;
			_cache.Set(userId, Context.ConnectionId);

			// Lấy danh sách bạn bè
			var friends = await _dbContext.FriendRequests
				.Where(f => (f.SenderId == userId || f.ReceiverId == userId)&&f.Status=="Accepted")
				.Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)

				.ToListAsync();
			var onlineFriends =friends
			.Where(friendId => _cache.TryGetValue(friendId!, out _))
			.Take(14)
			.ToList();
			await Clients.Client(Context.ConnectionId).SendAsync("ReceiveOnlineFriends", onlineFriends);
			var tasks = friends
				.Select(Id => _cache.Get<string>(Id!)) // Lấy connectionId từ cache
				.Where(connectionId => !string.IsNullOrEmpty(connectionId)) // Kiểm tra connectionId hợp lệ
				.Take(10)
				.Select(connectionId => Clients.Client(connectionId!).SendAsync("FriendOnline", userId)) // Gửi thông báo
				.ToList();

			await Task.WhenAll(tasks); // Gửi tất cả thông báo song song

			await base.OnConnectedAsync();
		}
		
		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var claimIdentity = Context.User?.Identity as ClaimsIdentity;
			var userIdClaim = claimIdentity?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

			if (userIdClaim == null)
			{
				await base.OnDisconnectedAsync(exception);
				return;
			}

			var userId = userIdClaim.Value;

			// Xóa khỏi cache
			_cache.Remove(userId);

			// Lấy danh sách bạn bè
			var friends = await _dbContext.FriendRequests
				.Where(f => f.SenderId == userId || f.ReceiverId == userId)
				.Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
				.ToListAsync();

			// Gửi thông báo cho bạn bè rằng user này đã offline
			var tasks = friends
				.Where(friendId => friendId != null && _cache.TryGetValue(friendId, out _))
				 .Select(friendId =>
				 {
					 if (friendId != null && _cache.TryGetValue(friendId, out string? friendConnectionId) && !string.IsNullOrEmpty(friendConnectionId))
					 {
						 return Clients.Client(friendConnectionId).SendAsync("FriendOffline", userId);
					 }
					 return Task.CompletedTask; // Tránh lỗi null
				 })
				.ToList();

			await Task.WhenAll(tasks);

			await base.OnDisconnectedAsync(exception);
		}






	}
}
