using System;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using AspNet.Identity.AdoNetProvider.Domain.Stores;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace AspNet.Identity.AdoNetProvider.WebUI.Infrastructure
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public override Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var hasher = new PasswordHasher();
            var result = hasher.VerifyHashedPassword(user.PasswordHash, password);

            return Task.FromResult(result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded);
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var database = context.Get<SqlServerDatabase>();
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(database));

            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            manager.PasswordValidator = new PasswordValidator
            {
                RequireDigit = true,
                RequiredLength = 6,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false
            };

            manager.UserLockoutEnabledByDefault = false;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 3;

            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });

            manager.EmailService = new EmailService();
            var dataProtectionProvider = options.DataProtectionProvider;

            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromHours(3)
                };
            }

            return manager;
        }
    }
}