using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.ViewModel;

namespace SocialNetwork.Controllers
{
	public class SearchController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		public SearchController(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		[Route("/search/top")]
		public async Task<IActionResult> Index([FromQuery] string q)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var friends = await _dbContext.FriendRequests
				.Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
				.Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
				.ToListAsync();

			var users = await _dbContext.Users
				.AsNoTracking()
				.Where(x => x.FullName != null && x.FullName.Contains(q))
				.Select(x => new ApplicationUser
				{
					Id = x.Id,
					ProfilePictureUrl = x.ProfilePictureUrl,
					UserName = x.UserName,
					FullName = x.FullName,
					Location = x.Location,
					IsFriend = friends.Contains(x.Id)
				})
				.OrderByDescending(x => x.IsFriend)
				.Take(5)
				.ToListAsync();





			foreach (var user in users)
			{
				var userFriends = await _dbContext.FriendRequests
					.Where(f => (f.SenderId == user.Id && f.ReceiverId != userId) && f.Status == "Accepted" || (f.ReceiverId == user.Id && f.SenderId != userId) && f.Status == "Accepted")
					.Select(f => f.SenderId == user.Id ? f.ReceiverId : f.SenderId)
					.ToListAsync();

				user.CommonFriendsCount = userFriends.Intersect(friends).Count();
			}
			var posts = await _dbContext.Posts
					.Where(x => x.Content != null && x.Content.Contains(q))
					.Include(x=>x.User)
					.Select(x=>new Post
					{
						Id= x.Id,
						Content = x.Content,
						ImageUrl = x.ImageUrl,
						LikesCount = x.LikesCount,
						CommentsCount= x.CommentsCount,
						CreatedAt = x.CreatedAt,
						UserId = x.UserId,
						User = x.User !=null ? new ApplicationUser
						{
							Id = x.User.Id,
							ProfilePictureUrl = x.User.ProfilePictureUrl,
							UserName = x.User.UserName,
							FullName = x.User.FullName
						}:null
					})
					.Take(3).OrderByDescending(x=>x.CreatedAt).ToListAsync();
			var viewModel = new SearchViewModel()
			{
				users = users,
				posts = posts
			};
			ViewBag.Query = q;
			ViewBag.Active = 1;
			ViewBag.Load = true;
			return View(viewModel);
		}
		[Route("/search/top/people")]
		public async Task<IActionResult> People([FromQuery] string q)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var friends = await _dbContext.FriendRequests
				.Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
				.Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
				.ToListAsync();

			var users = await _dbContext.Users
				.AsNoTracking()
				.Where(x => x.FullName != null && x.FullName.Contains(q))
				.Select(x => new ApplicationUser
				{
					Id = x.Id,
					ProfilePictureUrl = x.ProfilePictureUrl,
					UserName = x.UserName,
					FullName = x.FullName,
					Location = x.Location,
					IsFriend = friends.Contains(x.Id)
				})
				.OrderByDescending(x=>x.IsFriend)
				.Take(15)
				.ToListAsync();

		



			foreach (var user in users)
			{
				var userFriends = await _dbContext.FriendRequests
					.Where(f => (f.SenderId == user.Id && f.ReceiverId !=userId) && f.Status == "Accepted" || (f.ReceiverId == user.Id && f.SenderId !=userId) && f.Status == "Accepted")
					.Select(f => f.SenderId == user.Id ? f.ReceiverId : f.SenderId)
					.ToListAsync();

				user.CommonFriendsCount = userFriends.Intersect(friends).Count();
			}

			var viewModel = new SearchViewModel()
			{
				users = users,
				posts = null
			};

			ViewBag.Query = q;
			ViewBag.Load = false;
			ViewBag.Active = 3;

			return View("Index", viewModel);
		}
		[Route("/search/top/post")]
		public async Task<IActionResult> Post([FromQuery] string q)
		{
			
			var posts = await _dbContext.Posts
					.Where(x => x.Content != null && x.Content.Contains(q))
					.Include(x => x.User)
					.Select(x => new Post
					{
						Id = x.Id,
						Content = x.Content,
						ImageUrl = x.ImageUrl,
						LikesCount = x.LikesCount,
						CommentsCount = x.CommentsCount,
						CreatedAt = x.CreatedAt,
						UserId = x.UserId,
						User = x.User != null ? new ApplicationUser
						{
							Id = x.User.Id,
							ProfilePictureUrl = x.User.ProfilePictureUrl,
							UserName = x.User.UserName,
							FullName = x.User.FullName
						} : null
					})
					.Take(3).OrderByDescending(x => x.CreatedAt).ToListAsync();
			var viewModel = new SearchViewModel()
			{
				users = null,
				posts = posts
			};
			ViewBag.Query = q;
			ViewBag.Active = 2;
			ViewBag.Load = false;
			return View("Index",viewModel);
		}
		public async Task<IActionResult> Search([FromQuery] string q)
		{
			var user = await _dbContext.Users.Where(x => x.FullName != null && x.FullName.Contains(q)).Take(3).ToListAsync();
			var post = await _dbContext.Posts.Where(x => x.Content != null && x.Content.Contains(q)).Take(3).OrderByDescending(x => x.CreatedAt).ToListAsync();
			return Ok(user);
		}
	}
}
