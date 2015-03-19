using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using AspNet.Identity.AdoNetProvider.WebUI.Infrastructure;
using AspNet.Identity.AdoNetProvider.WebUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace AspNet.Identity.AdoNetProvider.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private const string XsrfKey = "XsrfId";
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        [HttpGet]
        public ViewResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);

            switch (result)
            {
                case SignInStatus.LockedOut:
                    ViewBag.Message = "Your account is locked out. Please contact administrator.";
                    return View(model);
                case SignInStatus.RequiresVerification:
                    ViewBag.Message = "You need to verify your account before proceeding.";
                    return View(model);
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.Failure:
                    ViewBag.Message = "Please check your username or password.";
                    return View(model);
                default:
                    ViewBag.Message = "Invalid login attempt.";
                    return View(model);
            }
        }

        [HttpGet]
        public ViewResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.UserName);
            IdentityResult result;

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.UserName
                };

                result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                    if (Request.Url != null)
                    {
                        ViewBag.Message = "A confirmation email is going to arrive in your inbox shortly!";
                        var confirmationUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Please Confirm you email", "Please confirm your account by clicking this link: <a href=\"" + confirmationUrl + "\">link</a>");
                    }

                    await UserManager.AddToRoleAsync(user.Id, "User");
                    return View();
                }
            }
            else
            {
                result = await UserManager.AddPasswordAsync(user.Id, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            AddErrors(result);
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { returnUrl }));
        }

        [HttpGet]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                ViewBag.Message = "You have to provide permission to the application in order to log in using a social provider.";
                return RedirectToAction("Login");
            }

            var result = await SignInManager.ExternalSignInAsync(loginInfo, false);

            switch (result)
            {
                case SignInStatus.LockedOut:
                    ViewBag.Message = "Your account is locked out. Please contact administrator.";
                    return View("Login");
                case SignInStatus.RequiresVerification:
                    ViewBag.Message = "You need to verify your account before proceeding.";
                    return View("Login");
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                // ReSharper disable once RedundantCaseLabel
                case SignInStatus.Failure:
                default:
                    var user = await UserManager.FindByNameAsync(loginInfo.Email);

                    // We need to check if user has already created an account using the normal registration process
                    // (without using external providers).
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = loginInfo.Email,
                            Email = loginInfo.Email,
                            EmailConfirmed = true
                        };

                        var createResult = await UserManager.CreateAsync(user);

                        if (!createResult.Succeeded)
                        {
                            return RedirectToAction("Login", "Account");
                        }

                        await UserManager.AddToRoleAsync(user.Id, "User");
                    }

                    await UserManager.AddLoginAsync(user.Id, loginInfo.Login);

                    if (loginInfo.Login.LoginProvider == "Facebook")
                    {
                        await StoreFacebookAuthenticationToken(user);
                    }

                    await SignInManager.SignInAsync(user, false, false);
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                ViewBag.Message = "We could not verify your account successfully.";
                return RedirectToAction("Login", "Account");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (!result.Succeeded)
            {
                ViewBag.Message = "We could not verify your account successfully.";
                return RedirectToAction("Login", "Account");
            }

            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.Message = "We could not find your account.";
                return RedirectToAction("Login", "Account");
            }

            await SignInManager.SignInAsync(user, false, true);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<ViewResult> EditProfile(string message = "")
        {
            var userId = User.Identity.GetUserId();
            var userLogins = await UserManager.GetLoginsAsync(userId);
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(p => auth.AuthenticationType != p.LoginProvider));
            var hasPassword = await UserManager.HasPasswordAsync(userId);

            var model = new EditProfileModel
            {
                ExternalProviders = userLogins.ToList(),
                OtherProviders = otherLogins.ToList(),
                UserHasPassword = hasPassword,
                Message = message
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            var userId = User.Identity.GetUserId();
            string message;

            if (loginInfo == null)
            {
                message = "You have to provide permission to the application in order to connect using a social provider.";
            }
            else
            {
                var result = await UserManager.AddLoginAsync(userId, loginInfo.Login);

                if (!result.Succeeded)
                {
                    message = "An error occured. Please try again.";
                    return RedirectToAction("EditProfile", "Account", new { message });
                }

                message = "You have successfully linked your account with " + loginInfo.Login.LoginProvider + ".";

                if (loginInfo.Login.LoginProvider != "Facebook")
                {
                    return RedirectToAction("EditProfile", "Account", new { message });
                }

                var user = await UserManager.FindByIdAsync(userId);
                await StoreFacebookAuthenticationToken(user);
            }

            return RedirectToAction("EditProfile", "Account", new { message });
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> DeleteLogin(string loginProvider, string providerKey)
        {
            string message;
            var userId = User.Identity.GetUserId();
            var result = await UserManager.RemoveLoginAsync(userId, new UserLoginInfo(loginProvider, providerKey));

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(userId);

                if (user != null)
                {
                    await SignInManager.SignInAsync(user, false, false);
                }

                message = loginProvider + " login was successfully removed.";
            }
            else
            {
                message = "Your login could not be removed. An error has occured.";
            }

            return RedirectToAction("EditProfile", "Account", new { message });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private async Task StoreFacebookAuthenticationToken(ApplicationUser user)
        {
            var claimsIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);

            if (claimsIdentity != null)
            {
                var currentClaims = await UserManager.GetClaimsAsync(user.Id);
                var facebookAccessToken = claimsIdentity.FindAll("FacebookAccessToken").First();

                if (!currentClaims.Any())
                {
                    await UserManager.AddClaimAsync(user.Id, facebookAccessToken);
                }
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties
                {
                    RedirectUri = RedirectUri
                };

                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}