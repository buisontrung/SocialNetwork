using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Models;


namespace SocialNetwork.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Like> Like { get; set; }
		public DbSet<FriendRequest> FriendRequests { get; set; }
		
		public DbSet<Follower> Followers { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<UserSettings> UserSettings { get; set; }
		public DbSet<ApplicationUser> AspNetUsers { get; set; }
		public DbSet<Conversation> Conversations { get; set; }
		public DbSet<ConversationUser> ConversationUsers { get; set; }
		public DbSet<Notification> NotificationUsers { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Thiết lập mối quan hệ giữa ApplicationUser và Follower
			modelBuilder.Entity<Follower>()
				.HasOne(f => f.FollowingUser) // Người mà một người dùng đang theo dõi
				.WithMany(u => u.Followers)  // Người theo dõi của một người dùng
				.HasForeignKey(f => f.FollowingId)
				.OnDelete(DeleteBehavior.Restrict); // Đảm bảo không xoá cascade

			modelBuilder.Entity<Follower>()
				.HasOne(f => f.FollowerUser) // Người theo dõi
				.WithMany(u => u.Following) // Người mà một người dùng đang theo dõi
				.HasForeignKey(f => f.FollowerId)
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Message>()
		 .HasOne(m => m.Sender)
		 .WithMany(u => u.SentMessages)
		 .HasForeignKey(m => m.SenderId)
		 .OnDelete(DeleteBehavior.Restrict);


		}


	}
}
