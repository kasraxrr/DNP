using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // -> /auth
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public AuthController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    // POST /auth/login
    [HttpPost("login")]
    public ActionResult<UserDto> Login([FromBody] LoginRequest request)
    {
        // 1) Find the user by username (same case-insensitive rule as UsersController uses)
        User? user = userRepo.GetManyAsync()
            .SingleOrDefault(u => u.Username.Equals(request.UserName, StringComparison.OrdinalIgnoreCase));

        // 2) + 3) Unknown user or wrong password -> 401 Unauthorized.
        // Same message for both, so nobody can find out which usernames exist.
        if (user is null || user.Password != request.Password)
            return Unauthorized("Incorrect username or password");

        // 4) + 5) Convert to UserDto (no password!) and return it
        UserDto dto = new()
        {
            Id = user.Id,
            UserName = user.Username
        };
        return Ok(dto);
    }
}
