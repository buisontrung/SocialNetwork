using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.hub;
using SocialNetwork.Models;

namespace SocialNetwork.Controllers
{
	public class CommentController : Controller
	{
		private readonly ApplicationDbContext _dbContext;

		private readonly ILogger<CommentController> _logger;
		private readonly IHubContext<CommentHub> _hubContext;
		public CommentController(ApplicationDbContext dbContext, ILogger<CommentController> logger, IHubContext<CommentHub> hubContext) {
			_dbContext = dbContext;
			_logger = logger;
			_hubContext = hubContext;


		}
		[HttpPost]
		public async Task<IActionResult> AddComment(int postId, string content)
		{
			// Kiểm tra bài viết có tồn tại không


			// Lấy userId
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "Người dùng chưa đăng nhập" });
			}
			if (string.IsNullOrWhiteSpace(content))
			{
				_logger.LogWarning("Nội dung bình luận trống.");
				return BadRequest(new { message = "Nội dung bình luận không được để trống." });
			}
			var user = await  _dbContext.AspNetUsers.FirstOrDefaultAsync(x => x.Id == userId);
			// Tạo comment mới
			var comment = new Comment()
			{
				PostId = postId,
				Content = content,
				UserId = userId,
				User = user,
				CreatedAt = DateTime.UtcNow,
			};
	
			
			await _dbContext.Comments.AddAsync(comment);

		
			await _dbContext.Database.ExecuteSqlRawAsync("UPDATE Posts SET CommentsCount = CommentsCount + 1 WHERE Id = {0}", postId);
			_dbContext.SaveChanges();
			
			// Gửi bình luận mới đến tất cả client trong nhóm bài viết
			await _hubContext.Clients.Group($"post-{postId}").SendAsync("ReceiveComment", comment);
			// Trả về comment vừa tạo
			return Ok(comment);
		}
		[HttpGet]
		public async Task<IActionResult> RenderComment(int postId, int pageIndex, int pageSize)
		{

			var comments = await _dbContext.Comments
								.Where(x=>x.PostId == postId)
								.Include(x=>x.User)
								.Select(x => new
								{
									x.Id,
									x.UserId,
									x.Content,
									x.CreatedAt,
									x.PostId,
									User = new
									{
										x.User.UserName,
										x.User.ProfilePictureUrl,	
										x.User.FullName,
									},
								})
								.OrderByDescending(x=>x.CreatedAt)
								.Skip((pageIndex - 1) * pageSize)
								.Take(pageSize)
								.ToListAsync();


			return Ok(comments);
		}

	}
}
