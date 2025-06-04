using ConfirmMe.Dto;
using ConfirmMe.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfirmMe.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<bool> UpdateUserAsync(string id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(string id);
    }
}
