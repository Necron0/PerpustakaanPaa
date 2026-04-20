using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerpustakaanPaa.Context;
using PerpustakaanPaa.Models;
using System.Security.Claims;

namespace PerpustakaanPaa.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/anggota")]
    public class AnggotaController : ControllerBase
    {
        private readonly string _connStr;

        public AnggotaController(IConfiguration config)
            => _connStr = config.GetConnectionString("DefaultConnection")!;

        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue("id_anggota")!);

        private bool IsAdminOrPetugas()
            => User.IsInRole("admin") || User.IsInRole("petugas");

        [Authorize(Roles = "admin,petugas")]
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var list = new AnggotaContext(_connStr).ListAnggota();
                return Ok(ApiResponse.SuccessList(list, list.Count));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mengambil data: {ex.Message}", 500));
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (!IsAdminOrPetugas() && GetCurrentUserId() != id)
                return StatusCode(403, ApiResponse.Error("Anda tidak memiliki akses ke data anggota lain", 403));

            try
            {
                var anggota = new AnggotaContext(_connStr).GetById(id);
                if (anggota == null)
                    return NotFound(ApiResponse.Error("Anggota tidak ditemukan", 404));
                return Ok(ApiResponse.Success(anggota));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mengambil data: {ex.Message}", 500));
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.nama) || string.IsNullOrWhiteSpace(req.email)
                || string.IsNullOrWhiteSpace(req.password))
                return BadRequest(ApiResponse.Error("Nama, email, dan password wajib diisi", 400));

            try
            {
                var anggota = new Anggota
                {
                    nama = req.nama,
                    email = req.email,
                    alamat = req.alamat,
                    id_role = 2 
                };
                var result = new AnggotaContext(_connStr).Insert(anggota, req.password);
                return StatusCode(201, ApiResponse.Success(result, 201));
            }
            catch (Exception ex) when (ex.Message.Contains("unique"))
            {
                return Conflict(ApiResponse.Error("Email sudah terdaftar", 409));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mendaftar: {ex.Message}", 500));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Anggota anggota)
        {
            if (string.IsNullOrWhiteSpace(anggota.nama) || string.IsNullOrWhiteSpace(anggota.email))
                return BadRequest(ApiResponse.Error("Nama dan email wajib diisi", 400));

            if (!IsAdminOrPetugas() && GetCurrentUserId() != id)
                return StatusCode(403, ApiResponse.Error("Anda hanya boleh mengubah data diri sendiri", 403));


            if (!User.IsInRole("admin"))
            {
                var existing = new AnggotaContext(_connStr).GetById(id);
                if (existing == null)
                    return NotFound(ApiResponse.Error("Anggota tidak ditemukan", 404));
                anggota.id_role = existing.id_role; 
            }

            try
            {
                bool updated = new AnggotaContext(_connStr).Update(id, anggota);
                if (!updated)
                    return NotFound(ApiResponse.Error("Anggota tidak ditemukan", 404));
                return Ok(ApiResponse.Success(new { message = "Anggota berhasil diperbarui" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal memperbarui: {ex.Message}", 500));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (GetCurrentUserId() == id)
                return BadRequest(ApiResponse.Error("Admin tidak dapat menghapus akunnya sendiri", 400));

            try
            {
                bool deleted = new AnggotaContext(_connStr).Delete(id);
                if (!deleted)
                    return NotFound(ApiResponse.Error("Anggota tidak ditemukan", 404));
                return Ok(ApiResponse.Success(new { message = "Anggota berhasil dihapus" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal menghapus: {ex.Message}", 500));
            }
        }
    }

    public class RegisterRequest
    {
        public string nama { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string? alamat { get; set; }
    }
}