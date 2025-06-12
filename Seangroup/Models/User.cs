using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Seangroup.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string PasswordHash { get; set; } // Тут должны быть хешированные пароли

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
