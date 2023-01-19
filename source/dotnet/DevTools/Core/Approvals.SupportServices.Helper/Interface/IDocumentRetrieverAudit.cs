// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

using System.Collections.Generic;

public interface IDocumentRetrieverAudit
{
    List<dynamic> GetDocuments(List<string[]> parameters, string partitionKeyValue, string collectionName = "");

    List<dynamic> GetReceivedRequests(List<string[]> parameters, string partitionKeyValue, string collectionName = "");

    List<dynamic> GetReceivedRequestsByDocumentNumbers(List<string[]> parameters, string partitionKeyValue, string collectionName = "");
}
