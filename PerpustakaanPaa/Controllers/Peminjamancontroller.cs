using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerpustakaanPaa.Context;
using PerpustakaanPaa.Models;
using System.Security.Claims;

namespace Perpustakaan.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/peminjaman")]
    public class PeminjamanController : ControllerBase
    {
        private readonly string _connStr;

        public PeminjamanController(IConfiguration config)
            => _connStr = config.GetConnectionString("DefaultConnection")!;


        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue("id_anggota")!);

        private bool IsAdminOrPetugas()
            => User.IsInRole("admin") || User.IsInRole("petugas");


        [HttpGet]
        public IActionResult GetAll([FromQuery] string? status)
        {
            try
            {
                List<Peminjaman> list;
                if (IsAdminOrPetugas())
                {
                    list = new PeminjamanContext(_connStr).ListPeminjaman(status);
                }
                else
                {
                    list = new PeminjamanContext(_connStr)
                               .ListPeminjamanByAnggota(GetCurrentUserId(), status);
                }
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
            try
            {
                var p = new PeminjamanContext(_connStr).GetById(id);
                if (p == null)
                    return NotFound(ApiResponse.Error("Data peminjaman tidak ditemukan", 404));

                if (!IsAdminOrPetugas() && p.id_anggota != GetCurrentUserId())
                    return StatusCode(403, ApiResponse.Error("Anda tidak memiliki akses ke data peminjaman ini", 403));

                return Ok(ApiResponse.Success(p));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mengambil data: {ex.Message}", 500));
            }
        }

        public IActionResult Pinjam([FromBody] PinjamRequest req)
        {
            if (req.id_anggota <= 0 || req.id_buku <= 0)
                return BadRequest(ApiResponse.Error("id_anggota dan id_buku wajib diisi", 400));

            if (!IsAdminOrPetugas())
            {
                int currentId = GetCurrentUserId();
                if (req.id_anggota != currentId)
                    return StatusCode(403, ApiResponse.Error("Anda hanya boleh meminjam buku atas nama diri sendiri", 403));
            }

            try
            {
                var result = new PeminjamanContext(_connStr).Pinjam(req);
                return StatusCode(201, ApiResponse.Success(result, 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Error(ex.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal meminjam buku: {ex.Message}", 500));
            }
        }

        [HttpPut("{id}/kembalikan")]
        public IActionResult Kembalikan(int id, [FromBody] KembaliRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.tanggal_kembali))
                return BadRequest(ApiResponse.Error("tanggal_kembali wajib diisi", 400));

            try
            {
                var p = new PeminjamanContext(_connStr).GetById(id);
                if (p == null)
                    return NotFound(ApiResponse.Error("Data peminjaman tidak ditemukan", 404));

                if (!IsAdminOrPetugas() && p.id_anggota != GetCurrentUserId())
                    return StatusCode(403, ApiResponse.Error("Anda hanya boleh mengembalikan buku yang Anda pinjam sendiri", 403));

                bool ok = new PeminjamanContext(_connStr).Kembalikan(id, req);
                if (!ok)
                    return BadRequest(ApiResponse.Error("Peminjaman sudah dikembalikan sebelumnya", 400));

                return Ok(ApiResponse.Success(new { message = "Buku berhasil dikembalikan" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal mengembalikan buku: {ex.Message}", 500));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                bool deleted = new PeminjamanContext(_connStr).Delete(id);
                if (!deleted)
                    return NotFound(ApiResponse.Error("Data peminjaman tidak ditemukan", 404));
                return Ok(ApiResponse.Success(new { message = "Data peminjaman berhasil dihapus" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal menghapus: {ex.Message}", 500));
            }
        }
    }
}