// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

public interface IUserDelegationHelper
{
    bool IsDelegationExist(string DelegationFor, string DelegationTo, int tenantID);

    Task<List<dynamic>> GetUserDelegations();

    UserDelegationSetting GetDelegation(string id);

    Task<bool> InsertUserDelegation(UserDelegationSetting userDelegationEntity);

    Task<bool> InsertUserDelegationHistory(UserDelegationSettingsHistory userDelegationSettingsHistory);

    Task DeleteUserDelegations(UserDelegationSetting delegations);
}