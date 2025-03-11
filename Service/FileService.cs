namespace SocialNetwork.Service
{
	public class FileService : IFileService
	{
		private readonly IWebHostEnvironment _environment;

		public FileService(IWebHostEnvironment environment)
		{
			_environment = environment;
		}

		public async Task<string> SaveFileAsync(IFormFile file, string folder)
		{
			if (file == null || file.Length == 0)
			{
				throw new ArgumentException("File không hợp lệ.");
			}

			// Định nghĩa thư mục lưu trữ trong wwwroot
			string uploadsFolder = Path.Combine(_environment.WebRootPath, folder);

			// Tạo thư mục nếu chưa tồn tại
			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}

			// Tạo tên file duy nhất
			string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
			string filePath = Path.Combine(uploadsFolder, uniqueFileName);

			// Lưu file vào thư mục
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			// Trả về đường dẫn để hiển thị ảnh
			return uniqueFileName;
		}
	}

}
