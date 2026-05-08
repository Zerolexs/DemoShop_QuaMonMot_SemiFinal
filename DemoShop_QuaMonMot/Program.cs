using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Helpers;
using DemoShop_QuaMonMot.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DemoShop_QuaMonMot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            var myConnectionString = builder.Configuration.GetConnectionString("MyConnectString");
            builder.Services.AddDbContext<DemoShopContext>(option => option.UseSqlServer(myConnectionString));
            // 1. Đăng ký Distributed Cache để Session hoạt động
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "My.Session";
                //options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
            });
            // Đăng ký AutoMapper
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
            var app = builder.Build();
            // // Register your DbContext 



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
