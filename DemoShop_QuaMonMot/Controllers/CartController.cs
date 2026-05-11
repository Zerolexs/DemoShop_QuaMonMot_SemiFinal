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

        // --- 1. HIỂN THỊ GIỎ HÀNG ---
        public IActionResult Index()
        {
            string maKh = HttpContext.Session.GetString("MaKh");

            if (!string.IsNullOrEmpty(maKh))
            {
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
                        NgayChon = DateTime.Now
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
    }
}