// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model.Flighting;

    public interface IFlightingHelper
    {
        /// <summary>
        /// Get all flighted features for given alias
        /// </summary>
        /// <param name="signedInUser">signed-in user</param>
        /// <param name="onBehalfUser">On-behalf user</param>
        /// <param name="oauth2UserToken">OAuth2 User Token</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="tcv">TCV</param>
        /// <param name="xcv">XCV</param>
        /// <returns>list of flighting features</returns>
        List<FlightingFeature> GetFlightingFeatures(User signedInUser, User onBehalfUserr, string oauth2UserToken, string clientDevice, string sessionId, string tcv, string xcv);

        /// <summary>
        /// Get all flighting features
        /// </summary>
        /// <param name="signedInUser">Signed-in user</param>
        /// <param name="onBehalfUser">On-behalf user</param>
        /// <param name="oauth2UserToken">OAuth2 User Token</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="tcv">TCV</param>
        /// <param name="xcv">XCV</param>
        /// <returns>list of flighting feature</returns>
        Task<List<FlightingFeature>> GetAllFlightingFeatures(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string tcv, string xcv);

        /// <summary>
        /// Get list of features
        /// </summary>
        /// <param name="featureIDs">feature IDs</param>
        /// <param name="loggedInUpn">Logged-in UPN</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="clientDevice">Client Device</param>
        /// <returns>List of features with Status</returns>
        List<dynamic> GetFeatures(string featureIDs, string loggedInUpn, string sessionId, string clientDevice);
    }
}