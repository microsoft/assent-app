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
    /// <param name="PayloadId">The payload identifier.</param>
    /// <param name="TenantID">The tenant identifier.</param>
    /// <param name="FiscalYear">The fiscal year.</param>
    /// <param name="DisplayDocNumber">The display document number.</param>
    /// <param name="DocTypeId">The document type identifier.</param>
    /// <param name="fromDate">The start date for filtering.</param>
    /// <param name="toDate">The end date for filtering.</param>
    /// <param name="isProcessedMsg">Indicates if the message is processed.</param>
    /// <param name="DocumentNumbers">The document numbers.</param>
    /// <param name="userAlias">The user alias.</param>
    /// <returns>dictionaries containing parameter name and value.</returns>
    private static Dictionary<string, string> BuildParameters(string PayloadId, string TenantID = "", string FiscalYear = "", string DisplayDocNumber = "", string DocTypeId = "", string fromDate = "", string toDate = "", bool isProcessedMsg = false, string DocumentNumbers = "", string userAlias = "")
    {
        var parameters = new Dictionary<string, string>();

        if (!isProcessedMsg)
        {
            if (!string.IsNullOrEmpty(DisplayDocNumber))
            {
                parameters.Add("@docNumber", DisplayDocNumber);
            }
            if (!string.IsNullOrEmpty(FiscalYear))
            {
                parameters.Add("@fiscalYear", FiscalYear);
            }
            if (!string.IsNullOrEmpty(PayloadId))
            {
                parameters.Add("@activityId", PayloadId);
            }
            if (!string.IsNullOrEmpty(DocTypeId))
            {
                parameters.Add("@docTypeId", DocTypeId);
            }
            if (!string.IsNullOrEmpty(userAlias))
            {
                parameters.Add("@alias", userAlias);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(DocTypeId))
            {
                parameters.Add("@docTypeId", DocTypeId);
            }
            if (!string.IsNullOrEmpty(fromDate))
            {
                parameters.Add("@fromDate", fromDate);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                parameters.Add("@toDate", toDate);
            }
            if (!string.IsNullOrEmpty(DocumentNumbers))
            {
                parameters.Add("@docNumbers", DocumentNumbers);
            }
            if (!string.IsNullOrEmpty(userAlias))
            {
                parameters.Add("@alias", userAlias);
            }
        }
        return parameters;
    }
}
