using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models;
using SocialNetwork.ViewModel;
using System.Security.Claims;

namespace SocialNetwork.Controllers
{
	public class AuthController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		// Action hiển thị form đăng ký
		[HttpGet]
		public IActionResult Register()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ nếu đã đăng nhập
			}
			return View();
		}

		// Xử lý đăng ký
		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.Username,
					Email = model.Email,
					FullName = model.FullName
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction("Index", "Home");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return View(model);
		}

		// Action hiển thị form đăng nhập
		[HttpGet]
		public IActionResult Login()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ nếu đã đăng nhập
			}
			return View();
		}

		// Xử lý đăng nhập
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

				if (result.Succeeded)
				{
			
						return RedirectToAction("Index", "Home");
				}

				ModelState.AddModelError(string.Empty, "Đăng nhập không hợp lệ.");
			}
			return View(model);
		}

		// Đăng xuất
		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login", "Auth");
		}
		[HttpGet]
		public async Task<IActionResult> GetImage(string userName)
		{
			// Tìm người dùng theo tên
			if (userName == null)
			{
				return Ok();
			}
			var user = await _userManager.FindByNameAsync(userName);

			// Kiểm tra nếu người dùng không tồn tại
			if (user == null)
			{
				return NotFound("User not found"); // Trả về thông báo lỗi nếu không tìm thấy người dùng
			}

			// Lấy đường dẫn đến hình ảnh từ trường ProfilePictureUrl
			var imagePath = "/img/"+ user.ProfilePictureUrl;

			// Kiểm tra nếu không có hình ảnh (trường ProfilePictureUrl có thể null hoặc rỗng)
			if (string.IsNullOrEmpty(user.ProfilePictureUrl))
			{
				// Trả về hình ảnh mặc định nếu không có hình ảnh của người dùng
				imagePath = "/img/default-avatar.png";
			}

			// Trả về đường dẫn hình ảnh (chuỗi)
			return Ok(imagePath); // Trả về đường dẫn hình ảnh dưới dạng chuỗi
		}
	}
}
