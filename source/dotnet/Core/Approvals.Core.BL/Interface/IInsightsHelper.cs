// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface IInsightsHelper
{
    /// <summary>
    /// Returns insights on the users current and past records.
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    Task<JObject> GetSummaryInsights(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId,string xcv, string tenantDocTypeId = "");

    /// <summary>
    /// Get insights on the user's history data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="host"></param>
    /// <param name="sessionId"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="timeperiod"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    Task<JObject> GetHistoryInsights(Graph.Models.User onBehalfUser, string host, string sessionId, int timeperiod, string tenantDocTypeId = "");
}