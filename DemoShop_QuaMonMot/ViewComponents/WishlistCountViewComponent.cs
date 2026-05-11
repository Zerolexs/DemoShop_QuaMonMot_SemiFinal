using DemoShop_QuaMonMot.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class WishlistCountViewComponent : ViewComponent
{
    private readonly DemoShopContext _context;
    public WishlistCountViewComponent(DemoShopContext context) => _context = context;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var maKh = HttpContext.Session.GetString("MaKh");
        int count = 0;
        if (!string.IsNullOrEmpty(maKh))
        {
            count = await _context.YeuThiches.CountAsync(y => y.MaKh == maKh);
        }
        return View(count);
    }
}