using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.Service;
using SocialNetwork.ViewModel;

namespace SocialNetwork.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _dbContext;
		private readonly IFileService _fileService;
		public AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext,IFileService fileService)
		{
			_userManager = userManager;
			_dbContext = dbContext;
			_fileService = fileService;
		}
		[Route("Profile/{userName}")]

		public async Task<IActionResult> Profile(string userName)
		{
			
			if (!userName.IsNullOrEmpty())
			{
				var user = await _userManager.FindByNameAsync(userName);

				if (user == null)
				{
					return NotFound(); // Nếu không tìm thấy người dùng
				}
				// Lấy ra 5 bạn bè đầu tiên
				var friends = await _dbContext.FriendRequests
					.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
					.Include(f => f.Receiver)
					.Include(f => f.Sender)
					.Take(6)
					.ToListAsync();
				var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			

				// Lấy tổng số bạn bè
				var totalFriendsCount = await _dbContext.FriendRequests
					.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
					.CountAsync();

				var model = new ProfileViewModel
				{
					User = user,
					Friends = friends.Select(f => new ApplicationUser
					{
						UserName = f.SenderId == user.Id ? f.Receiver?.UserName : f.Sender?.UserName,
						FullName = f.SenderId == user.Id ? f.Receiver?.FullName : f.Sender?.FullName,
						ProfilePictureUrl = f.SenderId == user.Id ? f.Receiver?.ProfilePictureUrl : f.Sender?.ProfilePictureUrl
					}).ToList(),
					TotalFriendsCount = totalFriendsCount, // Truyền tổng số bạn bè
				};
				

				return View(model); // Truyền đối tượng user vào View

			}
			else
			{
				return BadRequest();
			}

		}
		[HttpPost]
		public async Task<IActionResult> UpdateBio(string bio)
		{
			if (User.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(User.Identity.Name))
			{
				return BadRequest(new { error = "Unauthorized access" });
			}

			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null)
			{
				return NotFound(new { error = "User not found" });
			}

			// Kiểm tra bio hợp lệ
			if (string.IsNullOrWhiteSpace(bio) || bio.Length > 250)
			{
				return BadRequest(new { error = "Invalid bio. Maximum length is 250 characters." });
			}

			user.Bio = bio;
			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				return StatusCode(500, new { error = "An error occurred while updating bio." });
			}

			return Ok(new { message = "Cập nhật thành công" });
		}
		[HttpGet]
		public async Task<IActionResult> GetPosts(string userName, int pageIndex = 1, int pageSize = 2)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var posts = await _dbContext.Posts
				.Where(p => p.User!=null &&  p.User.UserName == userName)  // Lọc theo UserName
				.Include(p => p.User) // Load thông tin User
				.OrderByDescending(p => p.CreatedAt)
				.Skip((pageIndex - 1) * pageSize) // Bỏ qua các bài trước đó
				.Take(pageSize) // Lấy đúng số bài cần hiển thị
				.Select(p => new
				{
					Post = new Post
					{
						User = new ApplicationUser
						{
							FullName = p.User !=null ? p.User.FullName :"",
							ProfilePictureUrl = p.User != null ? p.User.ProfilePictureUrl:""
						},
						ImageUrl = p.ImageUrl,
						CreatedAt = p.CreatedAt,
						Content = p.Content,
						LikesCount = p.LikesCount,
						CommentsCount = p.CommentsCount,
						Id = p.Id
					},
					// Thêm trường IsLiked để xác định bài viết đã được like chưa
					IsLiked = _dbContext.Like.Any(l => l.PostId == p.Id && l.UserId == userId)
				})
				.ToListAsync();
			
			return Json(posts);
		}
		[HttpGet]

		public async Task<IActionResult> GetFriends(string userName, int pageIndex , int pageSize )
		{

			var user = await _dbContext.Users
				.Where(u => u.UserName == userName)
				.Select(u => new ApplicationUser { Id = u.Id })
				.FirstOrDefaultAsync();

			if (user == null)
			{
				return NotFound(new { error = "User not found" });
			}
			var friends = _dbContext.FriendRequests
				.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
				.Include(f => f.Sender)
				.Include(f => f.Receiver)
				.Select(f => new
				{
					Id = f.SenderId == user.Id ?   f.Receiver.Id : f.Sender.Id,
					FullName = f.SenderId == user.Id ? f.Receiver.FullName : f.Sender.FullName,
					ProfilePictureUrl = f.SenderId == user.Id ? f.Receiver.ProfilePictureUrl : f.Sender.ProfilePictureUrl
				})
				.OrderBy(f => f.FullName)  // Sắp xếp trên SQL Server
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToList(); // Lấy dữ liệu sau khi xử lý xong trên SQL

			return Ok(friends);
		}

		[HttpGet]
		public async Task<IActionResult> GetPostDetail(int postId)
		{
			try
			{
				Console.WriteLine($"Id nhận được: {postId}");
				var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				var post = await _dbContext.Posts
					.Where(p => p.Id == postId)
					.Select(p => new
					{
						p.Id,
						p.Content,
						p.ImageUrl,
						p.CreatedAt,
						p.LikesCount,
						p.CommentsCount,
						
						User = new
						{
							p.User.FullName,
							p.User.ProfilePictureUrl
						},
						Comments =  p.Comments
							.OrderByDescending(c => c.CreatedAt) // Sắp xếp comment mới nhất trước
							.Take(5) // Chỉ lấy tối đa 5 comment
							.Select(c => new
							{
								c.Id,
								c.PostId,
								c.UserId,
								c.Content,
								c.CreatedAt,
								User = new
								{
									c.User.FullName,
									c.User.ProfilePictureUrl,
									c.User.UserName
								}
							}).ToList()
					})
					.FirstOrDefaultAsync();
				
				if (post == null)
				{
					return NotFound(new { message = "Bài viết không tồn tại" });
				}
				var isLiked = await _dbContext.Like.AnyAsync(l => l.PostId == postId && l.UserId == userId);


				return Ok(new { post, IsLiked = isLiked });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Lỗi server: {ex.Message}");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}


		[HttpPost]
		public async Task<IActionResult> AddLike(int postId)
		{
			// Lấy bài viết từ database
			var post = await _dbContext.Posts.FirstOrDefaultAsync(x => x.Id == postId);
			if (post == null)
			{
				return NotFound("Post not found");
			}
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			// Kiểm tra xem user đã like bài viết chưa
			var userLike = await _dbContext.Like.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);

			if (userLike != null) // Nếu đã like => Bỏ like
			{
				_dbContext.Like.Remove(userLike);
				post.LikesCount -= 1;
			}
			else // Nếu chưa like => Thêm like mới
			{
				var newLike = new Like { UserId = userId, PostId = postId };
				_dbContext.Like.Add(newLike);
				post.LikesCount += 1;
			}

			// Lưu thay đổi vào database
			await _dbContext.SaveChangesAsync();
			return Ok(new { LikesCount = post.LikesCount });
		}
		[HttpPost]
		public async Task<IActionResult> AddFriend(string userName)
		{
			if (User.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(User.Identity.Name))
			{
				return BadRequest(new { error = "Unauthorized access" });
			}
			var user = await _dbContext.Users.FirstOrDefaultAsync(x=>x.UserName ==userName);
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (user == null)
			{
				return NotFound(new { error = "User not found" });
			}
			var friend = await _dbContext.FriendRequests.Where(x => x.SenderId == userId && x.ReceiverId == user.Id).FirstOrDefaultAsync();
			if (friend != null)
			{
				return Ok(new { message = "Lời mời đã được tạo từ trước" });
			}
			var addFriend = new FriendRequest()
			{
				SenderId = userId,
				ReceiverId = user.Id,
				Status = "Pending"
			};

			await _dbContext.FriendRequests.AddAsync(addFriend);
			var saveResult = await _dbContext.SaveChangesAsync();
			if (saveResult <= 0)
			{
				return StatusCode(500, new { error = "Không thể gửi lời mời kết bạn." });
			}

			return Ok(new { message = "Cập nhật thành công" });
		}
		[HttpPost]
		public async Task<IActionResult> AddPost([FromForm] string content, [FromForm] IFormFile? file)
		{
			if (User.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(User.Identity.Name))
			{
				return BadRequest(new { error = "Unauthorized access" });
			}

			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			string? fileUrl = null;
			if (file != null && file.Length > 0)
			{
				fileUrl = await _fileService.SaveFileAsync(file, "img");
			}

			var postModel = new Post()
			{
				UserId = userId,
				Content = content,
				LikesCount = 0,
				CreatedAt = DateTime.UtcNow,
				ImageUrl = fileUrl, // Ảnh có thể null nếu không upload
				CommentsCount = 0,
			};

			// Lưu vào database
			await _dbContext.Posts.AddAsync(postModel);
			await _dbContext.SaveChangesAsync();

			return Ok(new { message = "Tải lên thành công!", postId = postModel.Id, imageUrl = fileUrl });
		}

		[Route("Profile/{userName}/friends")]

		public async Task<IActionResult> Friend(string userName)
		{

			if (!userName.IsNullOrEmpty())
			{
				var user = await _userManager.FindByNameAsync(userName);

				if (user == null)
				{
					return NotFound(); // Nếu không tìm thấy người dùng
				}
				// Lấy ra 5 bạn bè đầu tiên
				var friends = await _dbContext.FriendRequests
					.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
					.Include(f => f.Receiver)
					.Include(f => f.Sender)
					.Take(5)
					.ToListAsync();
				var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				var checkfriend = userId == user.Id || await _dbContext.FriendRequests
						.AnyAsync(x =>
							(x.SenderId == user.Id && x.ReceiverId == userId && x.Status == "Accepted") ||
							(x.SenderId == userId && x.ReceiverId == user.Id && x.Status == "Accepted"));

				// Lấy tổng số bạn bè
				var totalFriendsCount = await _dbContext.FriendRequests
					.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
					.CountAsync();

				var model = new ProfileViewModel
				{
					User = user,
					Friends = friends.Select(f => new ApplicationUser
					{
						UserName = f.SenderId == user.Id ? f.Receiver?.UserName : f.Sender?.UserName,
						FullName = f.SenderId == user.Id ? f.Receiver?.FullName : f.Sender?.FullName,
						ProfilePictureUrl = f.SenderId == user.Id ? f.Receiver?.ProfilePictureUrl : f.Sender?.ProfilePictureUrl
					}).ToList(),
					TotalFriendsCount = totalFriendsCount, // Truyền tổng số bạn bè
					IsFriend = checkfriend
				};


				return View(model); // Truyền đối tượng user vào View

			}
			else
			{
				return BadRequest();
			}

		}
		[HttpGet]
		public async Task<IActionResult> SearchFriends(string query,string userName)
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(x=>x.UserName == userName);
			if(user == null)
			{
				return NotFound();
			}
			var friends = await _dbContext.FriendRequests
			.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
			.Where(f => (f.SenderId == user.Id ? f.Receiver.FullName : f.Sender.FullName).Contains(query)) // Lọc trước
			.Select(f => new
			{
				Id = f.SenderId == user.Id? f.Receiver.Id : f.Sender.Id,
				UserName = f.SenderId == user.Id ? f.Receiver.UserName : f.Sender.UserName,
				FullName = f.SenderId == user.Id ? f.Receiver.FullName : f.Sender.FullName,
				ProfilePictureUrl = f.SenderId == user.Id ? f.Receiver.ProfilePictureUrl : f.Sender.ProfilePictureUrl
			})
			.OrderBy(f => f.FullName)
			.ToListAsync();

			return Ok(friends);
		}
		[HttpGet]
		public async Task<IActionResult> CheckFriend(string userName)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest();
			}
			var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName);
			if (user == null)
			{ 
				return NotFound();
			}
			if (user.Id == userId) {
				return Ok("MyProfile");
			}
			var checkfriend = await _dbContext.FriendRequests
					.FirstOrDefaultAsync(x =>(x.SenderId == userId && x.ReceiverId == user.Id)||(x.ReceiverId == userId && x.SenderId== user.Id));
			if(checkfriend?.ReceiverId == userId && checkfriend.Status == "Pending")
			{
				return Ok(null);
			}
			return Ok(checkfriend?.Status);
		}
	}
}
