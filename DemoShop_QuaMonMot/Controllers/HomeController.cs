using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Localization;
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

        private async Task LoadDanhMucAsync()
        {
            var danhmucgoc = await _context.Loais.Include(l => l.HangHoas).ToListAsync();

            var danhMuc = danhmucgoc.Select(l => new
            {
                MaLoai = l.MaLoai,
                TenLoai = l.TenLoai.ToLower() switch
                {
                    "laptop" => "Laptop",
                    "đồng hồ" => "Đồng hồ",
                    "máy ảnh" => "Máy ảnh",
                    "điện thoại" => "Điện thoại",
                    "nước hoa" => "Nước hoa",
                    "trang sức" => "Trang sức",
                    "giày" => "Giày",
                    "vali" => "Vali",
                    _ => l.TenLoai
                },
                SoLuong = l.HangHoas.Count()
            }).ToList();

            ViewBag.DanhMuc = danhMuc;
        }

        public async Task<IActionResult> TrangChu()
        {
            await LoadDanhMucAsync();

            var products = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .OrderByDescending(h => h.MaHh)
                .Take(16)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Index(string? sort, string? search, int page = 1, int record = 9, string[]? priceRanges = null, int? maLoai = null)
        {
            var query = _context.HangHoas.AsQueryable();

            // --- 1. Lấy danh sách danh mục ---
            await LoadDanhMucAsync();
            // 2. Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                // Lọc sản phẩm có tên chứa từ khóa (không phân biệt hoa thường trong SQL)
                query = query.Where(h => h.TenHh.Contains(search));
            }
            // --- 2. Logic lọc theo danh mục ---
            if (maLoai.HasValue)
            {
                query = query.Where(h => h.MaLoai == maLoai.Value);
            }

            // --- 3. Luôn luôn tính số lượng sản phẩm cho bộ lọc Giá (Để ngoài IF) ---
            ViewBag.AllCount = await _context.HangHoas.CountAsync();
            var priceRangeCounts = new Dictionary<string, int>();
            for (var min = 0; min < 100; min += 10)
            {
                var max = min + 10;
                priceRangeCounts[$"{min}-{max}"] = await _context.HangHoas
                    .CountAsync(h => h.DonGia >= min && h.DonGia < max);
            }
            priceRangeCounts["100-plus"] = await _context.HangHoas
                .CountAsync(h => h.DonGia >= 100);
            ViewBag.PriceRangeCounts = priceRangeCounts;

            // --- 4. Logic lọc priceRanges ---
            if (priceRanges != null && priceRanges.Length > 0)
            {
                var allMatchedIds = new List<int>();
                foreach (var range in priceRanges)
                {
                    if (range == "100-plus")
                    {
                        var ids = await _context.HangHoas
                            .Where(h => h.DonGia >= 100)
                            .Select(h => h.MaHh)
                            .ToListAsync();
                        allMatchedIds.AddRange(ids);
                        continue;
                    }

                    var parts = range.Split('-');
                    if (parts.Length == 2 && double.TryParse(parts[0], out double min) && double.TryParse(parts[1], out double max))
                    {
                        var ids = await _context.HangHoas
                            .Where(h => h.DonGia >= min && h.DonGia < max)
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

            ViewBag.CurrentSort = sort; 

           

            // --- 5. Phân trang và trả về View ---
            int totalRecord = await query.CountAsync();
            ViewBag.TotalPage = (int)Math.Ceiling((double)totalRecord / record);
            ViewBag.CurrentPage = page;

            var maKh = HttpContext.Session.GetString("MaKh");
            if (!string.IsNullOrEmpty(maKh))
            {
                ViewBag.FavoriteIds = _context.YeuThiches
                    .Where(y => y.MaKh == maKh)
                    .Select(y => y.MaHh).ToList();
            }

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

        [HttpGet]
        public async Task<IActionResult> LandingProducts(int take = 16)
        {
            var products = await _context.HangHoas
                .OrderByDescending(h => h.MaHh)
                .Take(take)
                .Select(h => new
                {
                    id = h.MaHh,
                    name = h.TenHh,
                    price = h.DonGia ?? 0,
                    oldPrice = Math.Round((h.DonGia ?? 0) * 1.1, 2),
                    imageUrl = Url.Content("~/Hinh/HangHoa/" + (string.IsNullOrEmpty(h.Hinh) ? "default.jpg" : h.Hinh)),
                    detailUrl = Url.Action("Detail", "Home", new { id = h.MaHh }),
                    cartUrl = Url.Action("AddToCart", "Cart", new { id = h.MaHh })
                })
                .ToListAsync();

            return Json(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                });

            return LocalRedirect(returnUrl);
        }
    }
}
