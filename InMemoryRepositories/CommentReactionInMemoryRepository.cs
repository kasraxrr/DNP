using Entities;
using RepositoryContract;

namespace InMemoryRepositories;

public class CommentReactionInMemoryRepository : ICommentReactionRepository
{
    private List<CommentReaction> commentsReactions = new List<CommentReaction>();
    
    public CommentReactionInMemoryRepository()
    {
        CreateDummyData();
    }

    private void CreateDummyData()
    {
        // Adding initial dummy data to make testing easier when the application starts[cite: 1].
        commentsReactions.Add(new CommentReaction { Id = 1, UserId = 1, CommentId = 2, IsLike = true });
        commentsReactions.Add(new CommentReaction { Id = 2, UserId = 2, CommentId = 1, IsLike = true });
        commentsReactions.Add(new CommentReaction { Id = 3, UserId = 3, CommentId = 1, IsLike = false });
    }
    
    public Task<CommentReaction> AddAsync(CommentReaction commentReaction)
    {
        commentReaction.Id = commentsReactions.Any() 
            ? commentsReactions.Max(p => p.Id) + 1
            : 1;
        commentsReactions.Add(commentReaction);
        return Task.FromResult(commentReaction);
    }

    public Task UpdateAsync(CommentReaction commentReaction)
    {
        CommentReaction? existingCommentReaction = commentsReactions.SingleOrDefault(p => p.Id == commentReaction.Id);
        if (existingCommentReaction is null)
        {
            throw new InvalidOperationException(
                $"CommentReaction with ID '{commentReaction.Id}' not found");
        }

        commentsReactions.Remove(existingCommentReaction);
        commentsReactions.Add(commentReaction);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        CommentReaction? commentReactionToRemove = commentsReactions.SingleOrDefault(p => p.Id == id);
        if (commentReactionToRemove is null)
        {
            throw new InvalidOperationException(
                $"CommentReaction with ID '{id}' not found");
        }

        commentsReactions.Remove(commentReactionToRemove);
        return Task.CompletedTask;
    }

    public Task<CommentReaction> GetSingleAsync(int id)
    {
        CommentReaction foundCommentReaction = null;

        foreach (CommentReaction p in commentsReactions)
        {
            if (p.Id == id)
            {
                foundCommentReaction = p;
                break;
            }
        }

        if (foundCommentReaction == null)
        {
            throw new InvalidOperationException("CommentReaction with ID " + id + " not found");
        }
    
        return Task.FromResult(foundCommentReaction);
    }
    
    public IQueryable<CommentReaction> GetManyAsync()
    {
        return commentsReactions.AsQueryable();
    }

}