using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Models;
using DemoShop_QuaMonMot.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DemoShop_QuaMonMot.Controllers
{
    public class AdminController : Controller
    {
        private readonly DemoShopContext _context;

        public AdminController(DemoShopContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetInt32("VaiTro") == 1;

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "KhachHang");

            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile? fHinh)
        {
            var hangHoa = new HangHoa();
            try
            {
                hangHoa.TenHh = Request.Form["TenHh"];
                hangHoa.MoTa = Request.Form["MoTa"]; 
                hangHoa.DonGia = double.Parse(Request.Form["DonGia"]);
                hangHoa.MaLoai = int.Parse(Request.Form["MaLoai"]);
                hangHoa.MaNcc = Request.Form["MaNcc"];
                hangHoa.NgaySx = DateTime.Now;
                hangHoa.TenAlias = Util.GenerateAlias(hangHoa.TenHh);

                if (fHinh != null)
                {
                    hangHoa.Hinh = Util.UploadImg(fHinh, "HangHoa");
                }

                _context.HangHoas.Add(hangHoa);
                await _context.SaveChangesAsync(); 

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi lưu DB: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "KhachHang");

            var hangHoa = _context.HangHoas.Find(id);
            if (hangHoa == null) return NotFound();

            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", hangHoa.MaNcc);
            return View(hangHoa);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, IFormFile? fHinh)
        {
            if (!IsAdmin()) return Forbid();

            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null) return NotFound();

            try
            {
                hangHoa.TenHh = Request.Form["TenHh"];
                hangHoa.TenAlias = hangHoa.TenHh.ToLower().Replace(" ", "-");

                if (double.TryParse(Request.Form["DonGia"], out double gia)) hangHoa.DonGia = gia;
                if (double.TryParse(Request.Form["GiamGia"], out double gg)) hangHoa.GiamGia = gg;

                hangHoa.MoTa = Request.Form["MoTa"];
                hangHoa.MaLoai = int.Parse(Request.Form["MaLoai"]);
                hangHoa.MaNcc = Request.Form["MaNcc"];

                if (DateTime.TryParse(Request.Form["NgaySx"], out DateTime nsx))
                    hangHoa.NgaySx = nsx;

                if (fHinh != null)
                {
                    string fileName = Util.UploadImg(fHinh, "HangHoa");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        hangHoa.Hinh = fileName;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
            }

            ViewBag.MaLoai = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewBag.MaNcc = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", hangHoa.MaNcc);
            return View(hangHoa);
        }
        // Action này dùng để thực thi việc xóa sản phẩm
        [HttpPost]
        public async Task<IActionResult> DeleteHangHoa(int id)
        {
            // 1. Kiểm tra quyền Admin (giống các hàm khác của bạn)
            if (HttpContext.Session.GetInt32("VaiTro") != 1)
            {
                return Forbid();
            }

            try
            {
                // 2. Tìm sản phẩm theo ID
                var hangHoa = await _context.HangHoas.FindAsync(id);

                if (hangHoa == null)
                {
                    return NotFound();
                }

                // 3. Thực hiện xóa
                _context.HangHoas.Remove(hangHoa);
                await _context.SaveChangesAsync();

                // 4. Xóa thành công thì quay về trang chủ (hoặc trang danh sách admin)
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Nếu sản phẩm này đã có trong đơn hàng (khóa ngoại), DB sẽ báo lỗi không cho xóa
                TempData["Error"] = "Không thể xóa sản phẩm này vì đã có dữ liệu liên quan trong hóa đơn!";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}