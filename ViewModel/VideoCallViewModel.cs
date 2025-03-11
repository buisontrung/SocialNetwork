using NuGet.Protocol.Plugins;

namespace SocialNetwork.ViewModel
{
	public class VideoCallViewModel
	{
		 public UserViewModel? sender { get; set; }
		public UserViewModel? receiver { get; set; }
	}
}
