using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Helpers;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DemoShop_QuaMonMot.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly DemoShopContext _context;
        public CartSummaryViewComponent(DemoShopContext context) => _context = context;

        public IViewComponentResult Invoke()
        {
            string? maKh = HttpContext.Session.GetString("MaKh");
            int totalQuantity = 0;

            if (!string.IsNullOrEmpty(maKh))
            {
                // Nếu đã đăng nhập: Đếm tổng số lượng từ Database
                totalQuantity = _context.GioHangs
                    .Where(g => g.MaKh == maKh)
                    .Sum(g => (int?)g.SoLuong) ?? 0;
            }
            else
            {
                // Nếu là khách: Đếm tổng số lượng từ Session (CART_KEY phải khớp với Controller)
                var cart = HttpContext.Session.Get<List<CartItem>>("MYCART") ?? new List<CartItem>();
                totalQuantity = cart.Sum(c => c.Soluong);
            }
            Console.WriteLine($"Cart Count: {totalQuantity}");
            return View(totalQuantity);
        }
    }
}
