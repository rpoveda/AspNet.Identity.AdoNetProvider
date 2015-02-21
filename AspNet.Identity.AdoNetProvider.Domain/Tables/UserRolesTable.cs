using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;

namespace AspNet.Identity.AdoNetProvider.Domain.Tables
{
    public class UserRolesTable<T> where T : ApplicationUser
    {
        private readonly SqlServerDatabase _database;

        public UserRolesTable(SqlServerDatabase database)
        {
            _database = database;
        }

        public Task AddUserToRoleAsync(T user, string roleId)
        {
            const string commandText = "insert into dbo.AspNetUserRoles (UserId, RoleId) " +
                                       "values(@userId, @roleId);";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", user.Id},
                {"@roleId", roleId}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public async Task<List<string>> GetUserRolesByUserId(string userId)
        {
            const string commandText = "select r.Name from AspNetUserRoles as ur " +
                                       "inner join dbo.AspNetRoles as r on ur.RoleId = r.Id " +
                                       "where UserId = @userId;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            var rows = await _database.ExecuteReaderAsync(commandText, sqlParameters);
            return rows.Select(row => row["Name"]).ToList();
        }

        public Task RemoveUserFromRoleAsync(T user, string roleId)
        {
            const string commandText = "delete from dbo.AspNetUserRoles " +
                                       "where UserId = @userId and RoleId = @roleId;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", user.Id},
                {"@roleId", roleId}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }
    }
}