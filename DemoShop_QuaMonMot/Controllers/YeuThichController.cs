using DemoShop_QuaMonMot.Data;
using DemoShop_QuaMonMot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class YeuThichController : Controller
{
    private readonly DemoShopContext _context;
    public YeuThichController(DemoShopContext context) => _context = context;

    // Hiển thị danh sách yêu thích
    public async Task<IActionResult> Index()
    {
        var maKh = HttpContext.Session.GetString("MaKh");
        if (string.IsNullOrEmpty(maKh)) return RedirectToAction("DangNhap", "KhachHang");

        var data = await _context.YeuThiches
            .Where(y => y.MaKh == maKh)
            .Include(y => y.MaHhNavigation) // Join với bảng HangHoa
            .Select(y => y.MaHhNavigation)
            .ToListAsync();

        return View(data);
    }

    // Thêm vào yêu thích (Dùng AJAX sẽ mượt hơn)
    //[HttpPost]
    public async Task<IActionResult> Add(int id)
    {
        // Đảm bảo chữ "MaKh" viết giống hệt như lúc bạn lưu ở KhachHangController
        var maKh = HttpContext.Session.GetString("MaKh");

        if (string.IsNullOrEmpty(maKh))
        {
            // Khi dùng AJAX, ta trả về JSON để Script xử lý
            return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng này!" });
        }

        var item = await _context.YeuThiches
            .FirstOrDefaultAsync(y => y.MaKh == maKh && y.MaHh == id);

        string action = "";
        if (item != null)
        {
            _context.YeuThiches.Remove(item);
            action = "removed";
        }
        else
        {
            _context.YeuThiches.Add(new YeuThich { MaKh = maKh, MaHh = id, NgayChon = DateTime.Now });
            action = "added";
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, action = action });
    }

    // Không dùng [HttpPost] nếu bạn muốn dùng thẻ <a> click cho nhanh
    public async Task<IActionResult> Remove(int id)
    {
        var maKh = HttpContext.Session.GetString("MaKh");
        if (string.IsNullOrEmpty(maKh)) return RedirectToAction("DangNhap", "KhachHang");

        // Tìm món hàng cần xóa
        var item = await _context.YeuThiches
            .FirstOrDefaultAsync(y => y.MaKh == maKh && y.MaHh == id);

        if (item != null)
        {
            _context.YeuThiches.Remove(item);
            await _context.SaveChangesAsync();
        }

        // Xóa xong thì quay về trang danh sách yêu thích
        return RedirectToAction("Index");
    }
}