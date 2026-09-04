using Entities;
using RepositoryContract;


namespace InMemoryRepositories;

public class PostInMemoryRepository : IPostRepository
{
    private List<Post> posts = new List<Post>();
    
    public PostInMemoryRepository()
    {
        CreateDummyData();
    }

    private void CreateDummyData()
    {
        posts.Add(new Post { Id = 1, Title = "Alexander Parakeet Diet", Body = "What is the best daily mix for a parakeet like Greeny?", UserId = 1 });
        posts.Add(new Post { Id = 2, Title = "Digital Piano Comparison", Body = "Yamaha P-145 B vs Casio CDP-S110 BK. Thoughts?", UserId = 1 });
        posts.Add(new Post { Id = 3, Title = "Software Engineering Exams", Body = "Are the VIA University College exams usually open book?", UserId = 2 });
    }
    
    public Task<Post> AddAsync(Post post)
    {
        post.Id = posts.Any() 
            ? posts.Max(p => p.Id) + 1
            : 1;
        posts.Add(post);
        return Task.FromResult(post);
    }

    public Task UpdateAsync(Post post)
    {
        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);
        return Task.CompletedTask;
    }

    public Task<Post> GetSingleAsync(int id)
    {
        Post foundPost = null;

        foreach (Post p in posts)
        {
            if (p.Id == id)
            {
                foundPost = p;
                break;
            }
        }

        if (foundPost == null)
        {
            throw new InvalidOperationException("Post with ID " + id + " not found");
        }
    
        return Task.FromResult(foundPost);
    }
    
    public IQueryable<Post> GetManyAsync()
    {
        return posts.AsQueryable();
    }


    
}