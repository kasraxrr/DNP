using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContract;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
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

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        User? author = await userRepo.GetManyAsync().SingleOrDefaultAsync(u => u.Id == request.UserId);
        if (author is null)
            return BadRequest($"User with id {request.UserId} does not exist");

        Post post = new() { Title = request.Title, Body = request.Body, UserId = request.UserId };
        Post created = await postRepo.AddAsync(post);

        // Return a DTO, not the entity: Post -> User -> Posts -> ... is a cycle
        PostDto dto = new()
        {
            Id = created.Id,
            Title = created.Title,
            Body = created.Body,
            UserId = created.UserId,
            AuthorName = author.Username
        };
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Post existing = await postRepo.GetSingleAsync(id);
        existing.Title = request.Title;
        existing.Body = request.Body;
        await postRepo.UpdateAsync(existing);
        return NoContent();
    }

    // One SQL query that also loads the author and, if asked, the comments
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetPost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        IQueryable<Post> queryForPost = postRepo.GetManyAsync()
            .Where(p => p.Id == id)
            .Include(p => p.User);

        PostDto? dto;
        if (includeComments)
        {
            dto = await queryForPost
                .Include(p => p.Comments)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    UserId = p.UserId,
                    AuthorName = p.User.Username,
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Body = c.Body,
                        UserId = c.UserId,
                        PostId = c.PostId,
                        AuthorName = c.User.Username
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
        else
        {
            dto = await queryForPost
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    UserId = p.UserId,
                    AuthorName = p.User.Username
                })
                .FirstOrDefaultAsync();
        }

        return dto is null ? NotFound($"Post with id {id} not found") : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        IQueryable<Post> posts = postRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(title))
            posts = posts.Where(p => p.Title.ToLower().Contains(title.ToLower()));

        if (userId is not null)
            posts = posts.Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(userName))
            posts = posts.Where(p => p.User.Username.ToLower().Contains(userName.ToLower()));

        List<PostDto> dtos = await posts
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                AuthorName = p.User.Username
            })
            .ToListAsync();
        return Ok(dtos);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsOfPost([FromRoute] int id)
    {
        if (!await postRepo.GetManyAsync().AnyAsync(p => p.Id == id))
            return NotFound($"Post with id {id} not found");

        List<CommentDto> comments = await commentRepo.GetManyAsync()
            .Where(c => c.PostId == id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                PostId = c.PostId,
                AuthorName = c.User.Username
            })
            .ToListAsync();
        return Ok(comments);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        await postRepo.DeleteAsync(id);
        return NoContent();
    }
}