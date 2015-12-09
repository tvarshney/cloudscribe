﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2014-08-16
// Last Modified:			2015-12-09
// 

// TODO: we should update all the async signatures to take a cancellationtoken

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cloudscribe.Core.Models
{
    public interface ISiteRepository : IDisposable
    {
        Task<ISiteSettings> Fetch(int siteId);
        ISiteSettings FetchNonAsync(int siteId);
        Task<ISiteSettings> Fetch(Guid siteGuid);
        ISiteSettings FetchNonAsync(Guid siteGuid);
        Task<ISiteSettings> Fetch(string hostName);
        ISiteSettings FetchNonAsync(string hostName);
        Task<int> GetCount();
        Task<int> CountOtherSites(int currentSiteId);
        Task<List<ISiteInfo>> GetList();
        Task<List<ISiteInfo>> GetPageOtherSites(
            int currentSiteId,
            int pageNumber,
            int pageSize);

        Task<bool> Save(ISiteSettings site);
        Task<bool> Delete(int siteId);

        Task<List<ISiteHost>> GetSiteHosts(int siteId);
        Task<List<ISiteHost>> GetAllHosts();
        List<ISiteHost> GetAllHostsNonAsync();
        Task<ISiteHost> GetSiteHost(string hostName);
        Task<int> GetHostCount();
        Task<List<ISiteHost>> GetPageHosts(
            int pageNumber,
            int pageSize);
        Task<bool> AddHost(Guid siteGuid, int siteId, string hostName);
        Task<bool> DeleteHost(int hostId);
        Task<int> GetSiteIdByHostName(string hostName);

        
        Task<List<ISiteFolder>> GetSiteFoldersBySite(Guid siteGuid);
        Task<List<ISiteFolder>> GetAllSiteFolders();
        List<ISiteFolder> GetAllSiteFoldersNonAsync();
        Task<int> GetFolderCount();
        Task<ISiteFolder> GetSiteFolder(string folderName);
        Task<List<ISiteFolder>> GetPageSiteFolders(
            int pageNumber,
            int pageSize);
        Task<bool> Save(ISiteFolder siteFolder);
        Task<bool> DeleteFolder(Guid guid);
        Task<int> GetSiteIdByFolder(string folderName);
        int GetSiteIdByFolderNonAsync(string folderName);
        Task<Guid> GetSiteGuidByFolder(string folderName);
        Task<bool> FolderExists(string folderName);

    }
}
