namespace Server.Models
{
  public class UserGroup
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; }
  }
}