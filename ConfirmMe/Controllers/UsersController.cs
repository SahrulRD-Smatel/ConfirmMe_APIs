using ConfirmMe.Data;
using ConfirmMe.Dto;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ConfirmMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        public UsersController(IUserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [Authorize(Roles = "HRD, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(Roles = "Staff, Manager, HRD, Direktur, Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User tidak ditemukan." });
            return Ok(user);
        }

        [Authorize(Roles = "HRD, Admin")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [Authorize(Roles = "HRD, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _userService.UpdateUserAsync(id, dto);
            if (!success) return NotFound(new { message = "User tidak ditemukan." });

            return NoContent();
        }

        [Authorize(Roles = "HRD, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound(new { message = "User tidak ditemukan." });

            return NoContent();
        }

        [Authorize(Roles = "Staff, Manager, HRD, Direktur, Admin")]
        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions()
        {
            var positions = await _context.Positions
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

            return Ok(positions);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId claim missing", claims = User.Claims.Select(c => new { c.Type, c.Value }) });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { message = "User tidak ditemukan." });

            return Ok(user);
        }



    }
}
