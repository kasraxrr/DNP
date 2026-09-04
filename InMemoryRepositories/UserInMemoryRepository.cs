using Entities;
using RepositoryContract;

namespace InMemoryRepositories;

public class UserInMemoryRepository : IUserRepository
{
    private List<User> users = new List<User>();
    
    public UserInMemoryRepository()
    {
        CreateDummyData();
    }

    private void CreateDummyData()
    {
        users.Add(new User { Id = 1, Username = "Kasra", Password = "password123" });
        users.Add(new User { Id = 2, Username = "Devin", Password = "password123" });
        users.Add(new User { Id = 3, Username = "Hussein", Password = "password123" });
    }
    
    public Task<User> AddAsync(User user)
    {
        user.Id = users.Any() 
            ? users.Max(u => u.Id) + 1
            : 1;
        users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        return Task.CompletedTask;
    }

    public Task<User> GetSingleAsync(int id)
    {
        User foundUser = null;

        foreach (User u in users)
        {
            if (u.Id == id)
            {
                foundUser = u;
                break;
            }
        }

        if (foundUser == null)
        {
            throw new InvalidOperationException("User with ID " + id + " not found");
        }
    
        return Task.FromResult(foundUser);
    }
    
    public IQueryable<User> GetManyAsync()
    {
        return users.AsQueryable();
    }
}