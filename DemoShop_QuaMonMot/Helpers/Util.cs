using System.Text;
using System.Text.RegularExpressions;

namespace DemoShop_QuaMonMot.Helpers
{
    public class Util
    {
        public static string UploadImg(IFormFile Hinh, string folder)
        {
            try
            {
                if (Hinh == null) return string.Empty;

                string extension = Path.GetExtension(Hinh.FileName);
                string fileNameOnly = Path.GetFileNameWithoutExtension(Hinh.FileName);
                string safeFileName = GenerateAlias(fileNameOnly) + extension;

                // 2. Đường dẫn vật lý
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var fullPath = Path.Combine(path, safeFileName);

                // 3. Lưu file (Ghi đè nếu trùng tên)
                using (var myfile = new FileStream(fullPath, FileMode.Create))
                {
                    Hinh.CopyTo(myfile);
                }

                return safeFileName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GenerateAlias(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            str = str.ToLower().Trim();
            str = Regex.Replace(str, @"[áàảãạâấầẩẫậăắằẳẵặ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-").Trim();
            str = Regex.Replace(str, @"-+", "-");
            return str;
        }
    }
}