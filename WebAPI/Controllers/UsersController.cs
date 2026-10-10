using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // -> /users
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    //Post/users
    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (IsUserNameTaken(request.UserName))
            return Conflict($"Username '{request.UserName}' is already taken");

        User user = new() { Username = request.UserName, Password = request.Password };
        User created = await userRepo.AddAsync(user);

        UserDto dto = ToDto(created);
        return Created($"/users/{dto.Id}", dto);
    }

    // PUT /users/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        User existing = await userRepo.GetSingleAsync(id); // throws if not found -> 404
 
        if (existing.Username != request.UserName && IsUserNameTaken(request.UserName))
            return Conflict($"Username '{request.UserName}' is already taken");
 
        existing.Username = request.UserName;
        existing.Password = request.Password;
        await userRepo.UpdateAsync(existing);
        return NoContent();
    }

    // GET /users/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser([FromRoute] int id)
    {
        User user = await userRepo.GetSingleAsync(id);
        return Ok(ToDto(user));
    }

    // GET /users            -> all users
    // GET /users?userName=bo -> users whose name contains "bo"
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers([FromQuery] string? userName)
    {
        IQueryable<User> users = userRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(userName))
            users = users.Where(u => u.Username.Contains(userName, StringComparison.OrdinalIgnoreCase));

        List<UserDto> dtos = users.ToList().Select(ToDto).ToList();
        return Ok(dtos);
    }

    // DELETE /users/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    private bool IsUserNameTaken(string userName)
    {
        return userRepo.GetManyAsync()
            .Any(u => u.Username.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        UserName = user.Username
    };
}