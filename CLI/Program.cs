using CLI.UI;
using InMemoryRepositories;
using RepositoryContract;

Console.WriteLine("Starting the CLI ...");
IUserRepository userRepository=new UserInMemoryRepository();
IPostRepository postRepository=new PostInMemoryRepository();
ICommentRepository commentRepository=new CommentInMemoryRepository();

CliApp app=new CliApp(userRepository,commentRepository,postRepository);
await app.StartAsync();