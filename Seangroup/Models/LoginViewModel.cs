using System.ComponentModel.DataAnnotations;

namespace Seangroup.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }   // <-- добавили это свойство
    }
}
