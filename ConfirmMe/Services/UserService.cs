using AutoMapper;
using ConfirmMe.Dto;
using ConfirmMe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConfirmMe.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.Include(u => u.Position).ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager.Users.Include(u => u.Position).FirstOrDefaultAsync(u => u.Id == id);
            return _mapper.Map<UserDto>(user);
        }


        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                Role = dto.Role,
                PositionId = dto.PositionId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
                PhoneNumber = dto.PhoneNumber
            };

            // Cek apakah role valid
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                throw new Exception($"Role '{dto.Role}' tidak ditemukan di sistem.");

            // Buat user
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign role
            var addRoleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!addRoleResult.Succeeded)
                throw new Exception("Gagal assign role: " + string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));

            return _mapper.Map<UserDto>(user);
        }



        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.Role = dto.Role;
            user.PositionId = dto.PositionId;
            user.IsActive = dto.IsActive;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
