namespace Entities;

public class CommentReaction
{
    public int UserId { get; set; }
    public int CommentId { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
}