using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Helpers;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;

namespace DemoShop_QuaMonMot.Controllers
{
    public class CartController : Controller
    {
        private readonly DemoShopContext _context;
        private const string CART_KEY = "MYCART";

        public CartController(DemoShopContext context)
        {
            _context = context;
        }

        // Property bổ trợ để lấy giỏ hàng từ Session nhanh hơn
        public List<CartItem> SessionCart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                // TRƯỜNG HỢP ĐÃ ĐĂNG NHẬP: Lấy dữ liệu từ Database
                var data = _context.GioHangs
                    .Where(g => g.MaKh == maKh)
                    .Join(_context.HangHoas,
                          gh => gh.MaHh,
                          hh => hh.MaHh,
                          (gh, hh) => new CartItem
                          {
                              MaHh = hh.MaHh,
                              tenHH = hh.TenHh,
                              Hinh = hh.Hinh,
                              DonGia = hh.DonGia ?? 0,
                              Soluong = gh.SoLuong
                          }).ToList();
                return View(data);
            }

            // TRƯỜNG HỢP ẨN DANH: Lấy dữ liệu từ Session
            return View(SessionCart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                // LƯU VÀO DATABASE CHO THÀNH VIÊN
                var item = _context.GioHangs.FirstOrDefault(g => g.MaKh == maKh && g.MaHh == id);
                if (item == null)
                {
                    var hh = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
                    if (hh == null) return Redirect("/404");

                    item = new GioHang
                    {
                        MaKh = maKh,
                        MaHh = id,
                        SoLuong = quantity,
                        NgayChon = DateTime.Now,
                        SessionId = HttpContext.Session.Id // Vẫn lưu để tham chiếu nếu cần
                    };
                    _context.GioHangs.Add(item);
                }
                else
                {
                    item.SoLuong += quantity;
                }
                _context.SaveChanges();
            }
            else
            {
                // LƯU VÀO SESSION CHO KHÁCH ẨN DANH
                var gioHang = SessionCart;
                var item = gioHang.SingleOrDefault(p => p.MaHh == id);
                if (item == null)
                {
                    var hh = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
                    if (hh == null) return Redirect("/404");

                    gioHang.Add(new CartItem
                    {
                        MaHh = id,
                        tenHH = hh.TenHh,
                        DonGia = hh.DonGia ?? 0,
                        Hinh = hh.Hinh,
                        Soluong = quantity
                    });
                }
                else
                {
                    item.Soluong += quantity;
                }
                // Cập nhật lại Session
                HttpContext.Session.Set(CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id)
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                var item = _context.GioHangs.FirstOrDefault(g => g.MaHh == id && g.MaKh == maKh);
                if (item != null)
                {
                    _context.GioHangs.Remove(item);
                    _context.SaveChanges();
                }
            }
            else
            {
                var gioHang = SessionCart;
                var item = gioHang.SingleOrDefault(p => p.MaHh == id);
                if (item != null)
                {
                    gioHang.Remove(item);
                    HttpContext.Session.Set(CART_KEY, gioHang);
                }
            }
            return RedirectToAction("Index");
        }
    }
}