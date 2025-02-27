// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

using System.Collections.Generic;

public interface IDocumentRetrieverAudit
{
    List<dynamic> GetDocuments(Dictionary<string, string> parameters, string partitionKeyValue, string collectionName = "");

    List<dynamic> GetReceivedRequests(Dictionary<string, string> parameters, string partitionKeyValue, string collectionName = "");

    List<dynamic> GetReceivedRequestsByDocumentNumbers(Dictionary<string, string> parameters, string partitionKeyValue, string collectionName = "");
}