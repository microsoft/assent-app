// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Runtime.Serialization;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

[DataContract(Name = "ApprovalRequestResult", Namespace = "http://www.microsoft.com/document/routing/2010/11")]
public class ApprovalRequestResult
{
    [DataMember]
    public DateTime TimeStamp
    {
        get;
        set;
    }

    [DataMember]
    public ApprovalIdentifier ApprovalIdentifier { get; set; }

    [DataMember]
    public ApprovalRequestResultType Result { get; set; }

    public Exception Exception { get; set; }

    public static ApprovalRequestResult GetApprovalRequestResult(ApprovalIdentifier approvalIdentifier, ApprovalRequestResultType resultType, Exception exception = null)
    {
        var result = new ApprovalRequestResult()
        {
            ApprovalIdentifier = approvalIdentifier,
            Result = resultType,
            TimeStamp = DateTime.UtcNow,
            Exception = exception,
        };
        return result;
    }
}
