using DTOs;

namespace BlazorApp.Services;

public interface IPostService
{
    public Task<PostDto> AddPostAsync(CreatePostDto request);
    public  Task<List<PostDto>> GetAllPostsAsync();
    public Task<PostDto> GetPostAsync(int id);
}