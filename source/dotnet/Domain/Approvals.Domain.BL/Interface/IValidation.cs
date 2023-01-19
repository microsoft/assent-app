// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Interface;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public interface IValidation
{
    /// <summary>
    /// Validates if the alias is  valid using Graph API
    /// </summary>
    /// <param name="arx">Input payload in ApprovalRequestExpression format</param>
    /// <returns>List of ValidationResults containing validation error in the alias (if any), along with the list of invalid aliases</returns>
    public Task<List<ValidationResult>> ValidateAlias(ApprovalRequestExpression arx);

    /// <summary>
    /// Checks if the input alias of the user is from a microsoft domain or not
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>True if the alias is from a non-microsoft domain, else False</returns>
    public bool IsExternalUser(string alias);
}