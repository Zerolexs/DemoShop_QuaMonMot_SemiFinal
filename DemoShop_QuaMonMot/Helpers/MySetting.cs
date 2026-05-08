namespace DemoShop_QuaMonMot.Helpers;

public class MySetting
{
    public const string Username = "AccountName";

    public const string Email = "EmailAddress";    

    public const string ShopCart = "GioHang";  
    
    public const string UserID = "AccountId";

    public const string ClaimUser = "ClaimUser";
    
    public const string SessionID = "SessionID";

    // "MainConnectString" là tên của chuỗi kết nối được định nghĩa trong appsettings.json của ứng dụng. 
    public const string ConnectStringName = "MainConnectString";


    public static string UploadHinh(IFormFile fHinh, string folder)
    {
        try
        {
            // Tạo tên file mới để tránh trùng lặp: ví dụ 20240505_tencu.jpg
            string fileName = DateTime.Now.Ticks.ToString() + "_" + fHinh.FileName;

            // Đường dẫn đến thư mục wwwroot/Hinh/Folder (Folder ở đây là HangHoa)
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                fHinh.CopyTo(stream);
            }

            return fileName; // Trả về tên file để lưu vào Database
        }
        catch
        {
            return string.Empty;
        }
    }
}

