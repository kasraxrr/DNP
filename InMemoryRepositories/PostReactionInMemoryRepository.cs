using Entities;
using RepositoryContract;

namespace InMemoryRepositories;

public class PostReactionInMemoryRepository : IPostReactionRepository
{
    private List<PostReaction> postReactions = new List<PostReaction>();
    
    public PostReactionInMemoryRepository()
    {
        CreateDummyData();
    }

    private void CreateDummyData()
    {
        // Adding initial dummy data to make testing easier when the application starts[cite: 1].
        postReactions.Add(new PostReaction { Id = 1, UserId = 1, PostId = 2, IsLike = true });
        postReactions.Add(new PostReaction { Id = 2, UserId = 2, PostId = 1, IsLike = true });
        postReactions.Add(new PostReaction { Id = 3, UserId = 3, PostId = 1, IsLike = false });
    }
    
    public Task<PostReaction> AddAsync(PostReaction postReaction)
    {
        postReaction.Id = postReactions.Any() 
            ? postReactions.Max(p => p.Id) + 1
            : 1;
        postReactions.Add(postReaction);
        return Task.FromResult(postReaction);
    }

    public Task UpdateAsync(PostReaction postReaction)
    {
        PostReaction? existingPostReaction = postReactions.SingleOrDefault(p => p.Id == postReaction.Id);
        if (existingPostReaction is null)
        {
            throw new InvalidOperationException(
                $"PostReaction with ID '{postReaction.Id}' not found");
        }

        postReactions.Remove(existingPostReaction);
        postReactions.Add(postReaction);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        PostReaction? postReactionToRemove = postReactions.SingleOrDefault(p => p.Id == id);
        if (postReactionToRemove is null)
        {
            throw new InvalidOperationException(
                $"PostReaction with ID '{id}' not found");
        }

        postReactions.Remove(postReactionToRemove);
        return Task.CompletedTask;
    }

    public Task<PostReaction> GetSingleAsync(int id)
    {
        PostReaction foundPostReaction = null;

        foreach (PostReaction p in postReactions)
        {
            if (p.Id == id)
            {
                foundPostReaction = p;
                break;
            }
        }

        if (foundPostReaction == null)
        {
            throw new InvalidOperationException("PostReaction with ID " + id + " not found");
        }
    
        return Task.FromResult(foundPostReaction);
    }
    
    public IQueryable<PostReaction> GetManyAsync()
    {
        return postReactions.AsQueryable();
    }
}