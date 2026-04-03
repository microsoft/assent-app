// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IUserDelegationProvider
{

    /// <summary>
    /// Get delegation access level
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="delegateToUser"></param>
    /// <returns></returns>
    DelegationAccessLevel GetDelegationAccessLevel(User manager, User delegateToUser);

    /// <summary>
    /// Get user delegation for current user
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetUserDelegationsForCurrentUser(User signedInUser);

    /// <summary>
    /// Get people delegated to logged in alias
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetPeopleDelegatedToMe(User signedInUser);
}
