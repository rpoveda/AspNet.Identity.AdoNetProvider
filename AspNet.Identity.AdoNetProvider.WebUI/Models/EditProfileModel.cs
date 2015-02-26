using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspNet.Identity.AdoNetProvider.WebUI.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace AspNet.Identity.AdoNetProvider.WebUI.Models
{
    public class EditProfileModel
    {
        public List<UserLoginInfo> ExternalProviders { get; set; }
        public List<AuthenticationDescription> OtherProviders { get; set; }
        public bool UserHasPassword { get; set; }
        public string Message { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [RequiredIf("CurrentPassword != string.empty", "CurrentPassword", ErrorMessage = "Please set a new password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please confirm your new password")]
        [Compare("NewPassword", ErrorMessage = "Please confirm your new password correctly")]
        [DataType(DataType.Password)]
        public string NewPasswordConfirmation { get; set; }
    }
}