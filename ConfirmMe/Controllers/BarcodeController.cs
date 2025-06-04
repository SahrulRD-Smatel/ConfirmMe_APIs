using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using ZXing;
using System.Linq;

namespace ConfirmMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class BarcodeController : ControllerBase
    {
        private readonly IBarcodeService _barcodeService;
        private readonly ILogger<BarcodeController> _logger;

        public BarcodeController(IBarcodeService barcodeService, ILogger<BarcodeController> logger)
        {
            _barcodeService = barcodeService;
            _logger = logger;
        }

        [HttpGet("generate")]
        public IActionResult GenerateBarcode([FromQuery] string data,
                                             [FromQuery] int width = 300,
                                             [FromQuery] int height = 150,
                                             [FromQuery] string format = "CODE_128")
        {
            // Validasi data barcode
            if (string.IsNullOrEmpty(data) || data.Length < 1)
            {
                _logger.LogWarning("Invalid barcode data.");
                return BadRequest("Data barcode tidak valid. Pastikan data barcode tidak kosong.");
            }

            // Validasi ukuran barcode
            if (width < 1 || height < 1)
            {
                _logger.LogWarning("Invalid barcode dimensions.");
                return BadRequest("Lebar dan tinggi barcode harus lebih besar dari 0.");
            }

            // Mengecek jabatan pengguna untuk menentukan akses cetak
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            if (userRole == null)
            {
                return Unauthorized("User role is missing.");
            }

            // Menentukan akses cetak berdasarkan jabatan
            if (!HasPrintPermission(userRole))
            {
                return Forbid("Anda tidak memiliki akses untuk mencetak barcode.");
            }

            try
            {
                var barcodeFormat = Enum.TryParse(format, out BarcodeFormat formatEnum)
                    ? formatEnum
                    : BarcodeFormat.CODE_128;

                // Generate barcode dengan data dan parameter ukuran
                var barcode = _barcodeService.GenerateBarcode(data, barcodeFormat, width, height);

                _logger.LogInformation($"Barcode berhasil dibuat untuk data: {data}");

                // Mengembalikan barcode sebagai file PNG
                return File(barcode, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan saat membuat barcode.");
                return StatusCode(500, "Terjadi kesalahan internal saat menghasilkan barcode.");
            }
        }

        // Memeriksa apakah pengguna memiliki izin untuk mencetak berdasarkan jabatan
        private bool HasPrintPermission(string role)
        {
            // Menyesuaikan hak akses berdasarkan role yang diberikan
            switch (role)
            {
                case "Staff":
                    return false; // Staff tidak memiliki izin untuk mencetak barcode
                case "HRD":
                    return true;  // HRD memiliki izin
                case "Manager":
                    return true;  // Manager memiliki izin
                case "Direktur":
                    return true;  // Direktur memiliki izin
                default:
                    return false; // Role tidak dikenali, tidak diberi akses
            }
        }
    }
}
