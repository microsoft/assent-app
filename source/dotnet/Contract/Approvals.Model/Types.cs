// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Runtime.Serialization;


[Serializable]
public class MicrosoftApprovalsException : Exception
{
    public MicrosoftApprovalsException()
    {
    }

    public MicrosoftApprovalsException(string message)
        : base(message)
    {
    }

    public MicrosoftApprovalsException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public MicrosoftApprovalsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

[Serializable]
public class TenantDataNotFoundException : Exception
{
    public TenantDataNotFoundException()
    {
    }

    public TenantDataNotFoundException(string message)
        : base(message)
    {
    }

    public TenantDataNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public TenantDataNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

[Serializable]
public class ApprovalDeadMessageRow
{
    public string Application { get; set; }
    public string Approver { get; set; }
    public string EventPacificTime { get; set; }
    public string PreviousApprover { get; set; }
    public string Requestor { get; set; }
    public string DocumentNumber { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ApprovalDeadMessageRowEntity : BaseTableEntity
{
    public string Application { get; set; }
    public string Approver { get; set; }
    public string EventPacificTime { get; set; }
    public string PreviousApprover { get; set; }
    public string Requestor { get; set; }
    public string DocumentNumber { get; set; }

    public ApprovalDeadMessageRowEntity()
    { }

    public ApprovalDeadMessageRowEntity(ApprovalDeadMessageRow admr)
    {
        Application = admr.Application;
        Approver = admr.Approver;
        EventPacificTime = admr.EventPacificTime;
        PreviousApprover = admr.PreviousApprover;
        Requestor = admr.Requestor;
        DocumentNumber = admr.DocumentNumber;

        PartitionKey = admr.PartitionKey;
        RowKey = admr.RowKey;
        Timestamp = admr.Timestamp;
    }
}