namespace Seangroup.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } // Например: "Admin", "User"
        public List<User> Users { get; set; } = new();
    }
}
