using DTOs;
using DTOs;

namespace BlazorApp.Services;

public interface IUserService
{
    public Task<UserDto> AddUserAsync(CreateUserDto request);
    
    public Task<UserDto> GetUserAsync(int id);
}