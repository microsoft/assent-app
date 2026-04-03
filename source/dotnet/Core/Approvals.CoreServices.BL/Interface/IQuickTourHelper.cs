// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface
{
    public interface IQuickTourHelper
    {
        /// <summary>
        /// Get all quick tour features with its status (Is viewed or not)
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="loggedInUpn"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        Task<List<QuickTourFeatureWithStatus>> GetAllQuickTourFeatures(string sessionId, string loggedInUpn, string alias, string clientDevice, string domain);
    }
}
