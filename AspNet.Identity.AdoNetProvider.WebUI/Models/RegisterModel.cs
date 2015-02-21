using System.ComponentModel.DataAnnotations;

namespace AspNet.Identity.AdoNetProvider.WebUI.Models
{
    public class RegisterModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide an email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please set a password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Please confirm your password correctly")]
        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }
    }
}