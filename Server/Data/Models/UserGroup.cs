namespace Server.Models
{
  public class UserGroup
  {
    public Group Group { get; set; }
    public int GroupId { get; set; }
    public int Id { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
  }
}