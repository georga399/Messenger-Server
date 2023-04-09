namespace Messenger.Models;
public class Room
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public User? Admin { get; set; }
    public List<User>? AllUsers{get; set;}
    public ICollection<Message>? Messages { get; set; }
}