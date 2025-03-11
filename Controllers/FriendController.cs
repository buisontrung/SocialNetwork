using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SocialNetwork.Data;
using SocialNetwork.Models;

namespace SocialNetwork.Controllers
{
	[Authorize]
	public class FriendController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IHttpClientFactory _httpClientFactory;
		public FriendController(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
			_dbContext = dbContext;
		}
		public IActionResult Index()
		{
			return View();
		}
		[HttpGet]
		public async Task<IActionResult> GetFriendsPending( int pageIndex, int pageSize)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			var friends = await _dbContext.FriendRequests
			.Where(f => f.ReceiverId == userId && f.Status == "Pending")
			.Join(_dbContext.Users,
				  f => f.SenderId,
				  u => u.Id,
				  (f, u) => new
				  {
					  Id = u.Id,
					  FullName = u.FullName,
					  ProfilePictureUrl = u.ProfilePictureUrl,
					  UserName= u.UserName,
					  FriendRequestId =f.Id,
					  CommonFriends = _dbContext.FriendRequests
						  .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.Status == "Accepted" &&
									   _dbContext.FriendRequests
										   .Any(fr2 => ( fr2.Status == "Accepted" && fr2.SenderId == u.Id && fr2.ReceiverId == (fr.SenderId == userId ? fr.ReceiverId : fr.SenderId)) || 
													  (fr2.Status == "Accepted" && fr2.ReceiverId == u.Id && fr2.SenderId == (fr.SenderId == userId ? fr.ReceiverId : fr.SenderId))))
						  .Select(fr => fr.SenderId == userId ? fr.Receiver : fr.Sender)
						  .Take(2) // Lấy ra hai người bạn chung cụ thể
						  .ToList(),
					  CommonFriendsCount = _dbContext.FriendRequests
						  .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.Status == "Accepted" &&
									   _dbContext.FriendRequests
										   .Any(fr2 => (fr2.Status == "Accepted" && fr2.SenderId == u.Id && fr2.ReceiverId == (fr.SenderId == userId ? fr.ReceiverId : fr.SenderId)) ||
													   (fr2.Status == "Accepted" && fr2.ReceiverId == u.Id && fr2.SenderId == (fr.SenderId == userId ? fr.ReceiverId : fr.SenderId))))
						  .Count()
				  })
			.OrderBy(f => f.FullName)  // Sắp xếp trên SQL Server
			.Skip((pageIndex - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();
			return Ok(friends);
		}
		[HttpGet]
		public async Task<IActionResult> GetFriendsAccepted(int pageIndex, int pageSize)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			var friends = await _dbContext.FriendRequests
			.Where(f => (f.ReceiverId == userId || f.SenderId == userId) && f.Status == "Accepted")
			.Join(_dbContext.Users,
				  f => f.SenderId,
				  u => u.Id,
				  (f, u) => new ApplicationUser
				  {
					  Id = u.Id,
					  FullName = u.FullName,
					  ProfilePictureUrl = u.ProfilePictureUrl,
					  UserName = u.UserName,
					  
				  })
			.OrderBy(f => f.FullName)  // Sắp xếp trên SQL Server
			.Skip((pageIndex - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();
			return Ok(friends);
		}
		[HttpGet]
		public async Task<IActionResult> GetFriend(string friendId)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			var friend =await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == friendId);
			return Ok(friend);
		}
		[HttpPost]
		public async Task<IActionResult> AcceptFriend(int Id)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest(new { error = "User not found" });
			}

			

			var friendRequest = await _dbContext.FriendRequests
				.FirstOrDefaultAsync(x =>  x.ReceiverId == userId && x.Id == Id);

			if (friendRequest == null)
			{
				return BadRequest(new { error = "Friend request not found" });
			}

			friendRequest.Status = "Accepted";
			_dbContext.FriendRequests.Update(friendRequest);
			await _dbContext.SaveChangesAsync();

			return Ok(friendRequest);
		}
		[HttpDelete]
		public async Task<IActionResult> RemoveFriend(int Id)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest(new { error = "User not found" });
			}



			var friendRequest = await _dbContext.FriendRequests
				.FirstOrDefaultAsync(x => x.ReceiverId == userId && x.Id == Id);

			if (friendRequest == null)
			{
				return BadRequest(new { error = "Friend request not found" });
			}

			_dbContext.FriendRequests.Remove(friendRequest);
			await _dbContext.SaveChangesAsync();

			return Ok(friendRequest);
		}
		public async Task<IActionResult> GetFriends([FromQuery] List<string> userIds)
		{
			if (userIds == null || userIds.Count == 0)
				return BadRequest("Danh sách userId không hợp lệ");

			if (userIds.Count > 14)
				return BadRequest("Chỉ có thể lấy tối đa 14 user mỗi request");

			var friends = await _dbContext.Users
				.Where(u => userIds.Contains(u.Id))
				.Select(u => new
				{
					UserId = u.Id,
					FullName = u.FullName,
					ProfilePictureUrl = string.IsNullOrEmpty(u.ProfilePictureUrl) ? "/img/default-avatar.png" : u.ProfilePictureUrl
				})
				.ToListAsync();

			return Ok(friends);
		}
		[HttpGet]
		public async Task<IActionResult> GetRecommendFriends()
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest(new { error = "User not found" });
			}

			var httpClient = _httpClientFactory.CreateClient();

			// Gọi đến API để lấy danh sách bạn bè
			var response = await httpClient.GetAsync($"http://127.0.0.1:8000/recommendFriend/{userId}");

			if (response.IsSuccessStatusCode)
			{
				var friendsJson = await response.Content.ReadAsStringAsync();
				var friendIds = JsonConvert.DeserializeObject<List<string>>(friendsJson);
				if (friendIds == null || friendIds.Count == 0)
				{
					return Ok();
				}

				// Lọc những người bạn có trong friendIds
				var friends = await _dbContext.Users
					.Where(u => friendIds.Contains(u.Id))  // Sử dụng Contains() để kiểm tra ID trong friendIds
					.Select(x=>new ApplicationUser
					{
						Id = x.Id,
						FullName =x.FullName,
						ProfilePictureUrl = x.ProfilePictureUrl,
						UserName = x.UserName
					})
					.ToListAsync();


				return Ok(friends);
			}

			return BadRequest(new { error = "Failed to fetch recommended friends" });
		}


	}
}
