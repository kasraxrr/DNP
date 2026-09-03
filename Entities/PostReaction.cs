namespace Entities;

public class PostReaction
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
}