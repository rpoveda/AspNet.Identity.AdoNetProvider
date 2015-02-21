using System.ComponentModel.DataAnnotations;

namespace AspNet.Identity.AdoNetProvider.WebUI.Models
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide an email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please set a password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}