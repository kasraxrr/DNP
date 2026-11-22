using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;

    public CommentsController(ICommentRepository commentRepo, IUserRepository userRepo, IPostRepository postRepo)
    {
        this.commentRepo = commentRepo;
        this.userRepo = userRepo;
        this.postRepo = postRepo;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        User? author = await userRepo.GetManyAsync().SingleOrDefaultAsync(u => u.Id == request.UserId);
        if (author is null)
            return BadRequest($"User with id {request.UserId} does not exist");

        if (!await postRepo.GetManyAsync().AnyAsync(p => p.Id == request.PostId))
            return BadRequest($"Post with id {request.PostId} does not exist");

        Comment comment = new() { Body = request.Body, UserId = request.UserId, PostId = request.PostId };
        Comment created = await commentRepo.AddAsync(comment);

        CommentDto dto = new()
        {
            Id = created.Id,
            Body = created.Body,
            UserId = created.UserId,
            PostId = created.PostId,
            AuthorName = author.Username
        };
        return Created($"/comments/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        Comment existing = await commentRepo.GetSingleAsync(id);
        existing.Body = request.Body;
        await commentRepo.UpdateAsync(existing);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
    {
        CommentDto? dto = await commentRepo.GetManyAsync()
            .Where(c => c.Id == id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                PostId = c.PostId,
                AuthorName = c.User.Username
            })
            .FirstOrDefaultAsync();

        return dto is null ? NotFound($"Comment with id {id} not found") : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> comments = commentRepo.GetManyAsync();

        if (userId is not null)
            comments = comments.Where(c => c.UserId == userId);

        if (postId is not null)
            comments = comments.Where(c => c.PostId == postId);

        if (!string.IsNullOrWhiteSpace(userName))
            comments = comments.Where(c => c.User.Username.ToLower().Contains(userName.ToLower()));

        List<CommentDto> dtos = await comments
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                PostId = c.PostId,
                AuthorName = c.User.Username
            })
            .ToListAsync();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        await commentRepo.DeleteAsync(id);
        return NoContent();
    }
}