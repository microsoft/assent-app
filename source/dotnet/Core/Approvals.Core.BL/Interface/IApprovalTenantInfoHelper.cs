// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// interface IApprovalTenantInfoHelper
/// </summary>
public interface IApprovalTenantInfoHelper
{
    /// <summary>
    /// This method with fetch Approval Tenant Informations by host.
    /// </summary>
    /// <param name="host">The host/ClientDevice.</param>
    /// <returns>List of ApprovalTenantInformation.</returns>
    List<ApprovalTenantInfo> GetTenantInfoByHost(string host);

    /// <summary>
    /// This mehtod will fetch Approval Tenant Information by tenantId.
    /// </summary>
    /// <param name="tenantId">The TenantId.</param>
    /// <returns>The ApprovalTenantInformation.</returns>
    ApprovalTenantInfo GetTenantInfo(Int32 tenantId);

    /// <summary>
    /// This method will fetch all the Teant Informations.
    /// </summary>
    /// <returns>List of ApprovalTenantInformation.</returns>
    Task<List<ApprovalTenantInfo>> GetTenants(bool fetchImageDetails = true);

    /// <summary>
    /// This method will fetch all the application names.
    /// </summary>
    /// <returns>List of aplication names</returns>
    Task<List<string>> GetNames();

    /// <summary>
    /// This method will fetch all the tenants who has disabled user delegations.
    /// </summary>
    /// <returns>List of application names</returns>
    Task<List<string>> GetUserDelegationDisabledTenants();

    /// <summary>
    /// This method will fetch UserDelegatiohnDisabledTenants message.
    /// </summary>
    /// <param name="SessionId">The sessionId.</param>
    /// <param name="Alias">The alias.</param>
    /// <param name="ClientDevice">the clientDevice.</param>
    /// <returns>returns message.</returns>
    Task<string> GetUserDelegationDisabledTenantsMessage(string SessionId, string Alias, string ClientDevice);

    /// <summary>
    /// This method will return TenantIds.
    /// </summary>
    /// <param name="tenantName">The tenant name.</param>
    /// <returns>The TenantIds.</returns>
    Task<string> GetTenantIds(string tenantName);

    /// <summary>
    /// This method will get TenantDocTypeId.
    /// </summary>
    /// <param name="tenantName">The TenantName.</param>
    /// <returns>returns TenantDocTypeId.</returns>
    Task<string> GetTenantDocTypeId(string tenantName);

    /// <summary>
    /// This method will get bulk view tenant actions.
    /// </summary>
    /// <param name="bulkActionConcurrentMessageFormat">The bulkActionConcurrentMessageFormat.</param>
    /// <param name="loggedInAlias">The loggedInAlias.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="sessionId">The sessionId.</param>
    /// <returns>Returns BulkViewTenantActions</returns>
    Task<string> GetBulkViewTenantActions(string bulkActionConcurrentMessageFormat, string loggedInAlias, string alias, string clientDevice, string sessionId);

    /// <summary>
    /// This method will retrieve TenantActionDetails from Tenant.
    /// </summary>
    /// <param name="tenantId">The tenantId.</param>
    /// <param name="loggedInAlias">The loggedInAlias.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="sessionId">The sessionId.</param>
    /// <param name="xcv">The xcv.</param>
    /// <param name="tcv">The tcv.</param>
    /// <param name="aadUserToken">AAD Token</param>
    /// <returns>Returns Approval tenant info with action details.</returns>
    Task<ApprovalTenantInfo> GetTenantActionDetails(int tenantId, string loggedInAlias, string alias, string clientDevice, string sessionId, string xcv, string tcv, string aadUserToken);
}