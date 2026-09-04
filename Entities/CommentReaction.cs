namespace Entities;

public class CommentReaction
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public int CommentId { get; set; }
    public bool IsLike { get; set; }
    public bool IsDislike { get; set; }
}