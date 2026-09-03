using Entities;

namespace RepositoryContract;

public interface IPostReactionRepository
{
    
    Task<PostReaction> AddAsync(PostReaction reaction);
    Task UpdateAsync(PostReaction reaction);
    Task DeleteAsync(int id);
    Task<PostReaction> GetSingleAsync(int id);
    IQueryable<PostReaction> GetManyAsync();
}