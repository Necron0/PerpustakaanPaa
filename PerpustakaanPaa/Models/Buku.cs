namespace PerpustakaanPaa.Models
{
    public class Buku
    {
        public int id_buku { get; set; }
        public string judul { get; set; } = string.Empty;
        public string pengarang { get; set; } = string.Empty;
        public string? penerbit { get; set; }
        public int? tahun_terbit { get; set; }
        public int stok { get; set; }
    }
}