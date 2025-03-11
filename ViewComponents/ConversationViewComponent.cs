using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.ViewModel;
using System.Security.Claims;

namespace SocialNetwork.ViewComponents
{
	public class ConversationViewComponent: ViewComponent
	{
		private readonly ApplicationDbContext _dbContext;
		public ConversationViewComponent(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var user = (ClaimsPrincipal)User;
			var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

			if (userIdClaim == null)
			{
				return Content("User not found");
			}

			var userId = userIdClaim.Value;

			var conversation = await _dbContext
				.ConversationUsers
				.Where(c => c.UserId == userId)
				.Join(_dbContext.Conversations,
					cu => cu.ConversationId,
					c => c.Id,
					(cu, c) => new ConversationViewModel
					{
						Id = c.Id,
						Name = c.Name ?? _dbContext.ConversationUsers
							.Where(x => x.ConversationId == c.Id && x.UserId != userId)
							.Select(x => x.NickName)
							.FirstOrDefault(),
						NewMessage = _dbContext.Messages
							.Where(m => m.ConversationId == c.Id)
							.OrderByDescending(m => m.CreatedAt)
							.FirstOrDefault(),
						IsRead = !_dbContext.NotificationUsers
							.Any(x => x.ConversationId == cu.ConversationId && x.UserId == userId && !x.IsRead),
						

					})
				.OrderByDescending(x => x.NewMessage.CreatedAt)
				.Take(7)
				.ToListAsync();

			return View(conversation);
		}
	}
}
