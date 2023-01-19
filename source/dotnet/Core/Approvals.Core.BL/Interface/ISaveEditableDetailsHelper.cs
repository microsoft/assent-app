// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;

public interface ISaveEditableDetailsHelper
{
    /// <summary>
    /// Check if user is authorization for edit
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <param name="userAlias"></param>
    /// <returns></returns>
    bool CheckUserAuthorizationForEdit(int tenantId, string documentNumber, string userAlias);

    /// <summary>
    /// Save edited details
    /// </summary>
    /// <param name="detailsString"></param>
    /// <param name="tenantId"></param>
    /// <param name="userAlias"></param>
    /// <param name="Xcv"></param>
    /// <param name="Tcv"></param>
    /// <param name="loggedInUser"></param>
    /// <returns></returns>
    List<string> SaveEditedDetails(string detailsString, int tenantId, string userAlias, string Xcv, string Tcv, string loggedInUser);
}