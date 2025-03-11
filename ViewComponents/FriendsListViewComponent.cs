using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.Models;

namespace SocialNetwork.ViewComponents
{
	public class FriendsListViewComponent: ViewComponent
	{
		private readonly ApplicationDbContext _dbContext;

		public FriendsListViewComponent(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		public async Task<IViewComponentResult> InvokeAsync(string userName, int pageIndex = 1, int pageSize = 10)
		{
			var user = await _dbContext.Users
				.Where(u => u.UserName == userName)
				.Select(u => new ApplicationUser { Id = u.Id })
				.FirstOrDefaultAsync();

			if (user == null)
			{
				return Content("User not found");
			}

			var friends = _dbContext.FriendRequests
				.Where(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == "Accepted")
				.Include(f => f.Sender)
				.Include(f => f.Receiver)
				.AsEnumerable()
				.Select(f => new ApplicationUser
				{
					Id = f.SenderId == user.Id ? f.Receiver?.Id : f.Sender?.Id,
					FullName = f.SenderId == user.Id ? f.Receiver?.FullName : f.Sender?.FullName,
					ProfilePictureUrl = f.SenderId == user.Id ? f.Receiver?.ProfilePictureUrl : f.Sender?.ProfilePictureUrl
				})
				.OrderBy(f => f.FullName)
				.ToList();

			return View(friends);
		}
	}
}
