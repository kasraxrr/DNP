using Entities;
using InMemoryRepositories;
using RepositoryContract;

namespace CLI.UI;

public class CliApp
{
    private IUserRepository userRepository;
    private ICommentRepository commentRepository;
    private IPostRepository postRepository;
    
    public CliApp(IUserRepository userRepository, ICommentRepository commentRepository,
        IPostRepository postRepository)
    {
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
    }
    
    
     public async Task StartAsync()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("== Main Menu ==");
            Console.WriteLine("1. Create new user");
            Console.WriteLine("2. Create new post");
            Console.WriteLine("3. Add comment to post");
            Console.WriteLine("4. View posts overview");
            Console.WriteLine("5. View specific post");
            Console.WriteLine("0. Exit");
            Console.Write("> ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await CreateUserAsync();
                    break;
                case "2":
                    await CreatePostAsync();
                    break;
                case "3":
                    await AddCommentAsync();
                    break;
                case "4":
                    await ViewPostsOverviewAsync();
                    break;
                case "5":
                    await ViewSinglePostAsync();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Unknown option, try again.");
                    break;
            }
        }
    }

    private async Task CreateUserAsync()
    {
        Console.Write("Enter username: ");
        string? username = Console.ReadLine();

        Console.Write("Enter password: ");
        string? password = Console.ReadLine();

        await userRepository.AddAsync( new User { Username = username, Password = password });

        Console.WriteLine($"User created with name of "+username);
    }

    private async Task CreatePostAsync()
    {
        Console.Write("Enter title: ");
        string? title = Console.ReadLine();

        Console.Write("Enter body: ");
        string? body = Console.ReadLine();

        Console.Write("Enter your user ID(Number): ");
        int userId = int.Parse(Console.ReadLine());


      await postRepository.AddAsync(new Post { Title = title, Body = body, UserId = userId });

        Console.WriteLine($"Post created");
    }


    private async Task AddCommentAsync()
    {
        Console.WriteLine("Enter comment:");
        String? comment = Console.ReadLine();
        
        Console.WriteLine("Enter UserId:(Number only)");
        int userId = int.Parse(Console.ReadLine());
        
        Console.WriteLine("Enter postId:(Number only)");
        int postId = int.Parse(Console.ReadLine());

        await commentRepository.AddAsync(new Comment { Body = comment, PostId = postId, UserId = userId });

        Console.WriteLine("Comment Posted");
    }

    private Task ViewPostsOverviewAsync()
    {
        IQueryable<Post> posts = postRepository.GetManyAsync();

        Console.WriteLine();
        Console.WriteLine("== Posts ==");
        foreach (Post post in posts)
        {
            Console.WriteLine($"[{post.Id}] {post.Title}");
        }

        return Task.CompletedTask;
    }

    private async Task ViewSinglePostAsync()
    {
        Console.Write("Enter post ID (Number only): ");

        int postId = int.Parse(Console.ReadLine());



        Post post = await postRepository.GetSingleAsync(postId);



        Console.WriteLine();
        Console.WriteLine($"Title: {post.Title}");
        Console.WriteLine($"Body: {post.Body}");
        
    }

}