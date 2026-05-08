using System;
using System.Collections.Generic;

namespace DemoShop_QuaMonMot.Models;

public partial class GioHang
{
    public int MaGh { get; set; }

    public string SessionId { get; set; } = null!;

    public string? MaKh { get; set; } = null!;

    public int MaHh { get; set; }

    public int SoLuong { get; set; }

    public DateTime? NgayChon { get; set; }

    public virtual HangHoa MaHhNavigation { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
