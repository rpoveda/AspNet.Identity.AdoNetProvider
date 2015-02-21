using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.AdoNetProvider.Domain.Tables
{
    public class UserLoginsTable<T> where T : ApplicationUser
    {
        private readonly SqlServerDatabase _database;

        public UserLoginsTable(SqlServerDatabase database)
        {
            _database = database;
        }

        public async Task<object> FindUserIdByLoginAsync(UserLoginInfo userLoginInfo)
        {
            const string commandText = "select UserId from dbo.AspNetUserLogins " +
                                       "where LoginProvider = @loginProvider and ProviderKey = @providerKey;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@loginProvider", userLoginInfo.LoginProvider},
                {"@providerKey", userLoginInfo.ProviderKey}
            };

            return await _database.ExecuteScalarAsync(commandText, sqlParameters);
        }

        public Task AddUserLoginAsync(T user, UserLoginInfo login)
        {
            const string commandText = "insert into dbo.AspNetUserLogins (LoginProvider, ProviderKey, UserId) " +
                                       "values (@loginProvider, @providerKey, @userId);";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@loginProvider", login.LoginProvider},
                {"@providerKey", login.ProviderKey},
                {"@userId", user.Id}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public async Task<List<UserLoginInfo>> GetUserLoginsAsync(string userId)
        {
            const string commandText = "select LoginProvider, ProviderKey " +
                                       "from dbo.AspNetUserLogins " +
                                       "where UserId = @userId;";

            var sqlParameters = new Dictionary<string, object>();
            var rows = await _database.ExecuteReaderAsync(commandText, sqlParameters);

            return rows.Select(row => new UserLoginInfo(row["LoginProvider"], row["ProviderKey"])).ToList();
        }

        public Task DeleteUserLoginById(T user, UserLoginInfo login)
        {
            const string commandText = "delete from dbo.AspNetUserLogins " +
                                       "where UserId = @userId and LoginProvider = @loginProvider and ProviderKey = @providerKey;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", user.Id},
                {"@loginProvider", login.LoginProvider},
                {"@providerKey", login.ProviderKey}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }
    }
}