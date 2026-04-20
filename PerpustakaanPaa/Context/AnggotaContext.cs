using Npgsql;
using PerpustakaanPaa.Data;
using PerpustakaanPaa.Models;

namespace PerpustakaanPaa.Context
{
    public class AnggotaContext
    {
        private readonly string _connStr;

        public AnggotaContext(string connStr) => _connStr = connStr;

        public List<Anggota> ListAnggota()
        {
            var list = new List<Anggota>();
            const string query = @"
                SELECT a.id_anggota, a.nama, a.email, a.alamat, a.id_role
                FROM anggota a
                WHERE a.deleted_at IS NULL
                ORDER BY a.nama";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapAnggota(reader));
            db.Close();
            return list;
        }
        public Anggota? GetById(int id)
        {
            const string query = @"
                SELECT id_anggota, nama, email, alamat, id_role
                FROM anggota
                WHERE id_anggota = @id AND deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            var result = reader.Read() ? MapAnggota(reader) : null;
            db.Close();
            return result;
        }

        public (Anggota? anggota, string roleName) Authenticate(string email, string password)
        {
            const string query = @"
                SELECT a.id_anggota, a.nama, a.email, a.alamat, a.id_role, r.nama_role
                FROM anggota a
                JOIN role_anggota r ON a.id_role = r.id_role
                WHERE a.email = @email AND a.password = @password AND a.deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password", password);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) { db.Close(); return (null, ""); }

            var anggota = MapAnggota(reader);
            var role = reader.IsDBNull(5) ? "" : reader.GetString(5);
            db.Close();
            return (anggota, role);
        }

        public Anggota Insert(Anggota a, string password)
        {
            const string query = @"
                INSERT INTO anggota (nama, email, password, alamat, id_role, created_at, updated_at)
                VALUES (@nama, @email, @password, @alamat, @role, NOW(), NOW())
                RETURNING id_anggota, nama, email, alamat, id_role";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@nama", a.nama);
            cmd.Parameters.AddWithValue("@email", a.email);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@alamat", (object?)a.alamat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", a.id_role);
            using var reader = cmd.ExecuteReader();
            reader.Read();
            var result = MapAnggota(reader);
            db.Close();
            return result;
        }

        public bool Update(int id, Anggota a)
        {
            const string query = @"
                UPDATE anggota
                SET nama = @nama, email = @email, alamat = @alamat, id_role = @role, updated_at = NOW()
                WHERE id_anggota = @id AND deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nama", a.nama);
            cmd.Parameters.AddWithValue("@email", a.email);
            cmd.Parameters.AddWithValue("@alamat", (object?)a.alamat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", a.id_role);
            int rows = cmd.ExecuteNonQuery();
            db.Close();
            return rows > 0;
        }

        public bool Delete(int id)
        {
            const string query = @"
                UPDATE anggota SET deleted_at = NOW(), updated_at = NOW()
                WHERE id_anggota = @id AND deleted_at IS NULL";

            var db = new SqlDBHelper(_connStr);
            using var cmd = db.GetCommand(query);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            db.Close();
            return rows > 0;
        }
        private static Anggota MapAnggota(NpgsqlDataReader r) => new Anggota
        {
            id_anggota = r.GetInt32(0),
            nama = r.GetString(1),
            email = r.GetString(2),
            alamat = r.IsDBNull(3) ? null : r.GetString(3),
            id_role = r.GetInt32(4)
        };
    }
}