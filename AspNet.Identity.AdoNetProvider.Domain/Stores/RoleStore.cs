using System;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;
using AspNet.Identity.AdoNetProvider.Domain.Tables;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.AdoNetProvider.Domain.Stores
{
    public class RoleStore<T> : IQueryableRoleStore<T>, IRoleStore<T> where T : ApplicationRole
    {
        private readonly RoleTable<T> _roleTable;

        public RoleStore(SqlServerDatabase database)
        {
            _roleTable = new RoleTable<T>(database);
            Database = database;
        }

        public SqlServerDatabase Database { get; set; }

        #region IQueryableRoleStore implementation

        public IQueryable<T> Roles
        {
            get { return _roleTable.GetAllRoles(); }
        }

        public Task CreateAsync(T role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role", "Parameter role cannot be null.");
            }

            return _roleTable.InsertRoleAsync(role);
        }

        public Task DeleteAsync(T role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role", "Parameter role cannot be null.");
            }

            return _roleTable.DeleteRoleAsync(role.Id);
        }

        public async Task<T> FindByIdAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                throw new ArgumentNullException("roleId", "Parameter roleId cannot be null or empty.");
            }

            return await _roleTable.FindRoleByIdAsync(roleId);
        }

        public async Task<T> FindByNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName", "Parameter roleName cannot be null or empty.");
            }

            return await _roleTable.FindRoleByNameAsync(roleName);
        }

        public Task UpdateAsync(T role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role", "Parameter role cannot be null.");
            }

            return _roleTable.UpdateRoleAsync(role);
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
    }
}