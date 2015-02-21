using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Identity.AdoNetProvider.Domain.Concrete;
using AspNet.Identity.AdoNetProvider.Domain.Entities;

namespace AspNet.Identity.AdoNetProvider.Domain.Tables
{
    public class UserTable<T> where T : ApplicationUser
    {
        private readonly SqlServerDatabase _database;

        public UserTable(SqlServerDatabase database)
        {
            _database = database;
        }

        public Task CreateUserAsync(T user)
        {
            const string commandText =
                "insert into dbo.AspNetUsers (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc, LockoutEnabled, AccessFailedCount, UserName) " +
                "values (@id, @email, @emailConfirmed, @passwordHash, @securityStamp, @phoneNumber, @phoneNumberConfirmed, @twoFactorEnabled, @lockoutEndDateUtc, @lockoutEnabled, @accessFailedCount, @userName);";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", user.Id},
                {"@email", user.Email},
                {"@emailConfirmed", user.EmailConfirmed},
                {"@passwordHash", user.PasswordHash},
                {"@securityStamp", user.SecurityStamp},
                {"@phoneNumber", user.PhoneNumber},
                {"@phoneNumberConfirmed", user.PhoneNumberConfirmed},
                {"@twoFactorEnabled", user.TwoFactorEnabled},
                {"@lockoutEndDateUtc", user.LockoutEndDateUtc},
                {"@lockoutEnabled", user.LockoutEnabled},
                {"@accessFailedCount", user.AccessFailedCount},
                {"@userName", user.UserName}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public Task DeleteUserAsync(string userId)
        {
            const string commandText = "delete from dbo.AspNetUsers " +
                                       "where Id = @id;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", userId}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public async Task<T> GetUserByIdAsync(string userId)
        {
            const string commandText = "select * from dbo.AspNetUsers " +
                                       "where Id = @id;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", userId}
            };

            var rows = await _database.ExecuteReaderAsync(commandText, sqlParameters);

            if (rows == null || rows.Count != 1)
            {
                return null;
            }

            var row = rows.First();
            var user = (T)Activator.CreateInstance(typeof(T));

            user.Id = row["Id"];
            user.Email = string.IsNullOrEmpty(row["Email"]) ? null : row["Email"];
            user.EmailConfirmed = row["EmailConfirmed"] == "True";
            user.PasswordHash = string.IsNullOrEmpty(row["PasswordHash"]) ? null : row["PasswordHash"];
            user.SecurityStamp = string.IsNullOrEmpty(row["SecurityStamp"]) ? null : row["SecurityStamp"];
            user.PhoneNumber = string.IsNullOrEmpty(row["PhoneNumber"]) ? null : row["PhoneNumber"];
            user.PhoneNumberConfirmed = row["PhoneNumberConfirmed"] == "True";
            user.TwoFactorEnabled = row["TwoFactorEnabled"] == "True";
            user.LockoutEndDateUtc = string.IsNullOrEmpty(row["LockoutEndDateUtc"])
                ? default(DateTime?)
                : DateTime.Parse(row["LockoutEndDateUtc"]);
            user.LockoutEnabled = row["LockoutEnabled"] == "True";
            user.AccessFailedCount = string.IsNullOrEmpty(row["AccessFailedCount"])
                ? 0
                : int.Parse(row["AccessFailedCount"]);
            user.UserName = row["UserName"];

            return user;
        }

        public async Task<T> GetUserByNameAsync(string userName)
        {
            const string commandText = "select * from dbo.AspNetUsers " +
                                       "where UserName = @userName;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userName", userName}
            };

            var rows = await _database.ExecuteReaderAsync(commandText, sqlParameters);

            if (rows == null || rows.Count != 1)
            {
                return null;
            }

            var row = rows.First();
            var user = (T)Activator.CreateInstance(typeof(T));

            user.Id = row["Id"];
            user.Email = string.IsNullOrEmpty(row["Email"]) ? null : row["Email"];
            user.EmailConfirmed = row["EmailConfirmed"] == "True";
            user.PasswordHash = string.IsNullOrEmpty(row["PasswordHash"]) ? null : row["PasswordHash"];
            user.SecurityStamp = string.IsNullOrEmpty(row["SecurityStamp"]) ? null : row["SecurityStamp"];
            user.PhoneNumber = string.IsNullOrEmpty(row["PhoneNumber"]) ? null : row["PhoneNumber"];
            user.PhoneNumberConfirmed = row["PhoneNumberConfirmed"] == "True";
            user.TwoFactorEnabled = row["TwoFactorEnabled"] == "True";
            user.LockoutEndDateUtc = string.IsNullOrEmpty(row["LockoutEndDateUtc"])
                ? default(DateTime)
                : DateTime.Parse(row["LockoutEndDateUtc"]);
            user.LockoutEnabled = row["LockoutEnabled"] == "True";
            user.AccessFailedCount = string.IsNullOrEmpty(row["AccessFailedCount"])
                ? 0
                : int.Parse(row["AccessFailedCount"]);
            user.UserName = row["UserName"];

            return user;
        }

        public async Task<T> GetUserByEmailAsync(string email)
        {
            const string commandText = "select * from dbo.AspNetUsers " +
                                       "where Email = @email;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@email", email}
            };

            var rows = await _database.ExecuteReaderAsync(commandText, sqlParameters);

            if (rows == null || rows.Count != 1)
            {
                return null;
            }

            var row = rows.First();
            var user = (T)Activator.CreateInstance(typeof(T));

            user.Id = row["Id"];
            user.Email = string.IsNullOrEmpty(row["Email"]) ? null : row["Email"];
            user.EmailConfirmed = row["EmailConfirmed"] == "True";
            user.PasswordHash = string.IsNullOrEmpty(row["PasswordHash"]) ? null : row["PasswordHash"];
            user.SecurityStamp = string.IsNullOrEmpty(row["SecurityStamp"]) ? null : row["SecurityStamp"];
            user.PhoneNumber = string.IsNullOrEmpty(row["PhoneNumber"]) ? null : row["PhoneNumber"];
            user.PhoneNumberConfirmed = row["PhoneNumberConfirmed"] == "True";
            user.TwoFactorEnabled = row["TwoFactorEnabled"] == "True";
            user.LockoutEndDateUtc = string.IsNullOrEmpty(row["LockoutEndDateUtc"])
                ? default(DateTime)
                : DateTime.Parse(row["LockoutEndDateUtc"]);
            user.LockoutEnabled = row["LockoutEnabled"] == "True";
            user.AccessFailedCount = string.IsNullOrEmpty(row["AccessFailedCount"])
                ? 0
                : int.Parse(row["AccessFailedCount"]);
            user.UserName = row["UserName"];

            return user;
        }

        public IQueryable<T> GetAllUsers()
        {
            const string commandText = "select * " +
                                       "from dbo.AspNetUsers;";

            return _database.ExecuteReader(commandText, new Dictionary<string, object>())
                            .Select(e => new ApplicationUser
                            {
                                Id = e.Single(p => p.Key == "Id").Value,
                                Email = e.SingleOrDefault(p => p.Key == "Email").Value,
                                EmailConfirmed = e.Single(p => p.Key == "EmailConfirmed").Value == "True",
                                PasswordHash = e.SingleOrDefault(p => p.Key == "PasswordHash").Value,
                                SecurityStamp = e.SingleOrDefault(p => p.Key == "SecurityStamp").Value,
                                PhoneNumber = e.SingleOrDefault(p => p.Key == "PhoneNumber").Value,
                                PhoneNumberConfirmed = e.Single(p => p.Key == "PhoneNumberConfirmed").Value == "True",
                                TwoFactorEnabled = e.Single(p => p.Key == "TwoFactorEnabled").Value == "True",
                                LockoutEndDateUtc = string.IsNullOrEmpty(e.SingleOrDefault(p => p.Key == "LockoutEndDateUtc").Value)
                                        ? default(DateTime)
                                        : DateTime.Parse(e.SingleOrDefault(p => p.Key == "LockoutEndDateUtc").Value),
                                LockoutEnabled = e.Single(p => p.Key == "LockoutEnabled").Value == "1",
                                AccessFailedCount = string.IsNullOrEmpty(e.Single(p => p.Key == "AccessFailedCount").Value)
                                        ? 0
                                        : int.Parse(e.Single(p => p.Key == "AccessFailedCount").Value),
                                UserName = e.SingleOrDefault(p => p.Key == "UserName").Value
                            } as T).AsQueryable();
        }

        public Task UpdateUserAsync(T user)
        {
            const string commandText = "update dbo.AspNetUsers " +
                                       "set Email = @email, EmailConfirmed = @emailConfirmed, PasswordHash = @passwordHash, SecurityStamp = @securityStamp, PhoneNumber = @phoneNumber, " +
                                       "PhoneNumberConfirmed = @phoneNumberConfirmed, TwoFactorEnabled = @twoFactorEnabled, LockoutEndDateUtc = @lockoutEndDateUtc, LockoutEnabled = @lockoutEnabled, " +
                                       "AccessFailedCount = @accessFailedCount, UserName = @userName " +
                                       "where Id = @id;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@id", user.Id},
                {"@email", user.Email},
                {"@emailConfirmed", user.EmailConfirmed},
                {"@passwordHash", user.PasswordHash},
                {"@securityStamp", user.SecurityStamp},
                {"@phoneNumber", user.PhoneNumber},
                {"@phoneNumberConfirmed", user.PhoneNumberConfirmed},
                {"@twoFactorEnabled", user.TwoFactorEnabled},
                {"@lockoutEndDateUtc", user.LockoutEndDateUtc},
                {"@lockoutEnabled", user.LockoutEnabled},
                {"@accessFailedCount", user.AccessFailedCount},
                {"@userName", user.UserName}
            };

            return _database.ExecuteNonQueryAsync(commandText, sqlParameters);
        }

        public async Task<string> GetPasswordHashAsync(string userId)
        {
            const string commandText = "select PasswordHash " +
                                       "from dbo.AspNetUsers " +
                                       "where Id = @userId;";

            var sqlParameters = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            var passwordHash = await _database.ExecuteScalarAsync(commandText, sqlParameters);

            if (!(passwordHash is DBNull))
            {
                return (string)passwordHash;
            }

            return null;
        }
    }
}