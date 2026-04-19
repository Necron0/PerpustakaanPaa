namespace PerpustakaanPaa.Models
{
    public class Peminjaman
    {
        public int id_peminjaman { get; set; }
        public int id_anggota { get; set; }
        public int id_buku { get; set; }
        public string tanggal_pinjam { get; set; } = string.Empty;
        public string? tanggal_kembali { get; set; }
        public string status { get; set; } = "dipinjam";

        // Untuk response gabungan (join)
        public string? nama_anggota { get; set; }
        public string? judul_buku { get; set; }
    }

    public class PinjamRequest
    {
        public int id_anggota { get; set; }
        public int id_buku { get; set; }
    }

    public class KembaliRequest
    {
        public string tanggal_kembali { get; set; } = string.Empty;
    }
}