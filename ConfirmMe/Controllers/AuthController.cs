using ConfirmMe.Dto;
using ConfirmMe.Models;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConfirmMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        // 🔐 LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid login data." });

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { Message = "Invalid credentials or email not confirmed." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid email or password." });

            var token = GenerateJwtToken(user);

            // Kirim token melalui cookie dengan HttpOnly, Secure, dan SameSite
            //Response.Cookies.Append("token", token, new CookieOptions
            //{
            //    HttpOnly = true,       // Token hanya bisa diakses oleh server, bukan JavaScript
            //    Secure = true,         // True Cookie hanya dikirim melalui HTTPS
            //    SameSite = SameSiteMode.None, // Mencegah CSRF, none biar bisa lintas domain kalo mau domain sama ubah lagi ke Strict
            //    Expires = DateTime.UtcNow.AddHours(2) // buat waktu kedaluwarsa token
            //});

            //localstorage soalnya buat development
             

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                return Ok(new
                {
                    Message = "Login successful",
                    Token = token,
                    Role = roles.FirstOrDefault(),  // Mengambil role pertama
                    UserName = user.UserName,
                    UserId = user.Id
                });
            }
            else
            {
                return Unauthorized(new { Message = "User has no roles assigned." });
            }
        }


        // 🧾 REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid registration data." });

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return Conflict(new { Message = "Email is already registered." });

            var newUser = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PositionId = registerDto.PositionId,
                Role = "Staff",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded)
                return BadRequest(new { Message = "Registration failed.", Errors = result.Errors.Select(e => e.Description) });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var link = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = newUser.Id, token }, Request.Scheme);
            await _emailService.SendEmailAsync(newUser.Email, "Confirm your email", $"Click to confirm your account: {link}");

            return Ok(new { Message = "User registered successfully. Confirmation email sent." });
        }

        // ✅ EMAIL VERIFICATION
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? Ok("Email berhasil dikonfirmasi.") : BadRequest("Token tidak valid atau sudah kadaluarsa.");
        }

        [HttpPost("send-email-confirmation")]
        public async Task<IActionResult> SendEmailConfirmation([FromBody] EmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound(new { Message = "User not found." });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", $"Klik link ini untuk konfirmasi: {confirmationLink}");

            return Ok(new { Message = "Email verifikasi telah dikirim." });
        }

        // 🔁 RESET PASSWORD
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User tidak ditemukan.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(nameof(ResetPassword), "Auth", new { token, email = user.Email }, Request.Scheme);
            await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Klik link ini untuk reset password: {resetLink}");

            return Ok(new { Message = "Link reset password telah dikirim ke email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User tidak ditemukan.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            return result.Succeeded ? Ok("Password berhasil direset.") : BadRequest(result.Errors);
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId claim missing" });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Unauthorized(new { message = "User not found" });

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return Ok(new
            {
                user.FullName,
                Role = role
            });
        }




        // JWT GENERATOR
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("role", user.Role ?? "User"),
                new Claim("position", user.PositionId.ToString())
            };

            var key = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
