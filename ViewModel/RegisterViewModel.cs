using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.ViewModel
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Tên không được để trống.")]
		[Display(Name = "Họ Tên")]
		public string FullName { get; set; } = string.Empty;
		[Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
		[Display(Name = "Tên đăng nhập")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email không được để trống.")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ.")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Mật khẩu không được để trống.")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
		[DataType(DataType.Password)]
		[Display(Name = "Xác nhận mật khẩu")]
		[Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
