using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerpustakaanPaa.Context;
using PerpustakaanPaa.Models;

namespace PerpustakaanPaa.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/buku")]
    public class BukuController : ControllerBase
    {
        private readonly string _connStr;

        public BukuController(IConfiguration config)
            => _connStr = config.GetConnectionString("DefaultConnection")!;

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var list = new BukuContext(_connStr).ListBuku();
                return Ok(ApiResponse.SuccessList(list, list.Count));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mengambil data buku: {ex.Message}", 500));
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var buku = new BukuContext(_connStr).GetBukuById(id);
                if (buku == null)
                    return NotFound(ApiResponse.Error("Buku tidak ditemukan", 404));
                return Ok(ApiResponse.Success(buku));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mengambil data: {ex.Message}", 500));
            }
        }

        [Authorize(Roles = "admin,petugas")]
        [HttpPost]
        public IActionResult Create([FromBody] Buku buku)
        {
            if (string.IsNullOrWhiteSpace(buku.judul) || string.IsNullOrWhiteSpace(buku.pengarang))
                return BadRequest(ApiResponse.Error("Judul dan pengarang wajib diisi", 400));

            try
            {
                var result = new BukuContext(_connStr).InsertBuku(buku);
                return StatusCode(201, ApiResponse.Success(result, 201));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal menambah buku: {ex.Message}", 500));
            }
        }

        [Authorize(Roles = "admin,petugas")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Buku buku)
        {
            if (string.IsNullOrWhiteSpace(buku.judul) || string.IsNullOrWhiteSpace(buku.pengarang))
                return BadRequest(ApiResponse.Error("Judul dan pengarang wajib diisi", 400));

            try
            {
                bool updated = new BukuContext(_connStr).UpdateBuku(id, buku);
                if (!updated)
                    return NotFound(ApiResponse.Error("Buku tidak ditemukan", 404));
                return Ok(ApiResponse.Success(new { message = "Buku berhasil diperbarui" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal memperbarui buku: {ex.Message}", 500));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool deleted = new BukuContext(_connStr).DeleteBuku(id);
                if (!deleted)
                    return NotFound(ApiResponse.Error("Buku tidak ditemukan", 404));
                return Ok(ApiResponse.Success(new { message = "Buku berhasil dihapus" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal menghapus buku: {ex.Message}", 500));
            }
        }
    }
}