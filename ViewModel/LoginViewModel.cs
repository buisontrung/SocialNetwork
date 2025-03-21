﻿using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.ViewModel
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
		[Display(Name = "Tên đăng nhập")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "Mật khẩu không được để trống.")]
		[DataType(DataType.Password)]
		[Display(Name = "Mật khẩu")]
		public string Password { get; set; } = string.Empty;
		[Display(Name = "Ghi nhớ")]
		public bool RememberMe { get; set; } = false;
	}
}
