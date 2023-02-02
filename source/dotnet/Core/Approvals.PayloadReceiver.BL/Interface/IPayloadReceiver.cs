// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;

using System;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IPayloadReceiver
{
    PayloadProcessingResult ProcessPayload(Guid payloadId, Type payloadType, string payload, ApprovalTenantInfo tenant, out string Xcv, out string Tcv, out string ApprovalRequestOperationType, out string BusinessProcessName, out Dictionary<string, string> TenantTelemetry);

    ApprovalRequestExpression DeserializeAndReconstructPayload(Type payloadType, string payload);
}
