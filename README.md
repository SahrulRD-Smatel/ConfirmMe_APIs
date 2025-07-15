# ✅ ConfirmMe Backend

![.NET](https://img.shields.io/badge/.NET%208-success?logo=dotnet)
![Docker](https://img.shields.io/badge/containerized-docker-blue)
![CI/CD](https://github.com/SahrulRD-Smatel/ConfirmMe_APIs/actions/workflows/dotnet.yml/badge.svg)
![License](https://img.shields.io/badge/license-MIT-informational)

Sistem informasi persetujuan dokumen berbasis web, mendukung alur persetujuan multi-level, verifikasi QR Code satu kali pakai, dan pencetakan dokumen PDF otomatis setelah semua persetujuan selesai.

---

## 🚀 Fitur Utama

- Autentikasi JWT & role-based authorization
- Approval berjenjang (multi-step flow)
- Approval via QR Code (secure & time-limited)
- Unggah & unduh lampiran dokumen
- Generate surat PDF dengan [QuestPDF](https://www.questpdf.com/)
- RESTful API + dokumentasi Swagger
- Support Docker containerization
- Siap deploy ke Cloud VPS

---

## 🛠️ Teknologi

- **ASP.NET Core 8**
- **Entity Framework Core**
- **PostgreSQL / SQL Server**
- **ZXing.Net** (QR Code)
- **QuestPDF**
- **AutoMapper**
- **Docker**
- **GitHub Actions** (CI/CD)

---

## 📦 Struktur Proyek

```bash
ConfirmMe/
├── Controllers/
├── Dto/
├── Models/
├── Services/
├── Data/
├── Extensions/
├── Middleware/
├── Program.cs
├── Dockerfile
├── nginx.conf
├── .github/workflows/dotnet.yml
