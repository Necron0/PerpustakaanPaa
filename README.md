# 📚 Perpustakaan PAA — REST API

REST API untuk sistem manajemen perpustakaan digital. API ini memungkinkan pengelolaan data anggota, buku, dan transaksi peminjaman secara lengkap dengan autentikasi berbasis JWT dan kontrol akses berbasis peran (role-based access control).

---

## 🛠️ Teknologi yang Digunakan

| Komponen | Teknologi |
|---|---|
| Bahasa | C# |
| Framework | ASP.NET Core 8.0 |
| Database | PostgreSQL |
| ORM / DB Driver | Npgsql |
| Autentikasi | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) |
| Dokumentasi API | Swagger / Swashbuckle |

---

## ⚙️ Langkah Instalasi dan Cara Menjalankan Project

### Prasyarat

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL (versi 13 ke atas direkomendasikan)

### 1. Clone Repository

```bash
git clone <url-repository>
cd PerpustakaanPaa
```

### 2. Konfigurasi Database

Buka file `PerpustakaanPaa/appsettings.json` dan sesuaikan nilai connection string dengan konfigurasi PostgreSQL lokal Anda:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=Perpustakaan_PAA;Username=postgres;Password=<password-anda>"
}
```

### 3. Import Database

Lihat bagian [Cara Import Database](#cara-import-database) di bawah.

### 4. Jalankan Project

```bash
cd PerpustakaanPaa
dotnet run
```

API akan berjalan di:
- HTTP : `http://localhost:5000`
- HTTPS: `https://localhost:7000`

Dokumentasi Swagger dapat diakses di: `http://localhost:5000/swagger`

---

## 🗄️ Cara Import Database

1. Pastikan PostgreSQL sudah berjalan dan Anda memiliki akses ke `psql` atau pgAdmin.
2. Buat database baru bernama `Perpustakaan_PAA`:

```sql
CREATE DATABASE "Perpustakaan_PAA";
```

3. Import file SQL:

```bash
psql -U postgres -d Perpustakaan_PAA -f database.sql
```

Atau melalui pgAdmin: klik kanan database → **Query Tool** → buka `database.sql` → eksekusi.

---

## 📋 Daftar Endpoint

### Auth

| Method | URL | Keterangan | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Login dan mendapatkan token JWT | Tidak |

### Anggota

| Method | URL | Keterangan | Auth / Role |
|---|---|---|---|
| GET | `/api/anggota` | Ambil semua data anggota | Admin, Petugas |
| GET | `/api/anggota/{id}` | Ambil data anggota berdasarkan ID | Admin, Petugas, atau anggota itu sendiri |
| POST | `/api/anggota` | Registrasi anggota baru | Tidak |
| PUT | `/api/anggota/{id}` | Perbarui data anggota | Admin, Petugas, atau anggota itu sendiri |
| DELETE | `/api/anggota/{id}` | Hapus anggota (soft delete) | Admin |

### Buku

| Method | URL | Keterangan | Auth / Role |
|---|---|---|---|
| GET | `/api/buku` | Ambil semua data buku | Login |
| GET | `/api/buku/{id}` | Ambil detail buku berdasarkan ID | Login |
| POST | `/api/buku` | Tambah buku baru | Admin, Petugas |
| PUT | `/api/buku/{id}` | Perbarui data buku | Admin, Petugas |
| DELETE | `/api/buku/{id}` | Hapus buku (soft delete) | Admin |

### Peminjaman

| Method | URL | Keterangan | Auth / Role |
|---|---|---|---|
| GET | `/api/peminjaman` | Ambil semua data peminjaman (admin/petugas: semua; anggota: milik sendiri) | Login |
| GET | `/api/peminjaman/{id}` | Ambil detail peminjaman berdasarkan ID | Login |
| POST | `/api/peminjaman` | Buat transaksi peminjaman baru | Login |
| PUT | `/api/peminjaman/{id}/kembalikan` | Proses pengembalian buku | Login |
| DELETE | `/api/peminjaman/{id}` | Hapus data peminjaman | Admin |

---

## 📦 Format Response

Semua endpoint menggunakan format JSON yang konsisten.

**Sukses (single data):**
```json
{
  "status": 200,
  "data": { ... }
}
```

**Sukses (list data):**
```json
{
  "status": 200,
  "meta": { "total": 5 },
  "data": [ ... ]
}
```

**Error:**
```json
{
  "status": 404,
  "message": "Buku tidak ditemukan"
}
```

---

## 🔐 Autentikasi

API menggunakan JWT Bearer Token. Langkah penggunaan:

1. Lakukan login melalui `POST /api/auth/login` dengan body:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

2. Salin nilai `token` dari response.
3. Sertakan token pada header setiap request yang memerlukan autentikasi:
```
Authorization: Bearer <token>
```

---

## 🎥 Video Presentasi

> Link video presentasi: _[tambahkan link YouTube di sini]_
