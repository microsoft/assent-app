// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.DevTools.Model.Models;

public interface IUserDelegationHelper
{
    bool IsDelegationExist(string DelegationFor, string DelegationTo, int tenantID);

    Task<List<dynamic>> GetUserDelegations();

    UserDelegationEntity GetDelegation(string id);

    Task<bool> InsertUserDelegation(UserDelegationEntity userDelegationEntity);

    Task<bool> InsertUserDelegationHistory(UserDelegationSettingsHistory userDelegationSettingsHistory);

    Task DeleteUserDelegations(UserDelegationEntity delegations);
}