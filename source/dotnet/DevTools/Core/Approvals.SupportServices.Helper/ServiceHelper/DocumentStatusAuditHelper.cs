// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

using System;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

public class DocumentStatusAuditHelper : IDocumentStatusAuditHelper
{
    private readonly IDocumentRetrieverAudit _documentRetriever;

    public DocumentStatusAuditHelper(IDocumentRetrieverAudit documentRetriever)
    {
        _documentRetriever = documentRetriever;
    }

    /// <summary>
    /// Get Document History
    /// </summary>
    /// <param name="TenantID"></param>
    /// <param name="FiscalYear"></param>
    /// <param name="DisplayDocNumber"></param>
    /// <param name="collectionName"></param>
    /// <param name="PayloadId"></param>
    /// <param name="DocTypeId"></param>
    /// <returns></returns>
    public List<dynamic> GetDocumentHistory(string TenantID, string FiscalYear, string DisplayDocNumber, string collectionName, string PayloadId = "", string DocTypeId = "")
    {
        var parameters = BuildParameters(PayloadId, TenantID, FiscalYear, DisplayDocNumber, DocTypeId);
        var documents = _documentRetriever.GetDocuments(parameters, TenantID, collectionName);
        if (documents.Count > 0)
        {
            return documents;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get Received Requests
    /// </summary>
    /// <param name="DocTypeID"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public List<dynamic> GetReceivedRequests(string DocTypeID, string fromDate, string toDate, string collectionName)
    {
        var parameters = BuildParameters("", null, null, "", DocTypeID, fromDate, toDate, true);
        var documents = _documentRetriever.GetReceivedRequests(parameters, DocTypeID, collectionName);
        if (documents != null && documents.Count > 0)
        {
            return documents;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the Audit Agent Data for the given set of document numbers
    /// </summary>
    /// <param name="DocTypeID"></param>
    /// <param name="listOfDocumentNumbers"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public List<dynamic> GetReceivedRequestsByDocumentNumbers(string DocTypeID, string listOfDocumentNumbers, string collectionName)
    {
        var parameters = BuildParameters("", null, null, "", DocTypeID, "", "", true, listOfDocumentNumbers);
        var documents = _documentRetriever.GetReceivedRequestsByDocumentNumbers(parameters, DocTypeID, collectionName);
        if (documents != null && documents.Count > 0)
        {
            return documents;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Build Parameters
    /// </summary>
    /// <param name="PayloadId"></param>
    /// <param name="TenantID"></param>
    /// <param name="FiscalYear"></param>
    /// <param name="DisplayDocNumber"></param>
    /// <param name="DocTypeId"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <param name="isProcessedMsg"></param>
    /// <param name="DocumentNumbers"></param>
    /// <param name="userAlias"></param>
    /// <returns></returns>
    private static List<string[]> BuildParameters(string PayloadId, string TenantID = "", string FiscalYear = "", string DisplayDocNumber = "", string DocTypeId = "", string fromDate = "", string toDate = "", bool isProcessedMsg = false, string DocumentNumbers = "", string userAlias = "")
    {
        var parameters = new List<string[]>();
        //Note - Always keep the doc number first in sequence
        if (!isProcessedMsg)
        {
            if (!string.IsNullOrEmpty(DisplayDocNumber))
            {
                parameters.Add(new string[3] { "ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber", "@docNumber", DisplayDocNumber });
            }
            if (!string.IsNullOrEmpty(FiscalYear))
            {
                parameters.Add(new string[3] { "ApprovalRequest.ApprovalIdentifier.FiscalYear", "@fiscalYear", FiscalYear });
            }
            if (!string.IsNullOrEmpty(PayloadId))
            {
                parameters.Add(new string[3] { "BrokeredMsgId", "@activityId", PayloadId });
            }
            if (!string.IsNullOrEmpty(DocTypeId))
            {
                parameters.Add(new string[3] { "ApprovalRequest.DocumentTypeId", "@docTypeId", DocTypeId });
            }
            if (!string.IsNullOrEmpty(userAlias))
            {
                parameters.Add(new string[3] { "ApprovalRequest.Approvers[x].Alias", "@alias", userAlias });
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(DocTypeId))
            {
                parameters.Add(new string[3] { "ApprovalRequest.DocumentTypeId", "@docTypeId", DocTypeId });
            }
            if (!string.IsNullOrEmpty(fromDate))
            {
                parameters.Add(new string[3] { "EnqueuedTimeUtc", "@fromDate", fromDate });
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                parameters.Add(new string[3] { "EnqueuedTimeUtc", "@toDate", toDate });
            }
            if (!string.IsNullOrEmpty(DocumentNumbers))
            {
                parameters.Add(new string[3] { "ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber", "@docNumbers", DocumentNumbers });
            }
            if (!string.IsNullOrEmpty(userAlias))
            {
                parameters.Add(new string[3] { "ApprovalRequest.Approvers[x].Alias", "@alias", userAlias });
            }
        }
        return parameters;
    }
}
