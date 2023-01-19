// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using System.Collections.Generic;
public interface IMarkRequestOutOfSyncHelper
{
    Dictionary<string, string> MarkRequestsOutOfSync(List<string> documentCollection, string approver, int tenantID);
}
