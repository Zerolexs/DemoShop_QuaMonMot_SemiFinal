using System.ComponentModel.DataAnnotations;

namespace DemoShop_QuaMonMot.Models
{
    public class ProfileVM
    {
        // Thông tin cá nhân (lấy từ KhachHang.cs)
        [Display(Name = "Mã khách hàng")]
        public string MaKh { get; set; } = null!;

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "*")]
        public string HoTen { get; set; } = null!;

        [Display(Name = "Giới tính")]
        public bool GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"0\d{9,10}", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string? DienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // Đổi mật khẩu
        [Display(Name = "Mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu mới ít nhất 6 ký tự")]
        public string? NewPassword { get; set; }

        [Display(Name = "Xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}