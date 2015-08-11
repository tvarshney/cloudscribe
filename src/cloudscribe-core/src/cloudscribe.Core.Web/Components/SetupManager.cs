﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-08-11
// Last Modified:			2015-08-11
// 

using cloudscribe.Core.Models;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Core.Web.Components
{
    public class SetupManager
    {

        public SetupManager(
            IApplicationEnvironment appEnv,
            ILogger<SetupManager> logger,
            ConfigHelper configuration,
            IDb dbImplementation,
            SiteManager siteManager)
        {
            appBasePath = appEnv.ApplicationBasePath;
            this.siteManager = siteManager;
            log = logger;

        }

        private string appBasePath;
        private SiteManager siteManager;
        private ILogger log;


    }
}
