using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.hub;
using SocialNetwork.Models;
using SocialNetwork.Service;

namespace SocialNetwork
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
			);
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			builder.Services.AddScoped<IFileService, FileService>();
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Auth/Login"; // Đường dẫn tới trang đăng nhập
				options.AccessDeniedPath = "/Auth/Login"; // Đường dẫn khi không có quyền truy cập;

			});
            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache(); 
            builder.Services.AddSession();
			builder.Services.AddSignalR();
			builder.Logging.ClearProviders();
			builder.Logging.AddConsole();
			builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
			app.UseSession();
			app.UseAuthorization();
			app.MapHub<CommentHub>("/commentHub");
			app.MapHub<ConversionHub>("/conversionHub");
			app.MapHub<FriendHub>("/friendHub");
			app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
