using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace AspNet.Identity.AdoNetProvider.WebUI.Infrastructure
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var user = await UserManager.FindByNameAsync(userName);

            if (user == null)
            {
                return SignInStatus.Failure;
            }

            if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
            {
                return SignInStatus.RequiresVerification;
            }

            if (shouldLockout && (user.AccessFailedCount == UserManager.MaxFailedAccessAttemptsBeforeLockout - 1))
            {
                await UserManager.SetLockoutEnabledAsync(user.Id, true);
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }

            if (await UserManager.CheckPasswordAsync(user, password))
            {
                return await SignInOrTwoFactor(user, isPersistent);
            }

            if (!shouldLockout)
            {
                return SignInStatus.Failure;
            }

            await UserManager.AccessFailedAsync(user.Id);
            return SignInStatus.Failure;
        }

        private async Task<SignInStatus> SignInOrTwoFactor(ApplicationUser user, bool isPersistent)
        {
            var userId = Convert.ToString(user.Id);

            if (await UserManager.GetTwoFactorEnabledAsync(user.Id) && (await UserManager.GetValidTwoFactorProvidersAsync(user.Id)).Count > 0 &&
                !await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId))
            {
                var identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
                AuthenticationManager.SignIn(identity);

                return SignInStatus.RequiresVerification;
            }

            await SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;
        }

        public override async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
        {
            var userIdentity = await CreateUserIdentityAsync(user);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);

            if (rememberBrowser)
            {
                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(ConvertIdToString(user.Id));

                AuthenticationManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = isPersistent
                }, userIdentity, rememberBrowserIdentity);
            }
            else
            {
                AuthenticationManager.SignIn(new AuthenticationProperties
                {
                    IsPersistent = isPersistent
                }, userIdentity);
            }
        }

        public override async Task<SignInStatus> TwoFactorSignInAsync(string provider, string code, bool isPersistent,
            bool rememberBrowser)
        {
            var userId = await GetVerifiedUserIdAsync();

            if (userId == null)
            {
                return SignInStatus.Failure;
            }

            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return SignInStatus.Failure;
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }

            if (await UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code))
            {
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInAsync(user, isPersistent, rememberBrowser);

                return SignInStatus.Success;
            }

            await UserManager.AccessFailedAsync(user.Id);
            return SignInStatus.Failure;
        }

        public new async Task<SignInStatus> ExternalSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent)
        {
            var user = await UserManager.FindAsync(loginInfo.Login);

            if (user == null)
            {
                return SignInStatus.Failure;
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }

            return await SignInOrTwoFactor(user, isPersistent);
        }
    }
}