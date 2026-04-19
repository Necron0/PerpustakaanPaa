using Npgsql;
using PerpustakaanPaa.Data;
using PerpustakaanPaa.Models;

namespace PerpustakaanPaa.Context
{
    public class BukuContext
    {
        private readonly string _connStr;

        public BukuContext(string connStr) => _connStr = connStr;

        public List<Buku> ListBuku()
        {
            var list = new List<Buku>();
            const string query = @"
                SELECT id_buku, judul, pengarang, penerbit, tahun_terbit, stok
                FROM buku
                WHERE deleted_at IS NULL
                ORDER BY judul";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapBuku(reader));
            db.Close();
            return list;
        }

        public Buku? GetBukuById(int id)
        {
            const string query = @"
                SELECT id_buku, judul, pengarang, penerbit, tahun_terbit, stok
                FROM buku
                WHERE id_buku = @id AND deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            var buku = reader.Read() ? MapBuku(reader) : null;
            db.Close();
            return buku;
        }

        public Buku InsertBuku(Buku buku)
        {
            const string query = @"
                INSERT INTO buku (judul, pengarang, penerbit, tahun_terbit, stok, created_at, updated_at)
                VALUES (@judul, @pengarang, @penerbit, @tahun, @stok, NOW(), NOW())
                RETURNING id_buku, judul, pengarang, penerbit, tahun_terbit, stok";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@judul", buku.judul);
            cmd.Parameters.AddWithValue("@pengarang", buku.pengarang);
            cmd.Parameters.AddWithValue("@penerbit", (object?)buku.penerbit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tahun", (object?)buku.tahun_terbit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@stok", buku.stok);
            using var reader = cmd.ExecuteReader();
            reader.Read();
            var result = MapBuku(reader);
            db.Close();
            return result;
        }

        public bool UpdateBuku(int id, Buku buku)
        {
            const string query = @"
                UPDATE buku
                SET judul = @judul, pengarang = @pengarang, penerbit = @penerbit,
                    tahun_terbit = @tahun, stok = @stok, updated_at = NOW()
                WHERE id_buku = @id AND deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@judul", buku.judul);
            cmd.Parameters.AddWithValue("@pengarang", buku.pengarang);
            cmd.Parameters.AddWithValue("@penerbit", (object?)buku.penerbit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tahun", (object?)buku.tahun_terbit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@stok", buku.stok);
            int rows = cmd.ExecuteNonQuery();
            db.Close();
            return rows > 0;
        }

        public bool DeleteBuku(int id)
        {
            const string query = @"
                UPDATE buku SET deleted_at = NOW(), updated_at = NOW()
                WHERE id_buku = @id AND deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            db.Close();
            return rows > 0;
        }

        private static Buku MapBuku(NpgsqlDataReader r) => new Buku
        {
            id_buku = r.GetInt32(0),
            judul = r.GetString(1),
            pengarang = r.GetString(2),
            penerbit = r.IsDBNull(3) ? null : r.GetString(3),
            tahun_terbit = r.IsDBNull(4) ? null : r.GetInt16(4),
            stok = r.GetInt32(5)
        };
    }
}