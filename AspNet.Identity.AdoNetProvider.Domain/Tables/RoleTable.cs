using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;

namespace AspNet.Identity.AdoNetProvider.Domain.Tables
{
    public class RoleTable<T> where T : ApplicationRole
    {
        private readonly SqlServerDatabase _database;

        public RoleTable(SqlServerDatabase database)
        {
            _database = database;
        }

        public Task DeleteRoleAsync(string roleId)
        {
            const string commandText = "delete from dbo.AspNetRoles " +
                                       "where Id = @id;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", roleId}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public Task InsertRoleAsync(T role)
        {
            const string commandText = "insert into dbo.AspNetRoles (Id, Name) " +
                                       "values (@id, @name);";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", role.Id},
                {"@name", role.Name}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public Task UpdateRoleAsync(T role)
        {
            const string commandText = "update dbo.AspNetRoles " +
                                       "set Name = @name where Id = @id;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", role.Id},
                {"@name", role.Name}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public IQueryable<T> GetAllRoles()
        {
            const string commandText = "select Id, Name " +
                                       "from dbo.AspNetRoles;";

            return _database.ExecuteReader(commandText, new Dictionary<string, object>())
                            .Select(e => new ApplicationRole
                            {
                                Id = e.Single(p => p.Key == "Id").Value,
                                Name = e.Single(p => p.Key == "Name").Value
                            } as T).AsQueryable();
        }

        public async Task<T> FindRoleByIdAsync(string roleId)
        {
            var roleName = await GetRoleNameAsync(roleId);
            ApplicationRole role = null;

            if (roleName != null)
            {
                role = new ApplicationRole((string)roleName, roleId);
            }

            return role as T;
        }

        public async Task<T> FindRoleByNameAsync(string roleName)
        {
            var roleId = await GetRoleIdAsync(roleName);
            ApplicationRole role = null;

            if (roleId != null)
            {
                role = new ApplicationRole(roleName, (string)roleId);
            }

            return role as T;
        }

        public async Task<object> GetRoleNameAsync(string roleId)
        {
            const string commandText = "select Name " +
                                       "from dbo.AspNetRoles " +
                                       "where Id = @id;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", roleId}
            };

            return await _database.ExecuteScalarAsync(commandText, sqlParameters);
        }

        public async Task<object> GetRoleIdAsync(string roleName)
        {
            const string commandText = "select Id " +
                                       "from dbo.AspNetRoles " +
                                       "where Name = @name;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@name", roleName}
            };

            return await _database.ExecuteScalarAsync(commandText, sqlParameters);
        }
    }
}