using Entities;

namespace RepositoryContract;

public interface ICommentReactionRepository
{
    Task<CommentReaction> AddAsync(CommentReaction reaction);
    Task UpdateAsync(CommentReaction reaction);
    Task DeleteAsync(int id);
    Task<CommentReaction> GetSingleAsync(int id);
    IQueryable<CommentReaction> GetManyAsync();
}