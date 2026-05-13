using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Helpers;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<CartItem> SessionCart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

        private void MergeSessionCartToDatabase(string maKh)
        {
            var sessionCart = SessionCart;
            if (sessionCart.Count == 0)
            {
                return;
            }

            foreach (var cartItem in sessionCart)
            {
                var gioHangDb = _context.GioHangs.FirstOrDefault(g => g.MaKh == maKh && g.MaHh == cartItem.MaHh);
                if (gioHangDb == null)
                {
                    _context.GioHangs.Add(new GioHang
                    {
                        MaKh = maKh,
                        MaHh = cartItem.MaHh,
                        SoLuong = cartItem.Soluong,
                        NgayChon = DateTime.Now,
                        SessionId = HttpContext.Session.Id
                    });
                }
                else
                {
                    gioHangDb.SoLuong += cartItem.Soluong;
                    gioHangDb.NgayChon = DateTime.Now;
                }
            }

            _context.SaveChanges();
            HttpContext.Session.Set(CART_KEY, new List<CartItem>());
        }

        private List<CartItem> GetCheckoutCart()
        {
            var maKh = HttpContext.Session.GetString("MaKh");
            var sessionCart = SessionCart;

            if (string.IsNullOrEmpty(maKh))
            {
                return sessionCart;
            }

            var databaseCart = _context.GioHangs
                .Where(g => g.MaKh == maKh)
                .Include(g => g.MaHhNavigation)
                .Select(gh => new CartItem
                {
                    MaHh = gh.MaHh,
                    tenHH = gh.MaHhNavigation.TenHh,
                    Hinh = gh.MaHhNavigation.Hinh,
                    DonGia = gh.MaHhNavigation.DonGia ?? 0,
                    Soluong = gh.SoLuong
                })
                .ToList();

            return databaseCart.Count > 0 ? databaseCart : sessionCart;
        }

        private void ClearCheckoutCart(string maKh)
        {
            var gioHangDb = _context.GioHangs.Where(g => g.MaKh == maKh).ToList();
            if (gioHangDb.Count > 0)
            {
                _context.GioHangs.RemoveRange(gioHangDb);
                _context.SaveChanges();
            }

            HttpContext.Session.Set(CART_KEY, new List<CartItem>());
        }

        private IActionResult RedirectToLoginForCheckout()
        {
            TempData["LoginRequiredMessage"] = "Bạn phải đăng nhập để thanh toán";
            return RedirectToAction(
                "DangNhap",
                "KhachHang",
                new { ReturnUrl = Url.Action("ThanhToan", "Cart") });
        }

        private int? GetPendingOrderStatusId()
        {
            return _context.TrangThais
                .OrderBy(tt => tt.MaTrangThai)
                .Select(tt => (int?)tt.MaTrangThai)
                .FirstOrDefault();
        }

        // --- 1. HIỂN THỊ GIỎ HÀNG ---
        public IActionResult Index()
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                MergeSessionCartToDatabase(maKh);

                // THÀNH VIÊN: Lấy từ Database (Join với HangHoa để lấy đầy đủ thông tin)
                var data = _context.GioHangs
                    .Where(g => g.MaKh == maKh)
                    .Include(g => g.MaHhNavigation)
                    .Select(gh => new CartItem
                    {
                        MaHh = gh.MaHh,
                        tenHH = gh.MaHhNavigation.TenHh,
                        Hinh = gh.MaHhNavigation.Hinh,
                        DonGia = gh.MaHhNavigation.DonGia ?? 0,
                        Soluong = gh.SoLuong // Đã khớp tên biến Soluong
                    }).ToList();
                return View(data);
            }

            // KHÁCH: Lấy từ Session
            return View(SessionCart);
        }

        // --- 2. THÊM VÀO GIỎ HÀNG ---
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                // LƯU VÀO DATABASE
                var item = _context.GioHangs.FirstOrDefault(g => g.MaKh == maKh && g.MaHh == id);
                if (item == null)
                {
                    _context.GioHangs.Add(new GioHang
                    {
                        MaKh = maKh,
                        MaHh = id,
                        SoLuong = quantity,
                        NgayChon = DateTime.Now,
                        SessionId = HttpContext.Session.Id
                    });
                }
                else
                {
                    item.SoLuong += quantity;
                }
                _context.SaveChanges();
            }
            else
            {
                // LƯU VÀO SESSION
                var gioHang = SessionCart;
                var item = gioHang.SingleOrDefault(p => p.MaHh == id);
                if (item == null)
                {
                    var hh = _context.HangHoas.SingleOrDefault(p => p.MaHh == id);
                    if (hh == null) return NotFound();

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
                HttpContext.Session.Set(CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        // --- 3. TĂNG/GIẢM SỐ LƯỢNG (Sửa lỗi 404) ---
        public IActionResult UpdateQuantity(int id, int amount)
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                // CẬP NHẬT DATABASE
                var item = _context.GioHangs.FirstOrDefault(g => g.MaKh == maKh && g.MaHh == id);
                if (item != null)
                {
                    item.SoLuong += amount;
                    if (item.SoLuong <= 0) _context.GioHangs.Remove(item);
                    _context.SaveChanges();
                }
            }
            else
            {
                // CẬP NHẬT SESSION
                var gioHang = SessionCart;
                var item = gioHang.SingleOrDefault(p => p.MaHh == id);
                if (item != null)
                {
                    item.Soluong += amount;
                    if (item.Soluong <= 0) gioHang.Remove(item);
                    HttpContext.Session.Set(CART_KEY, gioHang);
                }
            }
            return RedirectToAction("Index");
        }

        // --- 4. XÓA MÓN HÀNG ---
        public IActionResult RemoveCart(int id)
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
                // XÓA TRONG DATABASE
                var item = _context.GioHangs.FirstOrDefault(g => g.MaHh == id && g.MaKh == maKh);
                if (item != null)
                {
                    _context.GioHangs.Remove(item);
                    _context.SaveChanges();
                }
            }
            else
            {
                // XÓA TRONG SESSION
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
        public IActionResult ThanhToan()
        {
            var maKh = HttpContext.Session.GetString("MaKh");
            if (string.IsNullOrEmpty(maKh))
            {
                return RedirectToLoginForCheckout();
            }

            MergeSessionCartToDatabase(maKh);
            var gioHang = GetCheckoutCart();

            if (gioHang.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var khachHang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == maKh);
            ViewBag.HoTen = khachHang?.HoTen;
            ViewBag.DiaChi = khachHang?.DiaChi;
            ViewBag.DienThoai = khachHang?.DienThoai;

            return View(gioHang);
        }
        [HttpPost]
        public IActionResult ThanhToan(string HoTen, string DiaChi, string DienThoai, string GhiChu, double PhiVanChuyen)
        {
            var maKh = HttpContext.Session.GetString("MaKh");

            if (string.IsNullOrEmpty(maKh))
            {
                return RedirectToLoginForCheckout();
            }

            MergeSessionCartToDatabase(maKh);
            var gioHang = GetCheckoutCart();

            if (gioHang.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var phiVanChuyen = gioHang.Sum(item => item.ThanhTien) > 0 ? 10.0 : 0.0;
            var maTrangThai = GetPendingOrderStatusId();

            if (string.IsNullOrWhiteSpace(HoTen))
            {
                ModelState.AddModelError(nameof(HoTen), "Vui lòng nhập họ tên người nhận.");
            }

            if (string.IsNullOrWhiteSpace(DienThoai))
            {
                ModelState.AddModelError(nameof(DienThoai), "Vui lòng nhập số điện thoại.");
            }

            if (string.IsNullOrWhiteSpace(DiaChi))
            {
                ModelState.AddModelError(nameof(DiaChi), "Vui lòng nhập địa chỉ giao hàng.");
            }

            if (maTrangThai == null)
            {
                ModelState.AddModelError(string.Empty, "Chưa có trạng thái đơn hàng trong hệ thống. Vui lòng thêm dữ liệu TrangThai trước khi đặt hàng.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.HoTen = HoTen;
                ViewBag.DiaChi = DiaChi;
                ViewBag.DienThoai = DienThoai;
                ViewBag.GhiChu = GhiChu;
                return View(gioHang);
            }

            var maTrangThaiValue = maTrangThai.GetValueOrDefault();
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var hoaDon = new HoaDon
                {
                    MaKh = maKh,
                    HoTen = HoTen.Trim(),
                    DiaChi = DiaChi.Trim(),
                    DienThoai = DienThoai.Trim(),
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "Giao hàng tận nơi",
                    PhiVanChuyen = phiVanChuyen,
                    MaTrangThai = maTrangThaiValue,
                    GhiChu = string.IsNullOrWhiteSpace(GhiChu) ? null : GhiChu.Trim()
                };

                _context.HoaDons.Add(hoaDon);
                _context.SaveChanges();

                foreach (var item in gioHang)
                {
                    var chiTiet = new ChiTietHd
                    {
                        MaHd = hoaDon.MaHd,
                        MaHh = item.MaHh,
                        DonGia = item.DonGia,
                        SoLuong = item.Soluong,
                        GiamGia = 0
                    };

                    _context.ChiTietHds.Add(chiTiet);
                }

                _context.SaveChanges();
                ClearCheckoutCart(maKh);
                transaction.Commit();

                TempData["OrderId"] = hoaDon.MaHd;
                TempData["OrderTotal"] = (gioHang.Sum(item => item.ThanhTien) + phiVanChuyen).ToString("#,##0.00");

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError(string.Empty, "Không thể đặt hàng lúc này: " + ex.Message);
                ViewBag.HoTen = HoTen;
                ViewBag.DiaChi = DiaChi;
                ViewBag.DienThoai = DienThoai;
                ViewBag.GhiChu = GhiChu;
                return View(gioHang);
            }
        }

        public IActionResult Success()
        {
            return View();
        }

    }
}
