using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;

namespace AspNet.Identity.AdoNetProvider.Domain.Tables
{
    public class UserClaimsTable
    {
        private readonly SqlServerDatabase _database;

        public UserClaimsTable(SqlServerDatabase database)
        {
            _database = database;
        }

        public async Task<List<Claim>> GetUserClaimsByUserId(string userId)
        {
            const string commandText = "select ClaimType, ClaimValue " +
                                       "from dbo.AspNetUserClaims " +
                                       "where UserId = @userId;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            var rows = await _database.ExecuteReaderAsync(commandText, sqlParameters);
            return rows.Select(row => new Claim(row["ClaimType"], row["ClaimValue"])).ToList();
        }

        public Task AddUserClaimAsync(Claim claim, string userId)
        {
            const string commandText = "insert into dbo.AspNetUserClaims (ClaimValue, ClaimType, UserId) " +
                                       "values (@claimValue, @claimType, @userId);";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@claimValue", claim.Value},
                {"@claimType", claim.Type},
                {"@userId", userId}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public Task DeleteUserClaimAsync(string userId)
        {
            const string commandText = "delete from dbo.AspNetUserClaims " +
                                       "where UserId = @userId;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }
    }
}