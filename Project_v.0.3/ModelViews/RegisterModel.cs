using System.ComponentModel.DataAnnotations;

namespace Project_v._0._3.ModelViews
{
    public class RegisterModel
    {
        [StringLength(256), Required, EmailAddress]
        public string Email { get; set; }
        [StringLength(256), Required]
        public string UserName { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }
}
