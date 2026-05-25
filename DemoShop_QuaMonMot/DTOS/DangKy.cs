using System.ComponentModel.DataAnnotations;
namespace DemoShop_QuaMonMot.DTOs;
public class DangKy
{
    [Key]
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "*")]
    [MaxLength(20, ErrorMessage = "Tối đa 20 kí tự")]
    public string MaKh { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "*")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    [Display(Name = "Họ tên")]
    [Required(ErrorMessage = "*")]
    [MaxLength(50, ErrorMessage = "Tối đa 50 kí tự")]
    public string HoTen { get; set; } = string.Empty;

    [Display(Name = "Giới tính")]
    public bool GioiTinh { get; set; } = true;

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "Địa chỉ")]
    [MaxLength(60, ErrorMessage = "Tối đa 60 kí tự")]

    [Required(ErrorMessage = "*")]
    public string DiaChi { get; set; } = string.Empty;

    [Display(Name = "Điện thoại")]
    [Required(ErrorMessage = "*")]
    [MaxLength(20, ErrorMessage = "Tối đa 20 kí tự")]
    [RegularExpression(@"0[39875]\d{8}", ErrorMessage = "Chưa đúng định dạng di động Việt Nam")]
    public string DienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "*")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email address")]
    [MaxLength(50)]
    [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Chưa đúng định dạng email")]
    public string? Email { get; set; }

    public string? Hinh { get; set; }
}



