namespace DemoShop_QuaMonMot.Models
{
    public class CartItem
    {
        public int MaHh { get; set; }
        public string Hinh { get; set; }
        public string tenHH { get; set; }
        public double DonGia { get; set; }
        public int Soluong { get; set; }
        public double ThanhTien => Soluong * DonGia;
    }
}
