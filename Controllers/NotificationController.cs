using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.ViewModel;
using System.Text.Json;

namespace SocialNetwork.Controllers
{
	public class NotificationController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		public NotificationController(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		[HttpGet]
		public async Task<IActionResult> GetNotificationByUserId(int id)
		{
			var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return NotFound();
			}
			var notification = await _dbContext.NotificationUsers
											.Where(x => x.UserId == userId && x.IsRead ==false)
											.Select(x => new {x.ConversationId })
											.ToListAsync();

			return Ok(notification);
		}

		[HttpPost]
		public async Task<IActionResult> AddNotification([FromBody] int conversationId)
		{
			var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest("User not found");
			}

			// Gộp truy vấn: Tìm người nhận & kiểm tra notification cùng lúc
			var conversationUserWithNotification = await _dbContext.ConversationUsers
				.Where(x => x.ConversationId == conversationId && x.UserId != userId)
				.Select(x => new
				{
					x.UserId,
					Notification = _dbContext.NotificationUsers
						.Select(x => new Notification{
							Id= x.Id,
							ConversationId = x.ConversationId,
							CreatedAt = x.CreatedAt,
							IsRead = false,
							UserId = x.UserId,
						})
						.FirstOrDefault(n => n.UserId == x.UserId && !n.IsRead && n.ConversationId==conversationId)
				})
				.FirstOrDefaultAsync();

			// Nếu đã có thông báo, trả về 
			if (conversationUserWithNotification?.Notification != null)
			{
				_dbContext.Update(conversationUserWithNotification.Notification);
				await _dbContext.SaveChangesAsync();
				return Ok(conversationUserWithNotification?.Notification);
			}

			// Tạo mới thông báo nếu chưa có
			var notification = new Models.Notification
			{
				UserId = conversationUserWithNotification?.UserId,
				IsRead = false,
				ConversationId = conversationId,
				CreatedAt = DateTime.UtcNow
			};

			_dbContext.NotificationUsers.Add(notification);
			await _dbContext.SaveChangesAsync();

			return Ok(notification);
		}



	}
}
