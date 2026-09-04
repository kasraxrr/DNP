using Entities;
using RepositoryContract;


namespace InMemoryRepositories;

public class CommentInMemoryRepository : ICommentRepository
{
   
    private List<Comment> comments = new List<Comment>();
    
    public CommentInMemoryRepository()
    {
        CreateDummyData();
    }

    private void CreateDummyData()
    {
        // Adding initial dummy data to make testing easier when the application starts[cite: 1].
        comments.Add(new Comment { Id = 1, Body = "This is really helpful, thanks!", UserId = 2, PostId = 1 });
        comments.Add(new Comment { Id = 2, Body = "I totally agree with this.", UserId = 3, PostId = 1 });
        comments.Add(new Comment { Id = 3, Body = "Could you provide more details?", UserId = 1, PostId = 2 });
    }
    
    public Task<Comment> AddAsync(Comment comment)
    {
        comment.Id = comments.Any() 
            ? comments.Max(p => p.Id) + 1
            : 1;
        comments.Add(comment);
        return Task.FromResult(comment);
    }

    public Task UpdateAsync(Comment comment)
    {
        Comment? existingComment = comments.SingleOrDefault(p => p.Id == comment.Id);
        if (existingComment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{comment.Id}' not found");
        }

        comments.Remove(existingComment);
        comments.Add(comment);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Comment? commentToRemove = comments.SingleOrDefault(p => p.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);
        return Task.CompletedTask;
    }

    public Task<Comment> GetSingleAsync(int id)
    {
        Comment foundComment = null;

        foreach (Comment p in comments)
        {
            if (p.Id == id)
            {
                foundComment = p;
                break;
            }
        }

        if (foundComment == null)
        {
            throw new InvalidOperationException("Comment with ID " + id + " not found");
        }
    
        return Task.FromResult(foundComment);
    }
    
    public IQueryable<Comment> GetManyAsync()
    {
        return comments.AsQueryable();
    }


       


}