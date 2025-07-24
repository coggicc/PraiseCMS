using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PraiseCMS.DataAccess.DAL
{
    public class AdoDataAccess : IDisposable
    {
        #region Boilerplate
        private readonly string connectionString;

        public AdoDataAccess()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private static readonly ApplicationDbContext db = new ApplicationDbContext();

        public void ExecuteNonQuery(string query, params (string, object)[] parameters)
        {
            try
            {
                using (var sqlCnn = new SqlConnection(connectionString))
                {
                    sqlCnn.Open();
                    using (var cmd = new SqlCommand(query, sqlCnn))
                    {
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }
        }

        private bool GetBoolean(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] is DBNull)
            {
                return false;
            }

            bool.TryParse(reader[columnName].ToString(), out bool result);
            return result;
        }

        private DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] is DBNull)
            {
                return DateTime.MinValue; // Or any default value you prefer
            }

            DateTime.TryParse(reader[columnName].ToString(), out DateTime result);
            return result;
        }

        public void Dispose()
        {
        }
        #endregion

        #region Roles
        public string InsertRole(ApplicationRoles role)
        {
            const string query = "INSERT INTO AspNetRoles (Id, Name, Description, ChurchId) VALUES (@Id, @Name, @Description, @ChurchId)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", role.Id);
                command.Parameters.AddWithValue("@Name", role.Name);
                command.Parameters.AddWithValue("@Description", role.Description);
                command.Parameters.AddWithValue("@ChurchId", role.ChurchId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return "success";
                }
                catch (SqlException ex)
                {
                    ExceptionLogger.LogSqlException(ex);
                    return "exception";
                }
            }
        }

        public string UpdateRole(ApplicationRoles role)
        {
            const string query = "UPDATE AspNetRoles SET [Name] = @Name, [Description] = @Description WHERE Id = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", role.Name);
                command.Parameters.AddWithValue("@Description", role.Description);
                command.Parameters.AddWithValue("@Id", role.Id);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return "updated";
                }
                catch (SqlException ex)
                {
                    ExceptionLogger.LogSqlException(ex);
                    return "exception";
                }
            }
        }

        public List<ApplicationRoles> ReadAllRoles(string churchId = null)
        {
            var result = new List<ApplicationRoles>();
            string query;

            // Define the SQL query based on whether a churchId is provided or not
            if (string.IsNullOrEmpty(churchId))
            {
                query = @"
                    SELECT Id, Name, Description, ChurchId 
                    FROM aspnetroles 
                    WHERE ChurchId IS NULL OR ChurchId = '' 
                    ORDER BY Name";
            }
            else
            {
                query = @"
                    SELECT Id, Name, Description, ChurchId 
                    FROM aspnetroles 
                    WHERE (ChurchId IS NULL OR ChurchId = @ChurchId) 
                    ORDER BY Name";
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, connection))
                {
                    // Add parameter only if churchId is provided
                    if (!string.IsNullOrEmpty(churchId))
                    {
                        cmd.Parameters.Add(new SqlParameter("@ChurchId", SqlDbType.NVarChar) { Value = churchId });
                    }

                    connection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                result.Add(new ApplicationRoles
                                {
                                    Id = reader["Id"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    ChurchId = reader["ChurchId"].ToString()
                                });
                            }
                            catch (Exception ex)
                            {
                                // Log exception related to data parsing
                                ExceptionLogger.LogException(ex);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public List<ApplicationRoles> ReadAllRolesWithModules(string churchId)
        {
            var result = new List<ApplicationRoles>();

            try
            {
                string query = churchId.IsNullOrEmpty()
                    ? "SELECT * FROM aspnetroles r ORDER BY r.name;"
                    : "SELECT * FROM aspnetroles r WHERE (r.churchid IS NULL OR r.churchid = @ChurchId) ORDER BY r.name;";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    // Add parameter
                    command.Parameters.AddWithValue("@ChurchId", string.IsNullOrEmpty(churchId) ? DBNull.Value : (object)churchId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var role = new ApplicationRoles
                                {
                                    Id = reader["Id"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    ChurchId = reader["ChurchId"].ToString()
                                };

                                result.Add(role);
                            }
                            catch (Exception ex)
                            {
                                ExceptionLogger.LogException(ex);
                            }
                        }
                    }
                }

                // Fetch modules once and associate them with each role
                var modules = db.Modules.ToList();
                foreach (var item in result)
                {
                    item.Modules = modules;
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public ApplicationRoles ReadRoleByUserId(string UserId)
        {
            var result = new ApplicationRoles();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string querystring = "SELECT r.* FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id = ur.RoleId WHERE ur.UserId = @UserId";
                    using (var cmd = new SqlCommand(querystring, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result.Id = reader["Id"].ToString();
                                result.Name = reader["Name"].ToString();
                                result.Description = reader["Description"].ToString();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public List<ApplicationRoles> ReadRolesByUserId(string userId)
        {
            var result = new List<ApplicationRoles>();

            try
            {
                var rolesIdList = ReadUserRolesByUserId(userId);

                if (rolesIdList.Count == 0)
                {
                    return result;
                }

                var joined = string.Join(",", rolesIdList.Select(x => "@RoleId" + x.RoleId)); // Parameterized query

                var query = "SELECT * FROM aspnetroles r WHERE r.id IN (" + joined + ") ORDER BY r.name;";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    // Add parameters
                    foreach (var roleId in rolesIdList)
                    {
                        command.Parameters.AddWithValue("@RoleId" + roleId.RoleId, roleId.RoleId);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ApplicationRoles
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                ChurchId = reader["ChurchId"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return result;
        }

        public Dictionary<string, List<ApplicationRoles>> ReadRolesByUserIds(List<string> userIds)
        {
            var result = new Dictionary<string, List<ApplicationRoles>>();

            try
            {
                // Get all role IDs for the given user IDs
                var rolesIdList = userIds
                    .SelectMany(userId => ReadUserRolesByUserId(userId)
                    .Select(role => new { UserId = userId, RoleId = role.RoleId }))
                    .ToList();

                if (rolesIdList.Count == 0)
                {
                    return result; // Return empty dictionary if no roles found
                }

                var roleIds = rolesIdList.Select(x => x.RoleId).Distinct().ToList();
                var joined = string.Join(",", roleIds.Select((_, index) => $"@RoleId{index}")); // Parameterized query

                var query = $"SELECT * FROM aspnetroles r WHERE r.id IN ({joined}) ORDER BY r.name;";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    // Add parameters for role IDs
                    for (var i = 0; i < roleIds.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@RoleId{i}", roleIds[i]);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        var rolesDictionary = new Dictionary<string, ApplicationRoles>();

                        while (reader.Read())
                        {
                            var role = new ApplicationRoles
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                ChurchId = reader["ChurchId"].ToString()
                            };
                            rolesDictionary[role.Id] = role;
                        }

                        // Map roles back to user IDs
                        foreach (var userRole in rolesIdList)
                        {
                            if (!result.ContainsKey(userRole.UserId))
                            {
                                result[userRole.UserId] = new List<ApplicationRoles>();
                            }

                            if (rolesDictionary.TryGetValue(userRole.RoleId, out var role))
                            {
                                result[userRole.UserId].Add(role);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return result;
        }

        public List<ApplicationRoles> ReadRolesByRoleIds(List<string> roleIds)
        {
            var result = new List<ApplicationRoles>();

            try
            {
                if (roleIds.Count == 0)
                {
                    return result;
                }

                string joined = "'" + string.Join("','", roleIds) + "'";
                string querystring = "SELECT * FROM aspnetroles r WHERE r.id IN (" + joined + ") ORDER BY r.name";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(querystring, connection))
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                result.Add(new ApplicationRoles
                                {
                                    Id = reader["Id"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    ChurchId = reader["ChurchId"].ToString()
                                });
                            }
                            catch (Exception ex)
                            {
                                ExceptionLogger.LogException(ex);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public ApplicationRoles ReadRoleById(string roleId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string querystring = "SELECT Id, Name, Description FROM aspnetroles WHERE id = @RoleId";
                    using (SqlCommand cmd = new SqlCommand(querystring, connection))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", roleId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ApplicationRoles
                                {
                                    Id = reader["Id"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return null;
        }

        public ApplicationRoles ReadRoleByName(string name)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string querystring = "SELECT Id, Name, Description FROM aspnetroles WHERE name = @Name";
                    using (SqlCommand cmd = new SqlCommand(querystring, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ApplicationRoles
                                {
                                    Id = reader["Id"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return null;
        }

        public Result<string> DeleteRoleById(string roleId)
        {
            try
            {
                int totalUsers = IsRoleAssignedToUsersByRoleId(roleId);

                if (totalUsers > 0)
                {
                    return Result<string>.Failure($"There are {totalUsers} users assigned to this role. Please remove users from this role before attempting to delete.");
                }
                else
                {
                    // Delete permissions
                    DeletePermissionsByRoleId(roleId);

                    // Delete role modules
                    DeleteModulesByRoleId(roleId);

                    // Delete role in AspNetRoles
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        const string querystring = "DELETE FROM aspnetroles WHERE id = @RoleId";
                        using (var cmd = new SqlCommand(querystring, connection))
                        {
                            cmd.Parameters.AddWithValue("@RoleId", roleId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return Result<string>.Success("success");
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return Result<string>.FromException(ex, "An exception occurred while processing your request.", null);
            }
        }
        #endregion

        #region Modules
        public bool DeleteModulesByRoleId(string roleId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "DELETE FROM RoleModules WHERE RoleId = @RoleId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", roleId);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return false;
            }
        }

        public List<Modules> ReadPermittedModulesByRolesAndUserId(List<ApplicationRoles> roles, string userId)
        {
            var result = new List<Modules>();

            try
            {
                // Extract role IDs from the provided roles
                var roleIds = roles.Select(x => x.Id).ToList();

                // Retrieve module IDs based on the roles
                var moduleIdsFromRoles = db.RoleModules
                    .Where(x => roleIds.Contains(x.RoleId))
                    .Select(x => x.ModuleId)
                    .ToList();

                // Retrieve parent module IDs based on the module IDs from roles
                var parentModuleIds = db.Modules
                    .Where(x => moduleIdsFromRoles.Contains(x.ParentId))
                    .Select(x => x.Id)
                    .ToList();

                // Combine module IDs from roles and parent module IDs
                var combinedModuleIds = moduleIdsFromRoles.Union(parentModuleIds).ToList();

                // Retrieve sub-child module IDs recursively
                var subChildModuleIds = new List<string>();
                while (parentModuleIds.Any())
                {
                    subChildModuleIds.AddRange(parentModuleIds);
                    parentModuleIds = db.Modules
                        .Where(x => parentModuleIds.Contains(x.ParentId))
                        .Select(x => x.Id)
                        .ToList();
                }

                // Combine module IDs from roles, parent modules, and sub-child modules
                combinedModuleIds.AddRange(subChildModuleIds);

                // Retrieve module IDs directly associated with the user
                var moduleIdsFromUserPermissions = db.Permissions
                    .Where(x => x.Type == PermissionType.User.ToString() && x.TypeId == userId)
                    .Select(x => x.ModuleId)
                    .ToList();

                // Combine all module IDs
                combinedModuleIds.AddRange(moduleIdsFromUserPermissions);

                // Distinct module IDs
                combinedModuleIds = combinedModuleIds.Distinct().ToList();

                // Retrieve modules based on the combined module IDs
                result = db.Modules
                    .Where(x => combinedModuleIds.Contains(x.Id) || combinedModuleIds.Contains(x.ParentId))
                    .ToList();
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return result;
        }
        #endregion

        #region User Roles
        public string InsertUserRole(AspNetUserRoles role)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES(@UserId, @RoleId)";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", role.UserId);
                        cmd.Parameters.AddWithValue("@RoleId", role.RoleId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return "success";
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return "exception";
            }
        }

        public List<AspNetUserRoles> ReadAllUserRoles()
        {
            var result = new List<AspNetUserRoles>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "SELECT UserId, RoleId FROM aspnetuserroles";
                    using (var cmd = new SqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                result.Add(new AspNetUserRoles
                                {
                                    UserId = reader["UserId"].ToString(),
                                    RoleId = reader["RoleId"].ToString()
                                });
                            }
                            catch (Exception ex)
                            {
                                ExceptionLogger.LogException(ex);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public List<AspNetUserRoles> ReadUserRolesByUserId(string userId)
        {
            var result = new List<AspNetUserRoles>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "SELECT UserId, RoleId FROM aspnetuserroles WHERE UserId = @UserId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    result.Add(new AspNetUserRoles
                                    {
                                        UserId = reader["UserId"].ToString(),
                                        RoleId = reader["RoleId"].ToString()
                                    });
                                }
                                catch (Exception ex)
                                {
                                    ExceptionLogger.LogException(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public int IsRoleAssignedToUsersByRoleId(string roleId)
        {
            int result = 0;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "SELECT COUNT(UserId) AS TotalUsers FROM AspNetUserRoles WHERE RoleId = @RoleId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", roleId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public bool DeleteUserRole(AspNetUserRoles obj)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", obj.UserId);
                        cmd.Parameters.AddWithValue("@RoleId", obj.RoleId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return false;
            }
        }

        public void UpdateUserRoles(string userId, List<string> roleIds)
        {
            try
            {
                ClearUserRoles(userId);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var roleId in roleIds)
                    {
                        const string query = "INSERT INTO AspNetUserRoles (UserId,RoleId) VALUES(@UserId, @RoleId)";
                        using (var cmd = new SqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@RoleId", roleId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }
        }

        public void InsertUserRoleByName(string userId, string roleName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "INSERT INTO AspNetUserRoles (UserId,RoleId) SELECT @UserId, Id FROM aspnetroles WHERE Name = @RoleName";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@RoleName", roleName);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }
        }

        public void ClearUserRoles(string userId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "DELETE FROM AspNetUserRoles WHERE UserId = @UserId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }
        }
        #endregion

        #region Users
        public bool InsertUser(ApplicationUser model)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "INSERT INTO AspNetUsers (Id, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, " +
                                "UserName, IsActive, CreatedDate, FirstName, LastName, Email, PasswordHash, SecurityStamp, PhoneNumber, " +
                                "PhoneVerificationCode, ExternalProvider, ExternalProviderId) VALUES " +
                                "(@Id, @EmailConfirmed, @PhoneNumberConfirmed, @TwoFactorEnabled, @TwoFactorEnabled, @LockoutEnabled, @AccessFailedCount, " +
                                "@UserName, @IsActive, @CreatedDate, @FirstName, @LastName, @Email, @PasswordHash, @SecurityStamp, @PhoneNumber, " +
                                "@PhoneVerificationCode, @ExternalProvider, @ExternalProviderId)";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", model.Id);
                        cmd.Parameters.AddWithValue("@EmailConfirmed", model.EmailConfirmed);
                        cmd.Parameters.AddWithValue("@PhoneNumberConfirmed", model.PhoneNumberConfirmed);
                        cmd.Parameters.AddWithValue("@TwoFactorEnabled", model.TwoFactorEnabled);
                        cmd.Parameters.AddWithValue("@TwoFactorEnabled", model.TwoFactorEnabled);
                        cmd.Parameters.AddWithValue("@LockoutEnabled", model.LockoutEnabled);
                        cmd.Parameters.AddWithValue("@AccessFailedCount", model.AccessFailedCount);
                        cmd.Parameters.AddWithValue("@UserName", model.UserName);
                        cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                        cmd.Parameters.AddWithValue("@CreatedDate", model.CreatedDate);
                        cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", model.LastName);
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
                        cmd.Parameters.AddWithValue("@SecurityStamp", model.SecurityStamp);
                        cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                        cmd.Parameters.AddWithValue("@PhoneVerificationCode", model.PhoneVerificationCode);
                        cmd.Parameters.AddWithValue("@ExternalProvider", model.ExternalProvider);
                        cmd.Parameters.AddWithValue("@ExternalProviderId", model.ExternalProviderId);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return false;
            }
        }

        public List<ApplicationUser> ReadUsersByRoleId(string roleId, string churchId = null)
        {
            var result = new List<ApplicationUser>();

            try
            {
                string querystring;

                if (churchId.IsNotNullOrEmpty())
                {
                    querystring = @"SELECT u.Id, u.Email, u.EmailConfirmed, u.PasswordHash, u.SecurityStamp,
                                       u.PhoneNumber, u.PhoneNumberConfirmed, u.TwoFactorEnabled,
                                       u.TwoFactorEnabled, u.LockoutEnabled, u.AccessFailedCount,
                                       u.UserName, u.PhoneVerificationCode, u.FirstName, u.LastName,
                                       u.IsActive, u.LastLogin, u.LockoutEndDateUtc, u.CreatedDate
                                    FROM aspnetusers u
                                    INNER JOIN aspnetuserroles ur ON u.Id = ur.userid
                                    INNER JOIN ChurchUsers cu ON cu.UserId = u.Id
                                    WHERE ur.roleid = @RoleId AND cu.ChurchId = @ChurchId
                                    ORDER BY u.firstname";
                }
                else
                {
                    querystring = @"SELECT u.Id, u.Email, u.EmailConfirmed, u.PasswordHash, u.SecurityStamp,
                                       u.PhoneNumber, u.PhoneNumberConfirmed, u.TwoFactorEnabled,
                                       u.TwoFactorEnabled, u.LockoutEnabled, u.AccessFailedCount,
                                       u.UserName, u.PhoneVerificationCode, u.FirstName, u.LastName,
                                       u.IsActive, u.LastLogin, u.LockoutEndDateUtc, u.CreatedDate
                                    FROM aspnetusers u
                                    INNER JOIN aspnetuserroles ur ON u.Id = ur.userid
                                    WHERE ur.roleid = @RoleId
                                    ORDER BY u.firstname";
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(querystring, connection))
                {
                    cmd.Parameters.AddWithValue("@RoleId", roleId);

                    if (churchId.IsNotNullOrEmpty())
                    {
                        cmd.Parameters.AddWithValue("@ChurchId", churchId);
                    }

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                result.Add(MapToApplicationUser(reader));
                            }
                            catch (Exception ex)
                            {
                                ExceptionLogger.LogException(ex);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        private ApplicationUser MapToApplicationUser(SqlDataReader reader)
        {
            return new ApplicationUser
            {
                Id = reader["Id"].ToString(),
                Email = reader["Email"].ToString(),
                EmailConfirmed = GetBoolean(reader, "EmailConfirmed"),
                PasswordHash = reader["PasswordHash"].ToString(),
                SecurityStamp = reader["SecurityStamp"].ToString(),
                PhoneNumber = reader["PhoneNumber"].ToString(),
                PhoneNumberConfirmed = GetBoolean(reader, "PhoneNumberConfirmed"),
                TwoFactorEnabled = GetBoolean(reader, "TwoFactorEnabled"),
                LockoutEnabled = GetBoolean(reader, "LockoutEnabled"),
                AccessFailedCount = Convert.ToInt32(reader["AccessFailedCount"]),
                UserName = reader["UserName"].ToString(),
                PhoneVerificationCode = reader["PhoneVerificationCode"].ToString(),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                IsActive = GetBoolean(reader, "IsActive"),
                LastLogin = GetDateTime(reader, "LastLogin"),
                LockoutEndDateUtc = GetDateTime(reader, "LockoutEndDateUtc"),
                CreatedDate = GetDateTime(reader, "CreatedDate")
            };
        }

        #region Delete User Data
        public Result<string> DeleteUserAndRelatedData(string userId)
        {
            try
            {
                //Delete person-related data first
                var personId = GetPersonIdByUserId(userId);

                if (!string.IsNullOrEmpty(personId))
                {
                    DeleteChurchPersonAndPerson(personId);
                }

                var constraints = GetConstraintsRelatedToUserId();

                // Disable constraints
                var disableConstraintsSql = GenerateDisableConstraintsSql(constraints);
                ExecuteNonQuery(disableConstraintsSql);

                // Delete child data
                var deleteChildDataSql = GenerateDeleteChildDataSql(userId);
                ExecuteNonQuery(deleteChildDataSql);

                // Delete parent data
                var deleteParentDataSql = GenerateDeleteParentDataSql(userId);
                ExecuteNonQuery(new List<string> { deleteParentDataSql });

                // Re-enable constraints
                var enableConstraintsSql = GenerateEnableConstraintsSql(constraints);
                ExecuteNonQuery(enableConstraintsSql);

                return Result<string>.Success("User and related data deleted successfully.");
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return Result<string>.FromException(ex, "An exception occurred while processing your request.", null);
            }
        }

        private void ExecuteNonQuery(IEnumerable<string> sqlCommands)
        {
            using (var sqlCnn = new SqlConnection(connectionString))
            {
                sqlCnn.Open();
                foreach (var sql in sqlCommands)
                {
                    using (var cmd = new SqlCommand(sql, sqlCnn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<(string ConstraintName, string TableName)> GetConstraintsRelatedToUserId()
        {
            var constraints = new List<(string ConstraintName, string TableName)>();

            const string query = @"
                SELECT 
                    fk.name AS ConstraintName,
                    tp.name AS TableName
                FROM 
                    sys.foreign_key_columns fkc
                    INNER JOIN sys.foreign_keys fk ON fkc.constraint_object_id = fk.object_id
                    INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
                    INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
                WHERE 
                    c.name = 'UserId';
            ";

            using (var sqlCnn = new SqlConnection(connectionString))
            {
                sqlCnn.Open();
                using (var cmd = new SqlCommand(query, sqlCnn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            constraints.Add((
                                ConstraintName: reader.GetString(0),
                                TableName: reader.GetString(1)
                            ));
                        }
                    }
                }
            }

            return constraints;
        }

        public List<string> GenerateDisableConstraintsSql(List<(string ConstraintName, string TableName)> constraints)
        {
            var sqlCommands = new List<string>();

            foreach (var (constraintName, tableName) in constraints)
            {
                var sql = $"ALTER TABLE {tableName} NOCHECK CONSTRAINT {constraintName};";
                sqlCommands.Add(sql);
            }

            return sqlCommands;
        }

        public List<string> GenerateDeleteChildDataSql(string userId)
        {
            var sqlCommands = new List<string>();

            // Implement this method similarly to get table names with UserId column.
            foreach (var tableName in GetTablesWithUserId())
            {
                var sql = $"DELETE FROM {tableName} WHERE UserId = '{userId}';";
                sqlCommands.Add(sql);
            }

            return sqlCommands;
        }

        private List<string> GetTablesWithUserId()
        {
            var tables = new List<string>();

            const string query = @"
                SELECT DISTINCT 
                    tp.name AS TableName
                FROM 
                    sys.columns c
                    INNER JOIN sys.tables tp ON c.object_id = tp.object_id
                WHERE 
                    c.name = 'UserId';
            ";

            using (var sqlCnn = new SqlConnection(connectionString))
            {
                sqlCnn.Open();
                using (var cmd = new SqlCommand(query, sqlCnn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return tables;
        }

        public string GenerateDeleteParentDataSql(string userId)
        {
            return $"DELETE FROM AspNetUsers WHERE Id = '{userId}';";
        }

        public List<string> GenerateEnableConstraintsSql(List<(string ConstraintName, string TableName)> constraints)
        {
            var sqlCommands = new List<string>();

            foreach (var (constraintName, tableName) in constraints)
            {
                var sql = $"ALTER TABLE {tableName} WITH CHECK CHECK CONSTRAINT {constraintName};";
                sqlCommands.Add(sql);
            }

            return sqlCommands;
        }

        //// TODO
        //public bool HasAssociatedData(string userId)
        //{
        //    // Add logic to check if the user has associated data
        //    // For example, check related tables for any records associated with the user
        //    return false; // Placeholder, replace with actual implementation
        //}

        public bool HasAssociatedData(string userId)
        {
            // List to store tables where UserId is found
            var tablesWithUserId = new List<string>();

            // Step 1: Find all tables with a UserId column
            const string tablesQuery = @"
                SELECT DISTINCT 
                    t.name AS TableName
                FROM 
                    sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                WHERE 
                    c.name = 'UserId';
            ";

            // Find tables
            using (var sqlCnn = new SqlConnection(connectionString))
            {
                sqlCnn.Open();
                using (var cmd = new SqlCommand(tablesQuery, sqlCnn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tablesWithUserId.Add(reader.GetString(0));
                        }
                    }
                }
            }

            // Step 2: Check if the userId exists in any of those tables
            foreach (var table in tablesWithUserId)
            {
                var checkQuery = $"SELECT COUNT(1) FROM {table} WHERE UserId = @UserId";

                using (var sqlCnn = new SqlConnection(connectionString))
                {
                    sqlCnn.Open();
                    using (var cmd = new SqlCommand(checkQuery, sqlCnn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        var count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return true; // Found associated data
                        }
                    }
                }
            }

            return false; // No associated data found
        }

        public string GetPersonIdByUserId(string userId)
        {
            string personId = null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string querystring = "SELECT TypeId FROM AspNetUsers WHERE Id = @UserId";
                    using (var cmd = new SqlCommand(querystring, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("TypeId")))
                            {
                                personId = reader.GetString(reader.GetOrdinal("TypeId"));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return personId ?? string.Empty;
        }

        public void DeleteChurchPersonAndPerson(string typeId)
        {
            ExecuteNonQuery("DELETE FROM ChurchPeople WHERE PersonId = @TypeId; DELETE FROM People WHERE Id = @TypeId", ("@TypeId", typeId));
        }

        #endregion

        #endregion

        #region Church Users
        public List<UsersWithRoles> GetAllChurchUsersByRole(string churchId, string role)
        {
            var result = new List<UsersWithRoles>();

            try
            {
                const string query = "SELECT * FROM aspnetusers u " +
                               "INNER JOIN churchusers cu ON u.id = cu.userid " +
                               "INNER JOIN aspnetuserroles ur ON u.id = ur.userid " +
                               "INNER JOIN aspnetroles r ON ur.roleid = r.id " +
                               "WHERE cu.churchid = @ChurchId AND (r.name = @Role AND (r.churchid IS NULL OR r.churchid = @ChurchId)) " +
                               "ORDER BY u.firstname";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ChurchId", churchId);
                        cmd.Parameters.AddWithValue("@Role", role);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var user = new UsersWithRoles
                                    {
                                        Id = reader["Id"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        EmailConfirmed = Convert.ToBoolean(reader["EmailConfirmed"]),
                                        PasswordHash = reader["PasswordHash"].ToString(),
                                        SecurityStamp = reader["SecurityStamp"].ToString(),
                                        PhoneNumber = reader["PhoneNumber"].ToString(),
                                        PhoneNumberConfirmed = Convert.ToBoolean(reader["PhoneNumberConfirmed"]),
                                        TwoFactorEnabled = Convert.ToBoolean(reader["TwoFactorEnabled"]),
                                        LockoutEnabled = Convert.ToBoolean(reader["LockoutEnabled"]),
                                        AccessFailedCount = Convert.ToInt32(reader["AccessFailedCount"]),
                                        UserName = reader["UserName"].ToString(),
                                        PhoneVerificationCode = reader["PhoneVerificationCode"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        LastName = reader["LastName"].ToString(),
                                        IsActive = Convert.ToBoolean(reader["IsActive"])
                                    };

                                    if (reader["LastLogin"] != DBNull.Value)
                                    {
                                        user.LastLogin = Convert.ToDateTime(reader["LastLogin"]);
                                    }

                                    if (reader["LockoutEndDateUtc"] != DBNull.Value)
                                    {
                                        user.LockoutEndDateUtc = Convert.ToDateTime(reader["LockoutEndDateUtc"]);
                                    }

                                    result.Add(user);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionLogger.LogException(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.FullName).ToList();
            }

            return result;
        }

        public List<UsersWithRoles> GetAllChurchUsersByRoles(string churchId, List<string> roles)
        {
            var result = new List<UsersWithRoles>();

            try
            {
                string rolesCondition = string.Join("','", roles);
                const string querystring = "SELECT * FROM aspnetusers u " +
                                     "INNER JOIN churchusers cu ON u.id = cu.userid " +
                                     "INNER JOIN aspnetuserroles ur ON u.id = ur.userid " +
                                     "INNER JOIN aspnetroles r ON ur.roleid = r.id " +
                                     "WHERE cu.churchid = @ChurchId AND (r.name IN (@Roles) AND (r.churchid IS NULL OR r.churchid = @ChurchId)) " +
                                     "ORDER BY u.firstname";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(querystring, connection))
                    {
                        cmd.Parameters.AddWithValue("@ChurchId", churchId);
                        cmd.Parameters.AddWithValue("@Roles", rolesCondition);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var user = new UsersWithRoles
                                    {
                                        Id = reader["Id"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        EmailConfirmed = Convert.ToBoolean(reader["EmailConfirmed"]),
                                        PasswordHash = reader["PasswordHash"].ToString(),
                                        SecurityStamp = reader["SecurityStamp"].ToString(),
                                        PhoneNumber = reader["PhoneNumber"].ToString(),
                                        PhoneNumberConfirmed = Convert.ToBoolean(reader["PhoneNumberConfirmed"]),
                                        TwoFactorEnabled = Convert.ToBoolean(reader["TwoFactorEnabled"]),
                                        LockoutEnabled = Convert.ToBoolean(reader["LockoutEnabled"]),
                                        AccessFailedCount = Convert.ToInt32(reader["AccessFailedCount"]),
                                        UserName = reader["UserName"].ToString(),
                                        PhoneVerificationCode = reader["PhoneVerificationCode"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        LastName = reader["LastName"].ToString(),
                                        IsActive = Convert.ToBoolean(reader["IsActive"])
                                    };

                                    if (reader["LastLogin"] != DBNull.Value)
                                    {
                                        user.LastLogin = Convert.ToDateTime(reader["LastLogin"]);
                                    }

                                    if (reader["LockoutEndDateUtc"] != DBNull.Value)
                                    {
                                        user.LockoutEndDateUtc = Convert.ToDateTime(reader["LockoutEndDateUtc"]);
                                    }

                                    result.Add(user);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionLogger.LogException(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.FullName).ToList();
            }

            return result;
        }
        #endregion

        #region Permissions
        public bool DeletePermissionsByRoleId(string roleId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "DELETE FROM permissions WHERE Type = @Type AND TypeId = @TypeId";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Type", "Role");
                        cmd.Parameters.AddWithValue("@TypeId", roleId);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return false;
            }
        }

        public List<Permissions> ReadPermissionsByModulesAndUserId(List<Modules> modules, List<ApplicationRoles> roles, string userId)
        {
            var result = new List<Permissions>();

            try
            {
                // Extract module IDs from the provided modules
                var moduleIds = modules.Select(x => x.Id).ToList();

                // Retrieve permissions based on the module IDs
                var permissions = db.Permissions.Where(x => moduleIds.Contains(x.ModuleId)).ToList();

                // Extract role IDs from the provided roles
                var roleIds = roles.Select(x => x.Id).ToList();

                // Filter permissions by roles and user ID
                var permissionsByRole = permissions
                    .Where(x => x.Type == PermissionType.Role.ToString() && roleIds.Contains(x.TypeId))
                    .ToList();

                var permissionsByUser = permissions
                    .Where(x => x.Type == PermissionType.User.ToString() && x.TypeId == userId)
                    .ToList();

                // Remove duplicate permissions obtained from both roles and user ID
                permissionsByRole.RemoveAll(r => permissionsByUser.Any(u => u.ModuleId == r.ModuleId));

                // Combine permissions from roles and user ID
                result.AddRange(permissionsByRole);
                result.AddRange(permissionsByUser);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return result;
        }
        #endregion

        #region Role Permissions
        public bool DeleteRolesPermissionsByPermissionId(string permissionId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = "DELETE FROM RolesPermissions WHERE PermissionId = @PermissionId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PermissionId", permissionId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                return false;
            }
        }
        #endregion

        #region User Settings
        public UserSetting ReadUserSettingsByUserId(string userId)
        {
            UserSetting userSettings = null;

            try
            {
                using (var sqlCnn = new SqlConnection(connectionString))
                {
                    sqlCnn.Open();
                    const string querystring = "SELECT * FROM UserSettings WHERE UserId = @UserId";
                    var cmd = new SqlCommand(querystring, sqlCnn);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userSettings = new UserSetting
                            {
                                Id = reader["Id"].ToString(),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                DarkModeEnabled = Convert.ToBoolean(reader["DarkModeEnabled"]),
                                ModifiedBy = reader["ModifiedBy"].ToString(),
                                PrimaryChurchId = reader["PrimaryChurchId"].ToString(),
                                PrimaryChurchCampusId = reader["PrimaryChurchCampusId"].ToString(),
                                ProfileImage = reader["ProfileImage"].ToString(),
                                UserId = reader["UserId"].ToString()
                            };

                            var modifiedDate = reader["ModifiedDate"].ToString();
                            if (!string.IsNullOrEmpty(modifiedDate))
                            {
                                userSettings.ModifiedDate = Convert.ToDateTime(modifiedDate);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return userSettings;
        }
        #endregion

        #region Reports and DataTables
        public DataTable ReadViaQuery(string query)
        {
            var table = new DataTable();

            try
            {
                using (var sqlCnn = new SqlConnection(connectionString))
                {
                    sqlCnn.Open();
                    var cmd = new SqlCommand(query, sqlCnn);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(table);
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return table;
        }

        public List<DBSchemaModel> ReadDbSchema()
        {
            var result = new List<DBSchemaModel>();

            try
            {
                using (var sqlCnn = new SqlConnection(connectionString))
                {
                    sqlCnn.Open();
                    var dataTable = sqlCnn.GetSchema("Tables");
                    foreach (var item in dataTable.Rows.Cast<DataRow>().ToList())
                    {
                        var table = item.ItemArray[2].ToString();
                        var columns = GetColumnNames(connectionString, table);
                        result.Add(new DBSchemaModel { Value = columns.ToArray(), Text = table });
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        private List<string> GetColumnNames(string conStr, string tableName)
        {
            var result = new List<string>();

            try
            {
                using (var sqlCon = new SqlConnection(conStr))
                {
                    sqlCon.Open();
                    var sqlCmd = sqlCon.CreateCommand();
                    sqlCmd.CommandText = "select * from " + tableName + " where 1=0";
                    sqlCmd.CommandType = CommandType.Text;

                    using (var reader = sqlCmd.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        var dataTable = reader.GetSchemaTable();
                        result.AddRange(from DataRow row in dataTable.Rows select row.Field<string>("ColumnName"));
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }

        public Report GetReport(string reportId)
        {
            var result = new Report();

            try
            {
                using (var sqlCnn = new SqlConnection(connectionString))
                {
                    sqlCnn.Open();
                    const string querystring = "SELECT Id, Name, Description, StartDate, EndDate FROM reports WHERE Id = @ReportId";
                    var cmd = new SqlCommand(querystring, sqlCnn);
                    cmd.Parameters.AddWithValue("@ReportId", reportId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.Id = reader["Id"].ToString();
                            result.Name = reader["Name"].ToString();
                            result.Description = reader["Description"].ToString();

                            var startDate = reader["StartDate"].ToString();
                            if (!string.IsNullOrEmpty(startDate))
                            {
                                result.StartDate = Convert.ToDateTime(startDate);
                            }

                            var endDate = reader["EndDate"].ToString();
                            if (!string.IsNullOrEmpty(endDate))
                            {
                                result.EndDate = Convert.ToDateTime(endDate);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
            }

            return result;
        }
        #endregion
    }
}