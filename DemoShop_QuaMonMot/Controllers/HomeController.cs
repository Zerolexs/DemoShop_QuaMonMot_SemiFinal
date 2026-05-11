using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DemoShop_QuaMonMot.Controllers
{
    public class HomeController : Controller
    {
        private readonly DemoShopContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DemoShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string sort, string? search, int page = 1, int record = 9, string[] priceRanges = null, int? maLoai = null)
        {
            var query = _context.HangHoas.AsQueryable();

            // --- 1. Lấy danh sách Danh mục và đổi tên sang Tiếng Anh ---
            var danhMucGốc = await _context.Loais.Include(l => l.HangHoas).ToListAsync();

            var danhMuc = danhMucGốc.Select(l => new
            {
                MaLoai = l.MaLoai,
                // Map tên từ DB sang Tiếng Anh
                TenLoai = l.TenLoai.ToLower() switch
                {
                    "laptop" => "Laptop",
                    "đồng hồ" => "Watch",
                    "máy ảnh" => "Camera",
                    "điện thoại" => "Phone",
                    "nước hoa" => "Perfume",
                    "trang sức" => "Jewelry",
                    "giày" => "Shoes",
                    "vali" => "Suitcase",
                    _ => l.TenLoai
                },
                SoLuong = l.HangHoas.Count()
            }).ToList();

            ViewBag.DanhMuc = danhMuc;
            // 2. Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                // Lọc sản phẩm có tên chứa từ khóa (không phân biệt hoa thường trong SQL)
                query = query.Where(h => h.TenHh.Contains(search));
            }
            // --- 2. Logic lọc theo Category ---
            if (maLoai.HasValue)
            {
                query = query.Where(h => h.MaLoai == maLoai.Value);
            }

            // --- 3. Luôn luôn tính số lượng sản phẩm cho bộ lọc Giá (Để ngoài IF) ---
            ViewBag.AllCount = await _context.HangHoas.CountAsync();
            ViewBag.Count0_100 = await _context.HangHoas.CountAsync(h => h.DonGia >= 0 && h.DonGia <= 100);
            ViewBag.Count100_200 = await _context.HangHoas.CountAsync(h => h.DonGia > 100 && h.DonGia <= 200);
            ViewBag.Count200_300 = await _context.HangHoas.CountAsync(h => h.DonGia > 200 && h.DonGia <= 300);
            ViewBag.Count300_400 = await _context.HangHoas.CountAsync(h => h.DonGia > 300 && h.DonGia <= 400);
            ViewBag.Count400_500 = await _context.HangHoas.CountAsync(h => h.DonGia > 400 && h.DonGia <= 500);

            // --- 4. Logic lọc priceRanges ---
            if (priceRanges != null && priceRanges.Length > 0)
            {
                var allMatchedIds = new List<int>();
                foreach (var range in priceRanges)
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 && double.TryParse(parts[0], out double min) && double.TryParse(parts[1], out double max))
                    {
                        var ids = await _context.HangHoas
                            .Where(h => h.DonGia >= min && h.DonGia <= max)
                            .Select(h => h.MaHh)
                            .ToListAsync();
                        allMatchedIds.AddRange(ids);
                    }
                }
                var finalIds = allMatchedIds.Distinct().ToList();
                query = query.Where(h => finalIds.Contains(h.MaHh));
            }
            switch (sort)
            {
                case "price_asc": query = query.OrderBy(p => p.DonGia); break;
                case "price_desc": query = query.OrderByDescending(p => p.DonGia); break;
                case "latest": query = query.OrderByDescending(p => p.MaHh); break;
                default: query = query.OrderBy(p => p.TenHh); break;
            }

            ViewBag.CurrentSort = sort; // Lưu lại để giữ trạng thái dropdown

            // --- 5. Phân trang và trả về View ---
            int totalRecord = await query.CountAsync();
            ViewBag.TotalPage = (int)Math.Ceiling((double)totalRecord / record);
            ViewBag.CurrentPage = page;

            var data = await query
                .Include(h => h.MaLoaiNavigation)
                .Skip((page - 1) * record)
                .Take(record)
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            // Lấy thông tin hàng hóa kèm theo Loại và Nhà cung cấp
            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(h => h.MaHh == id);

            if (hangHoa == null) return NotFound();

            // Tăng số lượt xem
            hangHoa.SoLanXem += 1;
            _context.Update(hangHoa);
            await _context.SaveChangesAsync();

            return View(hangHoa);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
