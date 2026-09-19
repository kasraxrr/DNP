using CLI.UI;
using FileRepositories;
using RepositoryContract;

Console.WriteLine("Starting the CLI ...");
IUserRepository userRepository=new UserFileRepositories();
IPostRepository postRepository=new PostFileRepository();
ICommentRepository commentRepository=new CommentFileRepository();

CliApp app=new CliApp(userRepository,commentRepository,postRepository);
await app.StartAsync();