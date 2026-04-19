using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerpustakaanPaa.Context;
using PerpustakaanPaa.Models;

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

        // GET api/anggota
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

        // GET api/anggota/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
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

        // POST api/anggota  (register anggota baru — public)
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nama) || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(ApiResponse.Error("Nama, email, dan password wajib diisi", 400));

            try
            {
                var anggota = new Anggota
                {
                    nama = dto.Nama,
                    email = dto.Email,
                    alamat = dto.Alamat,
                    id_role = 2
                };
                var result = new AnggotaContext(_connStr).Insert(anggota, dto.Password);
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

        // PUT api/anggota/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Anggota anggota)
        {
            if (string.IsNullOrWhiteSpace(anggota.nama) || string.IsNullOrWhiteSpace(anggota.email))
                return BadRequest(ApiResponse.Error("Nama dan email wajib diisi", 400));

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

        // DELETE api/anggota/{id}
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
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

    public class RegisterDto
    {
        public string Nama { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Alamat { get; set; }
    }
}