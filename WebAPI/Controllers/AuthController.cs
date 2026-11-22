using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContract;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
    {
        User? user = await userRepo.GetManyAsync()
            .SingleOrDefaultAsync(u => u.Username.ToLower() == request.UserName.ToLower());

        if (user is null || user.Password != request.Password)
            return Unauthorized("Incorrect username or password");

        return Ok(new UserDto { Id = user.Id, UserName = user.Username });
    }
}
