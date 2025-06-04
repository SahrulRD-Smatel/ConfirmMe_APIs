using ConfirmMe.Data;
using ConfirmMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConfirmMe.Controllers
{
    [Authorize]  // Pastikan hanya pengguna yang terotorisasi yang bisa mengakses controller ini
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApprovalTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/approvaltypes
        // Hanya Manager dan Direktur yang bisa melihat daftar ApprovalTypes
        [HttpGet]
        [Authorize(Roles = "Staff, Manager, HRD, Direktur")]
        public async Task<ActionResult<IEnumerable<ApprovalType>>> GetAll()
        {
            // Mengambil semua jenis approval dari database
            var approvalTypes = await _context.ApprovalTypes.ToListAsync();
            return Ok(approvalTypes);
        }
    }
}
