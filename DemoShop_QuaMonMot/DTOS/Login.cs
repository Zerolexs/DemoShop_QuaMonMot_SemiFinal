using System.ComponentModel.DataAnnotations;

namespace DemoShop_QuaMonMot.DTOs;


public class Login
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Chưa nhập tên đăng nhập")]
    [MaxLength(20, ErrorMessage = "Tối đa 20 kí tự")]
    public string UserName { get; set; }

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Chưa nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberLogin { get; set; }

    public string? ReturnUrl { get; set; }
}

