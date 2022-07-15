namespace Server.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public bool isAccepted { get; set; }

        // null if contact isn't accepted
        // name of contact showed at user from
        public string? NameAtUserFrom { get; set; }

        // name of contact showed at user to
        public string? NameAtUserTo { get; set; }

        public User UserFrom { get; set; }
        public int UserFromId { get; set; }
        public User UserTo { get; set; }
        public int UserToId { get; set; }
    }
}