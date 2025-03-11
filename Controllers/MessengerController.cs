using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.hub;
using SocialNetwork.Models;
using SocialNetwork.ViewModel;
using System.Linq;

namespace SocialNetwork.Controllers
{

	public class MessengerController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IHubContext<ConversionHub> _hubContext;
		public MessengerController(ApplicationDbContext dbContext, IHubContext<ConversionHub> hubContext) {
			_dbContext = dbContext;
			_hubContext = hubContext;
		}
		[Route("Messenger")]
		public IActionResult Index()
		{
			
	
			return View();
		}
		[HttpGet("Messenger/LoadConversation")]
		public async Task<IActionResult> LoadConversation([FromQuery] int pageIndex, [FromQuery] int pageSize)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			

			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			
			var conversation = await _dbContext
				.ConversationUsers
				.Where(c => c.UserId == userId)
				.Join(_dbContext.Conversations,
					cu => cu.ConversationId,
					c => c.Id,
					(cu, c) => new ConversationViewModel
					{
						Id = c.Id,
						Name = c.Name ?? _dbContext.ConversationUsers.Where(x => x.ConversationId == c.Id && x.UserId != userId).Select(x => x.NickName).FirstOrDefault(),
						NewMessage = _dbContext.Messages
							.Where(m => m.ConversationId == c.Id)
							.OrderByDescending(m => m.CreatedAt)
							.FirstOrDefault()
					})
				.Skip((pageIndex-1)*pageSize)
				.Take(pageSize).ToListAsync();
				foreach(var conver in conversation)
			{
				Console.WriteLine(conver.Name);
			}
			return Ok(conversation);
		}
		[HttpGet]
		[Route("Messenger/{Id}")]
		public async Task<IActionResult> Index(int Id)
		{
			var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			ViewBag.UserName = await _dbContext.Users
					.Where(x => x.Id == userId)
					.Select(x => x.UserName) // Chọn UserName
					.FirstOrDefaultAsync();
			var notification = await _dbContext.NotificationUsers.Where(x => x.ConversationId == Id && x.UserId == userId).FirstOrDefaultAsync();
			if (notification != null) { 
				notification.IsRead = true;
				_dbContext.NotificationUsers.Update(notification);
				await _dbContext.SaveChangesAsync();
			}

			var conversation = await _dbContext
				.Conversations
				.Where(x => x.Id == Id)
				.Include(c => c.ConversationUsers)
				.Include(c => c.Messages)
				.FirstOrDefaultAsync();

			if (conversation == null)
			{
				return NotFound(new { error = "Conversation not found" });
			}
			var user = await _dbContext.ConversationUsers.FirstOrDefaultAsync(x => x.UserId == userId && x.ConversationId == Id);
			if (user == null) {
				return NotFound();
			}
			// Lấy thông tin của hai người trong cuộc trò chuyện
			var usersInConversation = await _dbContext
				.ConversationUsers
				.Where(cu => cu.ConversationId == Id && cu.UserId != userId)
				.Join(_dbContext.Users,
					cu => cu.UserId,
					u => u.Id,
					(cu, u) => new UserViewModel
					{
						NickName = cu.NickName,
						ProfilePictureUrl = u.ProfilePictureUrl,
						UserName =u.UserName,
						FullName = u.FullName,
					})
				.Take(2)
				.ToListAsync();

			// Lấy 10 tin nhắn mới nhất
			var recentMessages = await (
				from m in _dbContext.Messages
				join u in _dbContext.Users on m.SenderId equals u.Id
				join cu in _dbContext.ConversationUsers on new { m.ConversationId, UserId = u.Id } equals new { cu.ConversationId, cu.UserId }
				where m.ConversationId == Id
				orderby m.CreatedAt descending
				select new MessageViewModel
				{
					Id = m.Id,
					ConversationId = m.ConversationId,
					Content = m.Content,
					CreatedAt = m.CreatedAt,
					Sender = new UserViewModel
					{
						UserName = u.UserName,
						FullName = u.FullName,
						NickName = cu.NickName,
						ProfilePictureUrl = u.ProfilePictureUrl
					},
					isMine = u.Id == userId,
				})
				.Take(10)
				.ToListAsync();




			// Tạo ViewModel để truyền vào view
			var viewModel = new ConversationDetailViewModel
			{
				ConversationId = conversation.Id,
				Users = usersInConversation,
				RecentMessages = recentMessages
			};
		
			return View(viewModel);
		}
		[HttpGet("Messenger/GetMessageInConversation")]
		public async Task<IActionResult> GetMessageInConversation(int Id,int pageSize,int pageNumber)
		{
			var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return NotFound(new { error = "User not found" });
			}
			ViewBag.UserName = await _dbContext.Users
					.Where(x => x.Id == userId)
					.Select(x => x.UserName) // Chọn UserName
					.FirstOrDefaultAsync();


			// Lấy 10 tin nhắn mới nhất
			var recentMessages = await (
				from m in _dbContext.Messages
				join u in _dbContext.Users on m.SenderId equals u.Id
				join cu in _dbContext.ConversationUsers on new { m.ConversationId, UserId = u.Id } equals new { cu.ConversationId, cu.UserId }
				where m.ConversationId == Id
				orderby m.CreatedAt descending
				select new MessageViewModel
				{
					Id = m.Id,
					ConversationId = m.ConversationId,
					Content = m.Content,
					CreatedAt = m.CreatedAt,
					Sender = new UserViewModel
					{
						UserName = u.UserName,
						FullName = u.FullName,
						NickName = cu.NickName,
						ProfilePictureUrl = u.ProfilePictureUrl
					},
					isMine = u.Id == userId,
				})
				.Skip((pageNumber-1)*pageSize)
				.Take(pageSize)
				.ToListAsync();






			return Ok(recentMessages);
		}

		[HttpPost("Messenger/AddMessage")]
		public async Task<IActionResult> AddMessage([FromBody]AddMessageRequest request)
		{
			var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return BadRequest();
			}
			
			
			var user = await _dbContext
				.ConversationUsers
				.Where(u => u.UserId == userId && u.ConversationId == request.ConversionId)
				.Join(_dbContext.Users,
					x=>x.UserId,
					u=>u.Id,
					(x, u) => new UserViewModel{ 
						FullName = u.FullName,
						UserName = u.UserName,
						NickName = x.NickName,
						ProfilePictureUrl= u.ProfilePictureUrl
					}
					
				)
				.FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound();
			}
			var newMessage = new Message()
			{
				Content = request.Content,
				ConversationId= request.ConversionId,
				SenderId = userId,
				CreatedAt= DateTime.UtcNow,
			};
			
			await _dbContext.Messages.AddAsync(newMessage);
			await _dbContext.SaveChangesAsync();
			var messagerView = new MessageViewModel()
			{
				ConversationId = request.ConversionId,
				Content = request.Content,
				CreatedAt = DateTime.UtcNow,
				isMine = true,
				Sender = user,
				Id = newMessage.Id,
				SenderId = userId
			};
			await _hubContext.Clients.Group($"conversion-{request.ConversionId}").SendAsync("ReceiveMessenger", messagerView);
			return Ok(messagerView);
		}
		[Route("Messenger/VideoCall")]
		public async Task<IActionResult> VideoCall(int conversionId,bool isSend,string sender,string received)
		{
			var user1 = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == sender);
			if (user1 == null)
			{
				return BadRequest();
			}


			var user = await _dbContext
				.ConversationUsers
				.Where(u => u.UserId == user1.Id && u.ConversationId == conversionId)
				.Join(_dbContext.Users,
					x => x.UserId,
					u => u.Id,
					(x, u) => new UserViewModel
					{
						
						FullName = u.FullName,
						UserName = u.UserName,
						NickName = x.NickName,
						ProfilePictureUrl = u.ProfilePictureUrl
					}

				)
				.FirstOrDefaultAsync();
			if (user == null)
			{
				return NotFound();
			}
			var received1 = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == received);
			if (received1 == null)
			{
				return NotFound();
			}
			var userreceived = await _dbContext
				.ConversationUsers
				.Where(u => u.UserId == received1.Id && u.ConversationId == conversionId)
				.Join(_dbContext.Users,
					x => x.UserId,
					u => u.Id,
					(x, u) => new UserViewModel
					{

						FullName = u.FullName,
						UserName = u.UserName,
						NickName = x.NickName,
						ProfilePictureUrl = u.ProfilePictureUrl
					}

				)
				.FirstOrDefaultAsync();

			if (userreceived == null)
			{
				return NotFound();
			}
			var view = new VideoCallViewModel()
			{
				sender = user,
				receiver = userreceived
			};
			ViewBag.ConversionId = conversionId;
			ViewBag.IsSend = isSend;
			return View(view);
		}
		[HttpPost("Messenger/CheckConversation")]
		public async Task<IActionResult> CheckConversation(string friendName)
		{
			var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var user =await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
			if (user == null)
			{
				return BadRequest("User not found.");
			}
			
			var friend = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == friendName);
			if (friend == null)
			{
				return NotFound("Friend not found.");
			}

			// Tìm cuộc trò chuyện mà chỉ có đúng 2 người tham gia
			var conversation = await _dbContext.Conversations
				.Where(c => c.ConversationUsers.Count == 2 &&
							c.ConversationUsers.Any(cu => cu.UserId == userId) &&
							c.ConversationUsers.Any(cu => cu.UserId == friend.Id))
				.FirstOrDefaultAsync();

			if (conversation != null)
			{
				return Ok(conversation.Id);
			}
			else
			{
				// Nếu chưa có, tạo cuộc trò chuyện mới
				conversation = new Conversation
				{
					Name = null, // Có thể đặt tên tùy ý, hoặc để null nếu là chat 1-1
					CreatedAt = DateTime.UtcNow,
					ConversationUsers = new List<ConversationUser>
					{
						new ConversationUser { UserId = userId ,NickName=user.FullName},
						new ConversationUser { UserId = friend.Id,NickName = friend.FullName }
					}
				};

				_dbContext.Conversations.Add(conversation);
				await _dbContext.SaveChangesAsync();
				return Ok(conversation.Id);
			}
		}
		

	}
}
