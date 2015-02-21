using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using AspNet.Identity.AdoNetProvider.WebUI.Infrastructure;
using AspNet.Identity.AdoNetProvider.WebUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace AspNet.Identity.AdoNetProvider.WebUI.Controllers
{
    [NoCache]
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        private ApplicationRoleManager RoleManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>(); }
        }

        [HttpGet]
        public ViewResult Users()
        {
            return View(UserManager.Users);
        }

        [HttpGet]
        public ViewResult CreateUser()
        {
            var roles = RoleManager.Roles;

            return View(new CreateUserModel
            {
                AvailableRoles = roles
            });
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    result = await UserManager.AddToRolesAsync(user.Id, model.SelectedRoles);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Users");
                    }
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }

            model.AvailableRoles = RoleManager.Roles;
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> EditUser(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            var userRoles = await UserManager.GetRolesAsync(id);
            var roles = RoleManager.Roles;

            if (user != null)
            {
                return View(new EditUserModel
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    SelectedRoles = userRoles.ToArray(),
                    AvailableRoles = roles
                });
            }

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<ActionResult> EditUser(EditUserModel model)
        {
            var user = await UserManager.FindByIdAsync(model.Id);

            if (user != null)
            {
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var validEmail = await UserManager.UserValidator.ValidateAsync(user);

                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }

                IdentityResult validPassword = null;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    validPassword = await UserManager.PasswordValidator.ValidateAsync(model.Password);

                    if (validPassword.Succeeded)
                    {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPassword);
                    }
                }

                if (validPassword != null &&
                    ((!validEmail.Succeeded || model.Password == string.Empty || !validPassword.Succeeded)))
                {
                    return View(model);
                }

                var result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var userRoles = await UserManager.GetRolesAsync(user.Id);
                    result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.ToArray());

                    if (!result.Succeeded)
                    {
                        return View(model);
                    }

                    result = await UserManager.AddToRolesAsync(user.Id, model.SelectedRoles);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Users");
                    }
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User was not found.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return RedirectToAction("Users");
            }

            var result = await UserManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                ViewBag.Message = "Ooops...An error occured during deleting the user.";
            }

            return RedirectToAction("Users");
        }

        [HttpGet]
        public ViewResult Roles()
        {
            return View(RoleManager.Roles);
        }

        [HttpGet]
        public ViewResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole([Required] string name)
        {
            if (!ModelState.IsValid)
            {
                return View((object)name);
            }

            var result = await RoleManager.CreateAsync(new ApplicationRole(name));

            if (result.Succeeded)
            {
                return RedirectToAction("Roles");
            }

            AddErrorsFromResult(result);
            return View((object)name);
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}