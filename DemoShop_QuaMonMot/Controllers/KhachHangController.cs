using AutoMapper;
using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.DTOs;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace DemoShop_QuaMonMot.Controllers
{

    public class KhachHangController : Controller
    {
        private readonly DemoShopContext _context;
        private readonly IMapper _mapper; 

        public KhachHangController(DemoShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
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
                // AUTO MAPPING THAY CHO GÁN THỦ CÔNG
                var khachHang = _mapper.Map<KhachHang>(model);

                // Gán thêm các trường mà DTO không có
                khachHang.HieuLuc = true;
                khachHang.VaiTro = 0;
                khachHang.RandomKey = Guid.NewGuid().ToString();

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
        // Hàm Đăng xuất
        public IActionResult DangXuat()
        {
            // 1. Xóa sạch dữ liệu trong Session hiện tại
            HttpContext.Session.Clear();

            // 2. Quan trọng: Xóa cookie session để trình duyệt cấp SessionId mới ở lần truy cập sau
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie == ".AspNetCore.Session") // Hoặc tên cookie session bạn cấu hình
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
