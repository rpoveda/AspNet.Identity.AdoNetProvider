using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using AspNet.Identity.AdoNetProvider.Domain.Tables;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.AdoNetProvider.Domain.Stores
{
    public class UserStore<T> : IUserLoginStore<T>, IUserClaimStore<T>, IUserRoleStore<T>, IUserPasswordStore<T>, IUserSecurityStampStore<T>, IQueryableUserStore<T>, IUserEmailStore<T>,
        IUserPhoneNumberStore<T>, IUserTwoFactorStore<T, string>, IUserLockoutStore<T, string>, IUserStore<T> where T : ApplicationUser
    {
        private readonly RoleTable<ApplicationRole> _roleTable;
        private readonly UserClaimsTable _userClaimsTable;
        private readonly UserLoginsTable<T> _userLoginsTable;
        private readonly UserRolesTable<T> _userRolesTable;
        private readonly UserTable<T> _userTable;

        public UserStore(SqlServerDatabase database)
        {
            Database = database;

            _userTable = new UserTable<T>(database);
            _userLoginsTable = new UserLoginsTable<T>(database);
            _roleTable = new RoleTable<ApplicationRole>(database);
            _userRolesTable = new UserRolesTable<T>(database);
            _userClaimsTable = new UserClaimsTable(database);
        }

        public SqlServerDatabase Database { get; set; }

        #region IQueryableUserStore implementation
        public IQueryable<T> Users
        {
            get { return _userTable.GetAllUsers(); }
        }
        #endregion

        #region IUserLoginStore implementation
        public Task AddLoginAsync(T user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login", "Parameter login cannot be null.");
            }

            return _userLoginsTable.AddUserLoginAsync(user, login);
        }

        public async Task<T> FindAsync(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login", "Parameter login cannot be null.");
            }

            var userId = await _userLoginsTable.FindUserIdByLoginAsync(login);

            if (userId == null)
            {
                return null;
            }

            var user = await _userTable.GetUserByIdAsync((string)userId);
            return user;
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            var logins = await _userLoginsTable.GetUserLoginsAsync(user.Id);
            return logins;
        }

        public Task RemoveLoginAsync(T user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login", "Parameter login cannot be null.");
            }

            return _userLoginsTable.DeleteUserLoginById(user, login);
        }

        public Task CreateAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            return _userTable.CreateUserAsync(user);
        }

        public Task DeleteAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            return _userTable.DeleteUserAsync(user.Id);
        }

        public async Task<T> FindByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId", "Parameter userId cannot be null or empty.");
            }

            var user = await _userTable.GetUserByIdAsync(userId);
            return user;
        }

        public async Task<T> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Parameter userName cannot be null or empty.");
            }

            var user = await _userTable.GetUserByNameAsync(userName);
            return user;
        }

        public Task UpdateAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            return _userTable.UpdateUserAsync(user);
        }

        public void Dispose()
        {
            if (Database == null)
            {
                return;
            }

            Database.Dispose();
            Database = null;
        }
        #endregion

        #region IUserClaimStore implementation
        public Task AddClaimAsync(T user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim", "Parameter claim cannot be null.");
            }

            return _userClaimsTable.AddUserClaimAsync(claim, user.Id);
        }

        public async Task<IList<Claim>> GetClaimsAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            return await _userClaimsTable.GetUserClaimsByUserId(user.Id);
        }

        public Task RemoveClaimAsync(T user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim", "Parameter claim cannot be null.");
            }

            return _userClaimsTable.DeleteUserClaimAsync(user.Id);
        }
        #endregion

        #region IUserRoleStore implementation
        public async Task AddToRoleAsync(T user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName", "Parameter roleName cannot be null.");
            }

            var roleId = await _roleTable.GetRoleIdAsync(roleName);

            if (roleId != null)
            {
                await _userRolesTable.AddUserToRoleAsync(user, (string)roleId);
            }
        }

        public async Task<IList<string>> GetRolesAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            var roles = await _userRolesTable.GetUserRolesByUserId(user.Id);
            return roles;
        }

        public async Task<bool> IsInRoleAsync(T user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("user", "Parameter roleName cannot be null or empty.");
            }

            var roles = await _userRolesTable.GetUserRolesByUserId(user.Id);
            return roles != null && roles.Contains(roleName);
        }

        public async Task RemoveFromRoleAsync(T user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("user", "Parameter roleName cannot be null or empty.");
            }

            var roleId = await _roleTable.GetRoleIdAsync(roleName);

            if (roleId != null)
            {
                await _userRolesTable.RemoveUserFromRoleAsync(user, (string)roleId);
            }
        }
        #endregion

        #region IUserPasswordStore implementation
        public async Task<string> GetPasswordHashAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            return await _userTable.GetPasswordHashAsync(user.Id);
        }

        public async Task<bool> HasPasswordAsync(T user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user", "Parameter user cannot be null.");
            }

            return await _userTable.GetPasswordHashAsync(user.Id) != null;
        }

        public Task SetPasswordHashAsync(T user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult<object>(null);
        }
        #endregion

        #region IUserSecurityStampStore implementation
        public Task<string> GetSecurityStampAsync(T user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(T user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult<object>(null);
        }
        #endregion

        #region IUserEmailStore implementation
        public async Task<T> FindByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email", "Parameter email cannot be null or empty.");
            }

            var user = await _userTable.GetUserByEmailAsync(email);
            return user;
        }

        public Task<string> GetEmailAsync(T user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(T user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailAsync(T user, string email)
        {
            user.Email = email;
            return _userTable.UpdateUserAsync(user);
        }

        public Task SetEmailConfirmedAsync(T user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return _userTable.UpdateUserAsync(user);
        }
        #endregion

        #region IUserPhoneNumberStore implementation
        public Task<string> GetPhoneNumberAsync(T user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(T user)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberAsync(T user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;
            return _userTable.UpdateUserAsync(user);
        }

        public Task SetPhoneNumberConfirmedAsync(T user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            return _userTable.UpdateUserAsync(user);
        }
        #endregion

        #region IUserTwoFactorStore implementation
        public Task<bool> GetTwoFactorEnabledAsync(T user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(T user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            return _userTable.UpdateUserAsync(user);
        }
        #endregion

        #region IUserLockoutStore implementation
        public Task<int> GetAccessFailedCountAsync(T user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(T user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(T user)
        {
            return Task.FromResult(user.LockoutEndDateUtc.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                : new DateTimeOffset());
        }

        public async Task<int> IncrementAccessFailedCountAsync(T user)
        {
            user.AccessFailedCount++;
            await _userTable.UpdateUserAsync(user);
            return user.AccessFailedCount;
        }

        public Task ResetAccessFailedCountAsync(T user)
        {
            user.AccessFailedCount = 0;
            return _userTable.UpdateUserAsync(user);
        }

        public Task SetLockoutEnabledAsync(T user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return _userTable.UpdateUserAsync(user);
        }

        public Task SetLockoutEndDateAsync(T user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            return _userTable.UpdateUserAsync(user);
        }
        #endregion
    }
}