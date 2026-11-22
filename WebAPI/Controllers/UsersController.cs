using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (await IsUserNameTakenAsync(request.UserName))
            return Conflict($"Username '{request.UserName}' is already taken");

        User user = new() { Username = request.UserName, Password = request.Password };
        User created = await userRepo.AddAsync(user);

        UserDto dto = new() { Id = created.Id, UserName = created.Username };
        return Created($"/users/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        User existing = await userRepo.GetSingleAsync(id);

        if (existing.Username != request.UserName && await IsUserNameTakenAsync(request.UserName))
            return Conflict($"Username '{request.UserName}' is already taken");

        existing.Username = request.UserName;
        existing.Password = request.Password;
        await userRepo.UpdateAsync(existing);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser([FromRoute] int id)
    {
        User user = await userRepo.GetSingleAsync(id);
        return Ok(new UserDto { Id = user.Id, UserName = user.Username });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string? userName)
    {
        IQueryable<User> users = userRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(userName))
            users = users.Where(u => u.Username.ToLower().Contains(userName.ToLower()));

        List<UserDto> dtos = await users
            .Select(u => new UserDto { Id = u.Id, UserName = u.Username })
            .ToListAsync();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    private Task<bool> IsUserNameTakenAsync(string userName)
    {
        return userRepo.GetManyAsync()
            .AnyAsync(u => u.Username.ToLower() == userName.ToLower());
    }
}