using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.DTOs;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DemoShop_QuaMonMot.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly DemoShopContext _context;

        public KhachHangController(DemoShopContext context)
        {
            _context = context;
        }

        #region Đăng ký - Đăng nhập - Đăng xuất

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangKy(DangKy model)
        {
            if (ModelState.IsValid)
            {
                var khachHang = new KhachHang
                {
                    MaKh = model.MaKh,
                    MatKhau = model.MatKhau,
                    HoTen = model.HoTen,
                    GioiTinh = model.GioiTinh,
                    NgaySinh = model.NgaySinh ?? DateTime.Now,
                    DiaChi = model.DiaChi,
                    DienThoai = model.DienThoai,
                    Email = model.Email ?? string.Empty,
                    Hinh = model.Hinh,
                    HieuLuc = true,
                    VaiTro = 0,
                    RandomKey = Guid.NewGuid().ToString()
                };

                _context.Add(khachHang);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(DemoShop_QuaMonMot.DTOs.Login model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var khachHang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);

                if (khachHang != null && khachHang.MatKhau == model.Password)
                {
                    HttpContext.Session.SetString("MaKh", khachHang.MaKh);
                    HttpContext.Session.SetString("HoTen", khachHang.HoTen);
                    HttpContext.Session.SetInt32("VaiTro", khachHang.VaiTro);

                    if (!string.IsNullOrEmpty(ReturnUrl)) return Redirect(ReturnUrl);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
            }
            return View(model);
        }

        public IActionResult DangXuat()
        {
            // 1. Xóa sạch dữ liệu trong Session hiện tại
            HttpContext.Session.Clear();

            // 2. Xóa cookie session
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie == "My.Session")
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Quản lý Hồ sơ & Đổi mật khẩu

        // --- 1. HIỂN THỊ HỒ SƠ (GET) ---
        [HttpGet]
        public IActionResult Profile()
        {
            var maKh = HttpContext.Session.GetString("MaKh");
            if (string.IsNullOrEmpty(maKh))
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var kh = _context.KhachHangs.SingleOrDefault(k => k.MaKh == maKh);
            if (kh == null)
            {
                return NotFound();
            }

            var model = new ProfileVM
            {
                MaKh = kh.MaKh,
                HoTen = kh.HoTen,
                GioiTinh = kh.GioiTinh,
                NgaySinh = kh.NgaySinh,
                Email = kh.Email,
                DienThoai = kh.DienThoai,
                DiaChi = kh.DiaChi
            };

            return View(model);
        }

        // --- 2. CẬP NHẬT THÔNG TIN CÁ NHÂN (POST) ---
        [HttpPost]
        public IActionResult UpdateProfile(ProfileVM model)
        {
            var maKh = HttpContext.Session.GetString("MaKh");
            if (string.IsNullOrEmpty(maKh)) return RedirectToAction("DangNhap");

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var kh = _context.KhachHangs.SingleOrDefault(k => k.MaKh == maKh);
            if (kh != null)
            {
                try
                {
                    kh.HoTen = model.HoTen;
                    kh.GioiTinh = model.GioiTinh;
                    kh.NgaySinh = model.NgaySinh;
                    kh.DienThoai = model.DienThoai;
                    kh.DiaChi = model.DiaChi;

                    _context.SaveChanges();

                    HttpContext.Session.SetString("HoTen", kh.HoTen);

                    TempData["Message"] = "Cập nhật thông tin cá nhân thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }
            return RedirectToAction("Profile");
        }

        // --- 3. ĐỔI MẬT KHẨU (POST) ---
        [HttpPost]
        public IActionResult ChangePassword(ProfileVM model)
        {
            var maKh = HttpContext.Session.GetString("MaKh");
            if (string.IsNullOrEmpty(maKh)) return RedirectToAction("DangNhap");

            var kh = _context.KhachHangs.SingleOrDefault(k => k.MaKh == maKh);

            if (kh != null)
            {
                if (kh.MatKhau != model.OldPassword)
                {
                    TempData["Error"] = "Mật khẩu cũ không chính xác!";
                    return RedirectToAction("Profile");
                }

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    kh.MatKhau = model.NewPassword;
                    _context.SaveChanges();
                    TempData["Message"] = "Đổi mật khẩu thành công!";
                }
                else
                {
                    TempData["Error"] = "Vui lòng nhập mật khẩu mới!";
                }
            }

            return RedirectToAction("Profile");
        }

        #endregion
    }
}
