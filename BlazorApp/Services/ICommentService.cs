using DTOs;

namespace BlazorApp.Services;

public interface ICommentService
{
    public Task<CommentDto> AddCommentAsync(CreateCommentDto request);
    public Task<List<CommentDto>> GetCommentsFromPostAsync(int postId);
}