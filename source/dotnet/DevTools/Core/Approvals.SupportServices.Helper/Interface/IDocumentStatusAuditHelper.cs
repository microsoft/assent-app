// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

using System.Collections.Generic;

public interface IDocumentStatusAuditHelper
{
    List<dynamic> GetDocumentHistory(string TenantID, string FiscalYear, string DocNumber, string collectionName, string ActivityID = "", string DocTypeId = "");

    List<dynamic> GetReceivedRequests(string TenantID, string fromDate, string toDate, string collectionName);

    List<dynamic> GetReceivedRequestsByDocumentNumbers(string DocTypeID, string listOfDocumentNumbers, string collectionName);
}
