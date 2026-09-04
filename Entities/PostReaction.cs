namespace Entities;

public class PostReaction
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public bool IsLike { get; set; }
    public bool IsDislike { get; set; }
    public int Id { get; set; }
}