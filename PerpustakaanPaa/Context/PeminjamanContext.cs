using Npgsql;
using PerpustakaanPaa.Data;
using PerpustakaanPaa.Models;

namespace PerpustakaanPaa.Context
{
    public class PeminjamanContext
    {
        private readonly string _connStr;

        public PeminjamanContext(string connStr) => _connStr = connStr;

        public List<Peminjaman> ListPeminjaman(string? status = null)
        {
            var list = new List<Peminjaman>();
            var query = @"
                SELECT p.id_peminjaman, p.id_anggota, p.id_buku,
                       TO_CHAR(p.tanggal_pinjam,'YYYY-MM-DD'),
                       TO_CHAR(p.tanggal_kembali,'YYYY-MM-DD'),
                       p.status, a.nama, b.judul
                FROM peminjaman p
                JOIN anggota a ON p.id_anggota = a.id_anggota
                JOIN buku b    ON p.id_buku    = b.id_buku"
                + (status != null ? " WHERE p.status = @status" : "")
                + " ORDER BY p.created_at DESC";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            if (status != null) cmd.Parameters.AddWithValue("@status", status);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapPeminjaman(reader));
            db.Close();
            return list;
        }

        public List<Peminjaman> ListPeminjamanByAnggota(int idAnggota, string? status = null)
        {
            var list = new List<Peminjaman>();
            var query = @"
                SELECT p.id_peminjaman, p.id_anggota, p.id_buku,
                       TO_CHAR(p.tanggal_pinjam,'YYYY-MM-DD'),
                       TO_CHAR(p.tanggal_kembali,'YYYY-MM-DD'),
                       p.status, a.nama, b.judul
                FROM peminjaman p
                JOIN anggota a ON p.id_anggota = a.id_anggota
                JOIN buku b    ON p.id_buku    = b.id_buku
                WHERE p.id_anggota = @id_anggota"
                + (status != null ? " AND p.status = @status" : "")
                + " ORDER BY p.created_at DESC";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id_anggota", idAnggota);
            if (status != null) cmd.Parameters.AddWithValue("@status", status);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapPeminjaman(reader));
            db.Close();
            return list;
        }
        public Peminjaman? GetById(int id)
        {
            const string query = @"
                SELECT p.id_peminjaman, p.id_anggota, p.id_buku,
                       TO_CHAR(p.tanggal_pinjam,'YYYY-MM-DD'),
                       TO_CHAR(p.tanggal_kembali,'YYYY-MM-DD'),
                       p.status, a.nama, b.judul
                FROM peminjaman p
                JOIN anggota a ON p.id_anggota = a.id_anggota
                JOIN buku b    ON p.id_buku    = b.id_buku
                WHERE p.id_peminjaman = @id";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            var result = reader.Read() ? MapPeminjaman(reader) : null;
            db.Close();
            return result;
        }

        public Peminjaman Pinjam(PinjamRequest req)
        {
            const string updateStok = @"
                UPDATE buku SET stok = stok - 1, updated_at = NOW()
                WHERE id_buku = @id_buku AND stok > 0";

            var db = new SqlDBHelper(_connStr);
            using (var cmd = db.GetCommand(updateStok))
            {
                cmd.Parameters.AddWithValue("@id_buku", req.id_buku);
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0) throw new InvalidOperationException("Stok buku habis atau buku tidak ditemukan");
            }
            db.Close();

            const string insert = @"
                INSERT INTO peminjaman (id_anggota, id_buku, tanggal_pinjam, status, created_at, updated_at)
                VALUES (@id_anggota, @id_buku, CURRENT_DATE, 'dipinjam', NOW(), NOW())
                RETURNING id_peminjaman";

            var db2 = new SqlDBHelper(_connStr);
            using var cmd2 = db2.GetCommand(insert);
            cmd2.Parameters.AddWithValue("@id_anggota", req.id_anggota);
            cmd2.Parameters.AddWithValue("@id_buku", req.id_buku);
            using var reader = cmd2.ExecuteReader();
            reader.Read();
            int newId = reader.GetInt32(0);
            db2.Close();

            return GetById(newId)!;
        }

        public bool Kembalikan(int id, KembaliRequest req)
        {
            const string updatePinjam = @"
                UPDATE peminjaman
                SET tanggal_kembali = @tgl_kembali, status = 'dikembalikan', updated_at = NOW()
                WHERE id_peminjaman = @id AND status = 'dipinjam'";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(updatePinjam);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@tgl_kembali", DateOnly.Parse(req.tanggal_kembali));
            int rows = cmd.ExecuteNonQuery();
            db.Close();
            if (rows == 0) return false;

            var p = GetById(id)!;
            const string updateStok = "UPDATE buku SET stok = stok + 1, updated_at = NOW() WHERE id_buku = @id";
            var db2 = new SqlDBHelper(_connStr);
            using var cmd2 = db2.GetCommand(updateStok);
            cmd2.Parameters.AddWithValue("@id", p.id_buku);
            cmd2.ExecuteNonQuery();
            db2.Close();
            return true;
        }

        public bool Delete(int id)
        {
            const string query = "DELETE FROM peminjaman WHERE id_peminjaman = @id";
            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            db.Close();
            return rows > 0;
        }

        private static Peminjaman MapPeminjaman(NpgsqlDataReader r) => new Peminjaman
        {
            id_peminjaman = r.GetInt32(0),
            id_anggota = r.GetInt32(1),
            id_buku = r.GetInt32(2),
            tanggal_pinjam = r.IsDBNull(3) ? "" : r.GetString(3),
            tanggal_kembali = r.IsDBNull(4) ? null : r.GetString(4),
            status = r.GetString(5),
            nama_anggota = r.IsDBNull(6) ? null : r.GetString(6),
            judul_buku = r.IsDBNull(7) ? null : r.GetString(7)
        };
    }
}