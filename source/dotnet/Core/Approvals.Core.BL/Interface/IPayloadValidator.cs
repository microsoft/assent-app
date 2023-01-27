// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public interface IPayloadValidator
{
    /// <summary>
    /// Valdiate arx
    /// </summary>
    /// <param name="arx"></param>
    /// <returns></returns>
    List<ValidationResult> Validate(ApprovalRequestExpressionV1 arx);

    /// <summary>
    /// Server side validation of arx
    /// </summary>
    /// <param name="arx"></param>
    /// <returns></returns>
    Task<List<ValidationResult>> ServerSideValidation(ApprovalRequestExpression arx);
}