using ApiContracts;
using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // -> /comments
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

    // POST /comments
    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        if (!userRepo.GetManyAsync().Any(u => u.Id == request.UserId))
            return BadRequest($"User with id {request.UserId} does not exist");

        if (!postRepo.GetManyAsync().Any(p => p.Id == request.PostId))
            return BadRequest($"Post with id {request.PostId} does not exist");

        Comment comment = new() { Body = request.Body, UserId = request.UserId, PostId = request.PostId };
        Comment created = await commentRepo.AddAsync(comment);

        CommentDto dto = ToDto(created, GetUserNames());
        return Created($"/comments/{dto.Id}", dto);
    }

    // PUT /comments/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        Comment existing = await commentRepo.GetSingleAsync(id); // throws if not found -> 404
        existing.Body = request.Body;   
        await commentRepo.UpdateAsync(existing);
        return NoContent();
    }

    // GET /comments/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        return Ok(ToDto(comment, GetUserNames()));
    }

    // GET /comments
    // GET /comments?postId=3
    // GET /comments?userId=2
    // GET /comments?userName=bob
    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> comments = commentRepo.GetManyAsync();
        Dictionary<int, string> userNames = GetUserNames();

        if (userId is not null)
            comments = comments.Where(c => c.UserId == userId);

        if (postId is not null)
            comments = comments.Where(c => c.PostId == postId);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            List<int> matchingUserIds = userNames
                .Where(kv => kv.Value.Contains(userName, StringComparison.OrdinalIgnoreCase))
                .Select(kv => kv.Key)
                .ToList();
            comments = comments.Where(c => matchingUserIds.Contains(c.UserId));
        }

        List<CommentDto> dtos = comments.ToList().Select(c => ToDto(c, userNames)).ToList();
        return Ok(dtos);
    }

    // DELETE /comments/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        await commentRepo.DeleteAsync(id);
        return NoContent();
    }

    // ---------- helpers ----------

    private Dictionary<int, string> GetUserNames()
    {
        return userRepo.GetManyAsync().ToDictionary(u => u.Id, u => u.Username);
    }

    private static CommentDto ToDto(Comment comment, Dictionary<int, string> userNames) => new()
    {
        Id = comment.Id,
        Body = comment.Body,
        UserId = comment.UserId,
        PostId = comment.PostId,
        AuthorName = userNames.GetValueOrDefault(comment.UserId)
    };
}