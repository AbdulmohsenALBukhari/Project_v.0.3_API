using System.ComponentModel.DataAnnotations;

namespace Project_v._0._3.ModelViews
{
    public class AccountModels
    {
        
    }
    public class RegisterModel
    {
        [StringLength(256), Required, EmailAddress]
        public required string Email { get; set; }
        [StringLength(256), Required]
        public required string UserName { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
    }

    public class LoginModel
    {
        [StringLength(256), Required]
        public required string UserName { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
    }
}
