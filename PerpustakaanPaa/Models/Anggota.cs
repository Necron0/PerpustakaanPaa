namespace PerpustakaanPaa.Models
{
    public class Anggota
    {
        public int id_anggota { get; set; }
        public string nama { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? alamat { get; set; }
        public int id_role { get; set; } = 2;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}