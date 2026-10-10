using ApiContracts;
using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")] // -> /posts
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo, ICommentRepository commentRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
    }

    // POST /posts
    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        if (!userRepo.GetManyAsync().Any(u => u.Id == request.UserId))
            return BadRequest($"User with id {request.UserId} does not exist");

        Post post = new() { Title = request.Title, Body = request.Body, UserId = request.UserId };
        Post created = await postRepo.AddAsync(post);

        PostDto dto = ToDto(created, GetUserNames());
        return Created($"/posts/{dto.Id}", dto);
    }

    // PUT /posts/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Post existing = await postRepo.GetSingleAsync(id); // throws if not found -> 404
        existing.Title = request.Title;
        existing.Body = request.Body;
        await postRepo.UpdateAsync(existing);
        return NoContent();
    }

    // GET /posts/5
    // GET /posts/5?includeComments=true
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetPost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        Post post = await postRepo.GetSingleAsync(id);
        Dictionary<int, string> userNames = GetUserNames();
        PostDto dto = ToDto(post, userNames);

        if (includeComments)
            dto.Comments = GetCommentsForPost(id, userNames);

        return Ok(dto);
    }

    // GET /posts
    // GET /posts?title=hello&userId=2
    // GET /posts?userName=bob
    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        IQueryable<Post> posts = postRepo.GetManyAsync();
        Dictionary<int, string> userNames = GetUserNames();

        if (!string.IsNullOrWhiteSpace(title))
            posts = posts.Where(p => p.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

        if (userId is not null)
            posts = posts.Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            List<int> matchingUserIds = userNames
                .Where(kv => kv.Value.Contains(userName, StringComparison.OrdinalIgnoreCase))
                .Select(kv => kv.Key)
                .ToList();
            posts = posts.Where(p => matchingUserIds.Contains(p.UserId));
        }

        List<PostDto> dtos = posts.ToList().Select(p => ToDto(p, userNames)).ToList();
        return Ok(dtos);
    }

    // GET /posts/5/comments  (comments "belong to" a post)
    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsOfPost([FromRoute] int id)
    {
        await postRepo.GetSingleAsync(id); // 404 if the post doesn't exist
        return Ok(GetCommentsForPost(id, GetUserNames()));
    }

    // DELETE /posts/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        await postRepo.DeleteAsync(id);
        return NoContent();
    }

    // ---------- helpers ----------

    private Dictionary<int, string> GetUserNames()
    {
        return userRepo.GetManyAsync().ToDictionary(u => u.Id, u => u.Username);
    }

    private List<CommentDto> GetCommentsForPost(int postId, Dictionary<int, string> userNames)
    {
        return commentRepo.GetManyAsync()
            .Where(c => c.PostId == postId)
            .ToList()
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                PostId = c.PostId,
                AuthorName = userNames.GetValueOrDefault(c.UserId)
            })
            .ToList();
    }

    private static PostDto ToDto(Post post, Dictionary<int, string> userNames) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Body = post.Body,
        UserId = post.UserId,
        AuthorName = userNames.GetValueOrDefault(post.UserId)
    };
}