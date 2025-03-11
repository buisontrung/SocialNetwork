using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.Models;

namespace SocialNetwork.Controllers
{
	[Authorize]
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        public HomeController(ILogger<HomeController> logger,ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

		public async Task<IActionResult> Index()
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			var user = await _dbContext.Users.FirstOrDefaultAsync(x =>x.Id == userId);
			ViewBag.fullName = user?.FullName;
			ViewBag.imageUrl = user?.ProfilePictureUrl;
			// Lấy danh sách bạn bè của user
			var friendIds = await _dbContext.FriendRequests
									.Where(f => (f.SenderId == userId || f.ReceiverId == userId)&& f.Status=="Accepted")
									.Select(p=>p.SenderId == userId?p.ReceiverId:p.SenderId)
									.ToListAsync();

			// Lấy tất cả bài viết của bạn bè
			var posts = await _dbContext.Posts
								.Where(p => friendIds.Contains(p.UserId))
								.Join(_dbContext.Users,
								p=>p.UserId,
								u=>u.Id,
								(p, u) =>new Post
								{	Id=p.Id,
									Content =p.Content,
									CreatedAt = p.CreatedAt,
									ImageUrl = p.ImageUrl,
									LikesCount = p.LikesCount,
									CommentsCount = p.CommentsCount,
									User = new ApplicationUser()
									{
										UserName = u.UserName,
										FullName = u.FullName,
										ProfilePictureUrl = u.ProfilePictureUrl,
									},
									IsLiked = _dbContext.Like.Any(l => l.PostId == p.Id && l.UserId == userId),
									
								})
								.OrderByDescending(p => p.CreatedAt)
								.Take(4)
								.ToListAsync();
			
			return View(posts); // Trả danh sách bài viết cho View
		}
		[HttpGet]
		public async Task<IActionResult> GetFriendPosts(int pageIndex,int pageSize)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			
			// Lấy danh sách bạn bè của user
			var friendIds = await _dbContext.FriendRequests
									.Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
									.Select(p => p.SenderId == userId ? p.ReceiverId : p.SenderId)
									.ToListAsync();

			// Lấy tất cả bài viết của bạn bè
			var posts = await _dbContext.Posts
								.Where(p => friendIds.Contains(p.UserId))
								.Join(_dbContext.Users,
								p => p.UserId,
								u => u.Id,
								(p, u) => new Post
								{
									Id = p.Id,
									Content = p.Content,
									CreatedAt = p.CreatedAt,
									ImageUrl = p.ImageUrl,
									LikesCount = p.LikesCount,
									CommentsCount = p.CommentsCount,
									User = new ApplicationUser()
									{
										UserName = u.UserName,
										FullName = u.FullName,
										ProfilePictureUrl = u.ProfilePictureUrl,
									},
									IsLiked = _dbContext.Like.Any(l => l.PostId == p.Id && l.UserId == userId)
								})
								.OrderByDescending(p => p.CreatedAt)
								.Skip((pageIndex-1)*pageSize)
								.Take(pageSize)
								.ToListAsync();

			return Ok(posts); // Trả danh sách bài viết cho View
		}

		public IActionResult Privacy()
        {
			return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
