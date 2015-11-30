﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2014-08-18
// Last Modified:			2015-11-18
// 


using cloudscribe.Core.Models;
using cloudscribe.Core.Models.DataExtensions;
using cloudscribe.DbHelpers.MSSQL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace cloudscribe.Core.Repositories.MSSQL
{
    public sealed class UserRepository : IUserRepository
    {
        
        public UserRepository(
            IOptions<MSSQLConnectionOptions> connectionOptions,
            ILoggerFactory loggerFactory)
        {
            if (connectionOptions == null) { throw new ArgumentNullException(nameof(connectionOptions)); }
            if (loggerFactory == null) { throw new ArgumentNullException(nameof(loggerFactory)); }
            
            logFactory = loggerFactory;
            log = loggerFactory.CreateLogger(typeof(UserRepository).FullName);

            readConnectionString = connectionOptions.Value.ReadConnectionString;
            writeConnectionString = connectionOptions.Value.WriteConnectionString;

            dbSiteUser = new DBSiteUser(readConnectionString, writeConnectionString, logFactory);
            dbUserLogins = new DBUserLogins(readConnectionString, writeConnectionString, logFactory);
            dbUserClaims = new DBUserClaims(readConnectionString, writeConnectionString, logFactory);
            dbUserLocation = new DBUserLocation(readConnectionString, writeConnectionString, logFactory);
            dbRoles = new DBRoles(readConnectionString, writeConnectionString, logFactory);
        }

        
        private ILoggerFactory logFactory;
        private ILogger log;
        private string readConnectionString;
        private string writeConnectionString;
        private DBSiteUser dbSiteUser;
        private DBUserLogins dbUserLogins;
        private DBUserClaims dbUserClaims;
        private DBUserLocation dbUserLocation;
        private DBRoles dbRoles;

        #region User 

        public async Task<bool> Save(ISiteUser user)
        {
            if (user.SiteId == -1) { throw new ArgumentException("user must have a siteid"); }
            if (user.SiteGuid == Guid.Empty) { throw new ArgumentException("user must have a siteguid"); }

            if (user.UserId == -1)
            {
                user.UserGuid = Guid.NewGuid();
                user.CreatedUtc = DateTime.UtcNow;

                user.UserId = await dbSiteUser.AddUser(
                    user.SiteGuid,
                    user.SiteId,
                    user.DisplayName,
                    user.UserName,
                    user.Email,
                    user.UserGuid,
                    user.CreatedUtc,
                    user.MustChangePwd,
                    user.FirstName,
                    user.LastName,
                    user.TimeZoneId,
                    user.DateOfBirth,
                    user.EmailConfirmed,
                    user.PasswordHash,
                    user.SecurityStamp,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEndDateUtc,
                    user.AccountApproved,
                    user.IsLockedOut,
                    user.DisplayInMemberList,
                    user.WebSiteUrl,
                    user.Country,
                    user.State,
                    user.AvatarUrl,
                    user.Signature,
                    user.AuthorBio,
                    user.Comment
                    );

                return user.UserId > -1;
            }
            else
            {
                return await Update(user);
            }

            
            

        }

        private async Task<bool> Update(ISiteUser user)
        {
            if (string.IsNullOrEmpty(user.LoweredEmail)) { user.LoweredEmail = user.Email.ToLowerInvariant(); }

            return await dbSiteUser.UpdateUser(
                    user.UserId,
                    user.DisplayName,
                    user.UserName,
                    user.Email,
                    user.Gender,
                    user.AccountApproved,
                    user.Trusted,
                    user.DisplayInMemberList,
                    user.WebSiteUrl,
                    user.Country,
                    user.State,
                    user.AvatarUrl,
                    user.Signature,
                    user.LoweredEmail,
                    user.Comment,
                    user.MustChangePwd,
                    user.FirstName,
                    user.LastName,
                    user.TimeZoneId,
                    user.NewEmail,
                    user.EmailChangeGuid,
                    user.PasswordResetGuid,
                    user.RolesChanged,
                    user.AuthorBio,
                    user.DateOfBirth,
                    user.EmailConfirmed,
                    user.PasswordHash,
                    user.SecurityStamp,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEndDateUtc,
                    user.IsLockedOut
                    );

        }




        /// <summary>
        /// Deletes an instance of User. Returns true on success.
        /// </summary>
        /// <param name="userID"> userID </param>
        /// <returns>bool</returns>
        public async Task<bool> Delete(int userId)
        {
            return await dbSiteUser.DeleteUser(userId);
        }

        public async Task<bool> FlagAsDeleted(int userId)
        {
            return await dbSiteUser.FlagAsDeleted(userId);
        }

        public async Task<bool> FlagAsNotDeleted(int userId)
        {
            return await dbSiteUser.FlagAsNotDeleted(userId);
        }

        
        public async Task<bool> ConfirmRegistration(Guid registrationGuid)
        {
            if (registrationGuid == Guid.Empty)
            {
                return false;
            }

            return await dbSiteUser.ConfirmRegistration(Guid.Empty, registrationGuid);
        }


        public async Task<bool> LockoutAccount(Guid userGuid)
        {
            return await dbSiteUser.AccountLockout(userGuid, DateTime.UtcNow);
        }

        public async Task<bool> UnLockAccount(Guid userGuid)
        {
            return await dbSiteUser.AccountClearLockout(userGuid);
        }

        public async Task<bool> UpdateFailedPasswordAttemptCount(Guid userGuid, int failedPasswordAttemptCount)
        {
            return await dbSiteUser.UpdateFailedPasswordAttemptCount(userGuid, failedPasswordAttemptCount);
        }

        

        public int GetCount(int siteId)
        {
            
            return dbSiteUser.UserCount(siteId);
        }

        //public int UserCount(int siteId, String userNameBeginsWith)
        //{
        //    if (config.UseRelatedSiteMode()) { siteId = config.RelatedSiteId(); }
        //    return DBSiteUser.UserCount(siteId, userNameBeginsWith);
        //}

        //public int UsersOnlineSinceCount(int siteId, DateTime sinceTime)
        //{
        //    if (multiTenantOptions.UseRelatedSitesMode) { siteId = multiTenantOptions.RelatedSiteId; }
        //    return dbSiteUser.CountOnlineSince(siteId, sinceTime);
        //}


        //public async Task<ISiteUser> FetchNewest(int siteId)
        //{
        //    if (multiTenantOptions.UseRelatedSitesMode) { siteId = multiTenantOptions.RelatedSiteId; }

        //    int newestUserId = await GetNewestUserId(siteId);
        //    return await Fetch(siteId, newestUserId);
        //}

        public async Task<ISiteUser> Fetch(int siteId, int userId)
        {
            using (DbDataReader reader = await dbSiteUser.GetSingleUser(userId))
            {
                if (reader.Read())
                {
                    SiteUser user = new SiteUser();

                    user.LoadFromReader(reader);

                    if (user.SiteId == siteId) { return user; }

                }
            }

            return null;
        }


        public async Task<ISiteUser> Fetch(int siteId, Guid userGuid)
        { 
            using (DbDataReader reader = await dbSiteUser.GetSingleUser(userGuid))
            {
                if (reader.Read())
                {
                    SiteUser user = new SiteUser();

                    user.LoadFromReader(reader);

                    if (user.SiteId == siteId) { return user; }

                }
            }

            return null;
        }

        public async Task<ISiteUser> FetchByConfirmationGuid(int siteId, Guid confirmGuid)
        {  
            using (DbDataReader reader = await dbSiteUser.GetUserByRegistrationGuid(siteId, confirmGuid))
            {
                if (reader.Read())
                {
                    SiteUser user = new SiteUser();

                    user.LoadFromReader(reader);

                    if (user.SiteId == siteId) { return user; }

                }
            }

            return null;
        }


        public async Task<ISiteUser> Fetch(int siteId, string email)
        { 
            using (DbDataReader reader = await dbSiteUser.GetSingleUser(siteId, email))
            {
                if (reader.Read())
                {
                    SiteUser user = new SiteUser();

                    user.LoadFromReader(reader);

                    return user;

                }
            }

            return null;
        }

        public async Task<ISiteUser> FetchByLoginName(int siteId, string userName, bool allowEmailFallback)
        {
            using (DbDataReader reader = await dbSiteUser.GetSingleUserByLoginName(siteId, userName, allowEmailFallback))
            {
                if (reader.Read())
                {
                    SiteUser user = new SiteUser();

                    user.LoadFromReader(reader);

                    return user;

                }
            }

            return null;
        }

        
        public async Task<List<IUserInfo>> GetByIPAddress(Guid siteGuid, string ipv4Address)
        {
            List<IUserInfo> userList = new List<IUserInfo>();
            
            using (DbDataReader reader = await dbUserLocation.GetUsersByIPAddress(siteGuid, ipv4Address))
            {
                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    userList.Add(user);

                }
            }

            return userList;

        }

        public async Task<List<IUserInfo>> GetCrossSiteUserListByEmail(string email)
        {
            List<IUserInfo> userList = new List<IUserInfo>();

            using (DbDataReader reader = await dbSiteUser.GetCrossSiteUserListByEmail(email))
            {
                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    userList.Add(user);

                }
            }

            return userList;

        }

        public async Task<int> CountUsers(int siteId, String userNameBeginsWith)
        {
            return await dbSiteUser.CountUsers(siteId, userNameBeginsWith);
        }

        public async Task<List<IUserInfo>> GetPage(
            int siteId,
            int pageNumber,
            int pageSize,
            string userNameBeginsWith,
            int sortMode)
        {
            //sortMode: 0 = DisplayName asc, 1 = JoinDate desc, 2 = Last, First

            List<IUserInfo> userList = new List<IUserInfo>();

            using (DbDataReader reader = await dbSiteUser.GetUserListPage(
                    siteId, pageNumber, pageSize, userNameBeginsWith, sortMode))
            {

                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    userList.Add(user);
                    //totalPages = Convert.ToInt32(reader["TotalPages"]);
                }
            }

            return userList;

        }

        //public async Task<int> CountUsersForSearch(int siteId, string searchInput)
        //{
        //    return await dbSiteUser.CountUsersForSearch(siteId, searchInput);
        //}

        //public async Task<List<IUserInfo>> GetUserSearchPage(
        //    int siteId,
        //    int pageNumber,
        //    int pageSize,
        //    string searchInput,
        //    int sortMode)
        //{
        //    //sortMode: 0 = DisplayName asc, 1 = JoinDate desc, 2 = Last, First

        //    List<IUserInfo> userList = new List<IUserInfo>();

        //    using (DbDataReader reader = await dbSiteUser.GetUserSearchPage(
        //        siteId,
        //        pageNumber,
        //        pageSize,
        //        searchInput,
        //        sortMode))
        //    {

        //        while (reader.Read())
        //        {
        //            UserInfo user = new UserInfo();
        //            user.LoadFromReader(reader);
        //            userList.Add(user);

        //        }
        //    }

        //    return userList;

        //}

        public async Task<int> CountUsersForAdminSearch(int siteId, string searchInput)
        {
            return await dbSiteUser.CountUsersForAdminSearch(siteId, searchInput);
        }

        public async Task<List<IUserInfo>> GetUserAdminSearchPage(
            int siteId,
            int pageNumber,
            int pageSize,
            string searchInput,
            int sortMode)
        { 
            List<IUserInfo> userList = new List<IUserInfo>();

            using (DbDataReader reader = await dbSiteUser.GetUserAdminSearchPage(
                siteId,
                pageNumber,
                pageSize,
                searchInput,
                sortMode))
            {
                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    userList.Add(user);
                }
            }

            return userList;
        }

        public async Task<int> CountLockedOutUsers(int siteId)
        {
            return await dbSiteUser.CountLockedOutUsers(siteId);
        }

        public async Task<List<IUserInfo>> GetPageLockedUsers(
            int siteId,
            int pageNumber,
            int pageSize)
        {
            List<IUserInfo> userList = new List<IUserInfo>();

            using (DbDataReader reader = await dbSiteUser.GetPageLockedUsers(
                siteId,
                pageNumber,
                pageSize))
            {

                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    userList.Add(user);

                }
            }

            return userList;
        }

        public async Task<int> CountNotApprovedUsers(int siteId)
        {
            return await dbSiteUser.CountNotApprovedUsers(siteId);
        }

        public async Task<List<IUserInfo>> GetNotApprovedUsers(
            int siteId,
            int pageNumber,
            int pageSize)
        {
            List<IUserInfo> userList = new List<IUserInfo>();

            using (DbDataReader reader = await dbSiteUser.GetPageNotApprovedUsers(
                siteId,
                pageNumber,
                pageSize))
            {

                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    userList.Add(user);

                }
            }

            return userList;
        }

        //public IDataReader GetSmartDropDownData(int siteId, string query, int rowsToGet)
        //{
        //    //if (UseRelatedSiteMode) { siteId = RelatedSiteID; }

        //    return DBSiteUser.GetSmartDropDownData(siteId, query, rowsToGet);
        //}

        //public IDataReader EmailLookup(int siteId, string query, int rowsToGet)
        //{
        //    //if (UseRelatedSiteMode) { siteId = RelatedSiteID; }

        //    return DBSiteUser.EmailLookup(siteId, query, rowsToGet);
        //}

        public async Task<bool> EmailExistsInDB(int siteId, string email)
        {
            bool found = false;

            using (DbDataReader r = await dbSiteUser.GetSingleUser(siteId, email))
            {
                while (r.Read()) { found = true; }
            }
            return found;
        }

        public async Task<bool> EmailExistsInDB(int siteId, int userId, string email)
        {
            bool found = false;

            using (DbDataReader r = await dbSiteUser.GetSingleUser(siteId, email))
            {
                while (r.Read())
                {
                    int foundId = Convert.ToInt32(r["UserID"]);
                    found = (foundId != userId);
                    if (found) { return found; }
                }
            }
            return found;
        }

        public bool LoginExistsInDB(int siteId, string loginName)
        {
            bool found = false;

            using (DbDataReader r = dbSiteUser.GetSingleUserByLoginNameNonAsync(siteId, loginName, false))
            {
                while (r.Read()) { found = true; }
            }

            return found;
        }

        /// <summary>
        /// available only if the found user matches the passed in one
        /// or if not found
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public async Task<bool> LoginIsAvailable(int siteId, int userId, string loginName)
        {
            bool available = true;

            using (DbDataReader r = await dbSiteUser.GetSingleUserByLoginName(siteId, loginName, false))
            {
                while (r.Read())
                {
                    int foundId = Convert.ToInt32(r["UserID"]);

                    available = (foundId == userId);
                    if (!available) { return available; }
                }
            }

            return available;
        }

        public async Task<string> GetUserNameFromEmail(int siteId, string email)
        {
            string result = String.Empty;
            if ((email != null) && (email.Length > 0) && (siteId > 0))
            {
                string comma = String.Empty;
                using (DbDataReader reader = await dbSiteUser.GetSingleUser(siteId, email))
                {
                    while (reader.Read())
                    {
                        result += comma + reader["LoginName"].ToString();
                        comma = ", ";

                    }
                }
            }

            return result;

        }



        //public async Task<int> GetNewestUserId(int siteId)
        //{
        //    return await dbSiteUser.GetNewestUserId(siteId);
        //}



        #endregion

        #region Roles

        /// <summary>
        /// Persists a new instance of Role. Returns true on success.
        /// when a role is created displayname corresponds to rolename
        /// but rolename can never change since it is used in a cookies and coded
        /// into security checks in some cases
        /// so subsequent changes to rolename really only effect displayname
        /// ie for localization or customization
        /// to really change a rolename you can delete the role and create a new one with the desired name
        /// some specific required system roles (Admin, Content Administrators) 
        /// are also not allowed to be deleted
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveRole(ISiteRole role)
        {
            if (role.SiteId == -1) { throw new ArgumentException("SiteId must be provided"); }
            if (role.SiteGuid == Guid.Empty) { throw new ArgumentException("SiteGuid must be provided"); }

            if (role.RoleId == -1) // new role
            {
                bool exists = await RoleExists(role.SiteId, role.DisplayName);
                if (exists)
                {
                    log.LogError("attempt to create a duplicate role "
                        + role.DisplayName + " for site "
                        + role.SiteId.ToString());

                    return false;
                }

                role.RoleGuid = Guid.NewGuid();

                role.RoleId = await dbRoles.RoleCreate(
                    role.RoleGuid,
                    role.SiteGuid,
                    role.SiteId,
                    role.DisplayName
                    );

                role.RoleName = role.DisplayName;

                return (role.RoleId > -1);
            }
            else
            {
                return await dbRoles.Update(
                    role.RoleId,
                    role.DisplayName);

            }

        }



        public async Task<bool> DeleteRole(int roleId)
        {
            return await dbRoles.Delete(roleId);
        }

        public async Task<bool> AddUserToRole(
            int roleId,
            Guid roleGuid,
            int userId,
            Guid userGuid
            )
        {
            return await dbRoles.AddUser(roleId, userId, roleGuid, userGuid);
        }

        public async Task<bool> RemoveUserFromRole(int roleId, int userId)
        {
            return await dbRoles.RemoveUser(roleId, userId);
        }



        //public async Task<bool> AddUserToDefaultRoles(ISiteUser siteUser)
        //{
        //    // moved this to the config setting below instead of hard coded
        //    //IRole role = Fetch(siteUser.SiteId, "Authenticated Users");
        //    //if (role.RoleID > -1)
        //    //{
        //    //    AddUser(role.RoleID, role.RoleGuid, siteUser.UserId, siteUser.UserGuid);
        //    //}

        //    ISiteRole role;
        //    bool result = true;
        //    string defaultRoles = config.DefaultRolesForNewUsers();

        //    if (defaultRoles.Length > 0)
        //    {
        //        if (defaultRoles.IndexOf(";") == -1)
        //        {
        //            role = await FetchRole(siteUser.SiteId, defaultRoles);
        //            if ((role != null) && (role.RoleId > -1))
        //            {
        //                result = await AddUserToRole(role.RoleId, role.RoleGuid, siteUser.UserId, siteUser.UserGuid);
        //            }
        //        }
        //        else
        //        {
        //            string[] roleArray = defaultRoles.Split(';');
        //            foreach (string roleName in roleArray)
        //            {
        //                if (!string.IsNullOrEmpty(roleName))
        //                {
        //                    role = await FetchRole(siteUser.SiteId, roleName);
        //                    if ((role != null) && (role.RoleId > -1))
        //                    {
        //                        result = result && await AddUserToRole(role.RoleId, role.RoleGuid, siteUser.UserId, siteUser.UserGuid);
        //                    }
        //                }
        //            }

        //        }

        //    }

        //    return result;
        //}

        public async Task<bool> DeleteUserRoles(int userId)
        {
            return await dbRoles.DeleteUserRoles(userId);
        }

        public async Task<bool> DeleteUserRolesByRole(int roleId)
        {
            return await dbRoles.DeleteUserRolesByRole(roleId);
        }


        public async Task<bool> RoleExists(int siteId, String roleName)
        {
            return await dbRoles.Exists(siteId, roleName);
        }

        //public int GetRoleMemberCount(int roleId)
        //{
        //    // TODO: implement actual select count from db
        //    // this is works but is not ideal
        //    int count = 0;
        //    using (DbDataReader reader = dbRoles.GetRoleMembers(roleId))
        //    {
        //        while (reader.Read())
        //        {
        //            count += 1;
        //        }
        //    }

        //    return count;

        //}

        public async Task<ISiteRole> FetchRole(int roleId)
        {
            using (DbDataReader reader = await dbRoles.GetById(roleId))
            {
                if (reader.Read())
                {
                    SiteRole role = new SiteRole();
                    role.LoadFromReader(reader);
                    return role;
                }
            }

            return null;
        }

        public async Task<ISiteRole> FetchRole(int siteId, string roleName)
        {
            SiteRole role = null;

            using (DbDataReader reader = await dbRoles.GetByName(siteId, roleName))
            {
                if (reader.Read())
                {
                    role = new SiteRole();
                    role.LoadFromReader(reader);
                }
            }

            return role;

        }

        public async Task<List<string>> GetUserRoles(int siteId, int userId)
        {
            List<string> userRoles = new List<string>();
            using (DbDataReader reader = await dbSiteUser.GetRolesByUser(siteId, userId))
            {
                while (reader.Read())
                {
                    userRoles.Add(reader["RoleName"].ToString());
                }

            }

            return userRoles;
        }

        public async Task<int> CountOfRoles(int siteId, string searchInput)
        {
            return await dbRoles.GetCountOfSiteRoles(siteId, searchInput);
        }

        public async Task<IList<ISiteRole>> GetRolesBySite(
            int siteId,
            string searchInput,
            int pageNumber,
            int pageSize)
        {
            IList<ISiteRole> roles = new List<ISiteRole>();
            using (DbDataReader reader = await dbRoles.GetPage(siteId, searchInput, pageNumber, pageSize))
            {
                while (reader.Read())
                {
                    SiteRole role = new SiteRole();
                    role.LoadFromReader(reader);
                    role.MemberCount = Convert.ToInt32(reader["MemberCount"]);

                    roles.Add(role);
                }
            }

            return roles;

        }

        //public IList<ISiteRole> GetRolesUserIsNotIn(
        //    int siteId,
        //    int userId)
        //{
        //    if (multiTenantOptions.UseRelatedSitesMode) { siteId = multiTenantOptions.RelatedSiteId; }

        //    IList<ISiteRole> roles = new List<ISiteRole>();
        //    using (DbDataReader reader = dbRoles.GetRolesUserIsNotIn(siteId, userId))
        //    {
        //        SiteRole role = new SiteRole();
        //        role.LoadFromReader(reader);

        //        roles.Add(role);
        //    }
        //    return roles;
        //}

        //public async Task<List<int>> GetRoleIds(int siteId, string roleNamesSeparatedBySemiColons)
        //{
        //    List<int> roleIds = new List<int>();

        //    List<string> roleNames = GetRolesNames(roleNamesSeparatedBySemiColons);

        //    foreach (string roleName in roleNames)
        //    {
        //        if (string.IsNullOrEmpty(roleName)) { continue; }
        //        ISiteRole r = await FetchRole(siteId, roleName);
        //        if (r == null)
        //        {
        //            log.LogDebug("could not get roleid for role named " + roleName);
        //            continue;
        //        }
        //        if (r.RoleId > -1) { roleIds.Add(r.RoleId); }
        //    }

        //    return roleIds;
        //}

        //public static List<string> GetRolesNames(string roleNamesSeparatedBySemiColons)
        //{
        //    List<string> roleNames = new List<string>();
        //    string[] roles = roleNamesSeparatedBySemiColons.Split(';');
        //    foreach (string r in roles)
        //    {
        //        if (!roleNames.Contains(r)) { roleNames.Add(r); }
        //    }

        //    return roleNames;

        //}


        public async Task<int> CountUsersInRole(int siteId, int roleId, string searchInput)
        {  
            return await dbRoles.GetCountOfUsersInRole(siteId, roleId, searchInput);
        }

        public async Task<IList<IUserInfo>> GetUsersInRole(
            int siteId,
            int roleId,
            string searchInput,
            int pageNumber,
            int pageSize)
        {
            IList<IUserInfo> users = new List<IUserInfo>();

            using (DbDataReader reader = await dbRoles.GetUsersInRole(siteId, roleId, searchInput, pageNumber, pageSize))
            {
                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    users.Add(user);

                }

            }

            return users;
        }

        public async Task<IList<ISiteUser>> GetUsersInRole(
            int siteId,
            string roleName)
        {
            IList<ISiteUser> users = new List<ISiteUser>();

            ISiteRole role = await FetchRole(siteId, roleName);
            int roleId = -3;
            if(role != null)
            {
                roleId = role.RoleId;
            }

            using (DbDataReader reader = await dbRoles.GetUsersInRole(siteId, roleId, string.Empty, 1, 100000))
            {
                while (reader.Read())
                {
                    SiteUser user = new SiteUser();
                    user.LoadFromReader(reader);
                    users.Add(user);

                }

            }

            return users;
        }

        public async Task<int> CountUsersNotInRole(int siteId, int roleId, string searchInput)
        {
            return await dbRoles.GetCountOfUsersNotInRole(siteId, roleId, searchInput);
        }

        public async Task<IList<IUserInfo>> GetUsersNotInRole(
            int siteId,
            int roleId,
            string searchInput,
            int pageNumber,
            int pageSize)
        {
            IList<IUserInfo> users = new List<IUserInfo>();

            using (DbDataReader reader = await dbRoles.GetUsersNotInRole(siteId, roleId, searchInput, pageNumber, pageSize))
            {
                while (reader.Read())
                {
                    UserInfo user = new UserInfo();
                    user.LoadFromReader(reader);
                    users.Add(user);

                }

            }

            return users;
        }



        #endregion

        #region Claims

        /// <summary>
        /// Persists a new instance of UserClaim. Returns true on success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveClaim(IUserClaim userClaim)
        {
            int newId = await dbUserClaims.Create(
                userClaim.SiteId,
                userClaim.UserId,
                userClaim.ClaimType,
                userClaim.ClaimValue);

            userClaim.Id = newId;

            return (newId > -1);
        }

        public async Task<bool> DeleteClaim(int id)
        {
            return await dbUserClaims.Delete(id);
        }

        public async Task<bool> DeleteClaimsByUser(int siteId, string userId)
        {
            return await dbUserClaims.DeleteByUser(siteId, userId);
        }

        public async Task<bool> DeleteClaimByUser(int siteId, string userId, string claimType)
        {
            return await dbUserClaims.DeleteByUser(siteId, userId, claimType);
        }

        public async Task<bool> DeleteClaimsBySite(int siteId)
        {
            return await dbUserClaims.DeleteBySite(siteId);
        }

        public async Task<IList<IUserClaim>> GetClaimsByUser(int siteId, string userId)
        {
            DbDataReader reader = await dbUserClaims.GetByUser(siteId, userId);
            return LoadClaimListFromReader(reader);

        }

        public async Task<IList<ISiteUser>> GetUsersForClaim(
            int siteId,
            string claimType, 
            string claimValue)
        {
            IList<ISiteUser> users = new List<ISiteUser>();

            using (DbDataReader reader = await dbUserClaims.GetUsersByClaim(siteId, claimType, claimValue))
            {
                while (reader.Read())
                {
                    SiteUser user = new SiteUser();
                    user.LoadFromReader(reader);
                    users.Add(user);

                }

            }

            return users;
        }


        private List<IUserClaim> LoadClaimListFromReader(DbDataReader reader)
        {
            List<IUserClaim> userClaimList = new List<IUserClaim>();

            using (reader)
            {
                while (reader.Read())
                {
                    UserClaim userClaim = new UserClaim();
                    userClaim.LoadFromReader(reader);
                    userClaimList.Add(userClaim);

                }
            }
            
            return userClaimList;

        }


        #endregion

        #region Logins

        /// <summary>
        /// Persists a new instance of UserLogin. Returns true on success.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateLogin(IUserLogin userLogin)
        {
            if (userLogin.LoginProvider.Length == -1) { return false; }
            if (userLogin.ProviderKey.Length == -1) { return false; }
            if (userLogin.UserId.Length == -1) { return false; }

            return await dbUserLogins.Create(
                userLogin.SiteId,
                userLogin.LoginProvider,
                userLogin.ProviderKey,
                userLogin.ProviderDisplayName,
                userLogin.UserId);


        }


        /// <param name="loginProvider"> loginProvider </param>
        /// <param name="providerKey"> providerKey </param>
        public async Task<IUserLogin> FindLogin(
            int siteId,
            string loginProvider,
            string providerKey)
        {
            using (DbDataReader reader = await dbUserLogins.Find(
                siteId,
                loginProvider,
                providerKey))
            {
                if (reader.Read())
                {
                    UserLogin userLogin = new UserLogin();
                    userLogin.LoadFromReader(reader);
                    return userLogin;

                }
            }

            return null;
        }


        /// <summary>
        /// Deletes an instance of UserLogin. Returns true on success.
        /// </summary>
        /// <param name="loginProvider"> loginProvider </param>
        /// <param name="providerKey"> providerKey </param>
        /// <param name="userId"> userId </param>
        /// <returns>bool</returns>
        public async Task<bool> DeleteLogin(
            int siteId,
            string loginProvider,
            string providerKey,
            string userId)
        {
            return await dbUserLogins.Delete(
                siteId,
                loginProvider,
                providerKey,
                userId);
        }

        public async Task<bool> DeleteLoginsByUser(int siteId, string userId)
        {
            return await dbUserLogins.DeleteByUser(siteId, userId);
        }

        public async Task<bool> DeleteLoginsBySite(int siteId)
        {
            return await dbUserLogins.DeleteBySite(siteId);
        }




        /// <summary>
        /// Gets an IList with all instances of UserLogin.
        /// </summary>
        public async Task<IList<IUserLogin>> GetLoginsByUser(
            int siteId,
            string userId)
        {
            List<IUserLogin> userLoginList = new List<IUserLogin>();
            using (DbDataReader reader = await dbUserLogins.GetByUser(siteId, userId))
            {
                while (reader.Read())
                {
                    UserLogin userLogin = new UserLogin();
                    userLogin.LoadFromReader(reader);
                    userLoginList.Add(userLogin);

                }
            }

            return userLoginList;

        }




        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SiteRoleStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
