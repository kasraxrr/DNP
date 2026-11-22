using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContract;

namespace EfcRepositories;

public class EfcPostRepository : IPostRepository
{
    private readonly AppDbContext ctx;

    public EfcPostRepository(AppDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Post> AddAsync(Post post)
    {
        await ctx.Posts.AddAsync(post);
        await ctx.SaveChangesAsync();
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        if (!await ctx.Posts.AnyAsync(p => p.Id == post.Id))
            throw new NotFoundException($"Post with id {post.Id} not found");

        ctx.Posts.Update(post);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Post? existing = await ctx.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (existing is null)
            throw new NotFoundException($"Post with id {id} not found");

        ctx.Posts.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        Post? post = await ctx.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (post is null)
            throw new NotFoundException($"Post with id {id} not found");
        return post;
    }

    public IQueryable<Post> GetManyAsync()
    {
        return ctx.Posts.AsQueryable();
    }
}