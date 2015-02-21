using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspNet.Identity.AdoNetProvider.Domain.Entities;

namespace AspNet.Identity.AdoNetProvider.WebUI.Models
{
    public class EditUserModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public IEnumerable<ApplicationRole> AvailableRoles { get; set; }
        public string[] SelectedRoles { get; set; }
    }
}