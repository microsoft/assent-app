// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

    public interface IBulkDeleteHelper
    {
        Task<object> BulkDelete(string tenant, string approver, string days, string docNumber);

        HttpResponseMessage SendDeletePayload(TestHarnessDocument documnet, string approver, string comment, string action);

        Task<bool> UpdateDocumentStatus(TestHarnessDocument document);
    }
}