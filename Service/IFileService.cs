namespace SocialNetwork.Service
{
	public interface IFileService
	{
		Task<string> SaveFileAsync(IFormFile file, string folder);
	}
}
