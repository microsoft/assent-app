// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

using Microsoft.Azure.Cosmos;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

public class DocumentRetrieverAudit : IDocumentRetrieverAudit
{
    protected readonly IConfiguration _configuration;
    protected readonly ICosmosDbHelper _cosmosDbHelper;

    public DocumentRetrieverAudit(IConfiguration configuration, ICosmosDbHelper cosmosDbHelper)
    {
        _configuration = configuration;
        _cosmosDbHelper = cosmosDbHelper;
        cosmosDbHelper.SetTarget(this._configuration[ConfigurationKey.CosmosDbNameAuditAgent.ToString()],
            this._configuration[ConfigurationKey.CosmosDbCollectionAuditAgent.ToString()],
            this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]);
    }
    /// <summary>
    /// Get Documents
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="partitionKeyValue"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public virtual List<dynamic> GetDocuments(Dictionary<string, string> parameters, string partitionKeyValue, string collectionName = "")
    {
        string queryText;
        QueryDefinition query;
        List<string[]> parametersToPass = new List<string[]>();

        string documentNumber = parameters.ContainsKey("@docNumber") ? parameters["@docNumber"] : null;
        string fiscalYear = parameters.ContainsKey("@fiscalYear") ? parameters["@fiscalYear"] : null;
        string payloadId = parameters.ContainsKey("@activityId") ? parameters["@activityId"] : null;
        string docTypeId = parameters.ContainsKey("@docTypeId") ? parameters["@docTypeId"] : null;
        string alias = parameters.ContainsKey("@alias") ? parameters["@alias"] : null;

        if (string.IsNullOrEmpty(documentNumber))
        {
            throw new ArgumentException("Document number cannot be null or empty", nameof(parameters));
        }

        if (!documentNumber.Contains(','))
        {
            queryText = $"SELECT * FROM ApprovalsTenantNotifications f WHERE f.ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber = @docNumber";
            parametersToPass.Add(new string[] { "@docNumber", documentNumber });
        }
        else //if the parameter name is document number and multiple documents entered
        {
            List<string> docNumbers = documentNumber.Split(',').Select(doc => doc.Trim()).ToList();
            string documentsParam = string.Join(", ", docNumbers.Select((doc, index) => $"@doc{index}"));
            queryText = $"SELECT * FROM ApprovalsTenantNotifications f WHERE f.ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber IN ({documentsParam})";
            for (int i = 0; i < docNumbers.Count; i++)
            {
                parametersToPass.Add(new string[] { $"@doc{i}", docNumbers[i] });
            }
        }
        if (!string.IsNullOrEmpty(fiscalYear))
        {
            queryText += " AND f.ApprovalRequest.ApprovalIdentifier.FiscalYear = @fiscalYear";
            parametersToPass.Add(new string[] { "@fiscalYear", fiscalYear });
        }
        if (!string.IsNullOrEmpty(payloadId))
        {
            queryText += " AND f.BrokeredMsgId = @activityId";
            parametersToPass.Add(new string[] { "@activityId", payloadId });
        }
        if (!string.IsNullOrEmpty(docTypeId))
        {
            queryText += " AND f.ApprovalRequest.DocumentTypeId = @docTypeId";
            parametersToPass.Add(new string[] { "@docTypeId", docTypeId });
        }
        if (!string.IsNullOrEmpty(alias))
        {
            queryText += " AND f.ApprovalRequest.Approvers[x].Alias = @alias";
            parametersToPass.Add(new string[] { "@alias", alias });
        }

        query = new QueryDefinition(queryText);
        for (int i = 0; i < parametersToPass.Count; i++)
        {
            query.WithParameter(parametersToPass[i][0], parametersToPass[i][1]);
        }

        if (string.IsNullOrEmpty(collectionName))
        {
            return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();
        }
        else
        {
            _cosmosDbHelper.SetTarget(this._configuration[ConfigurationKey.CosmosDbNameAuditAgent.ToString()], collectionName, this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]);
            return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();
        }
    }

    public List<dynamic> GetReceivedRequests(Dictionary<string, string> parameters, string partitionKeyValue, string collectionName = "")
    {
        try
        {
            string queryText = "";
            QueryDefinition query;
            List<string[]> parametersToPass = new List<string[]>();

            string docTypeId = parameters.ContainsKey("@docTypeId") ? parameters["@docTypeId"] : null;
            string fromDate = parameters.ContainsKey("@fromDate") ? parameters["@fromDate"] : null;
            string toDate = parameters.ContainsKey("@toDate") ? parameters["@toDate"] : null;

            if (!string.IsNullOrEmpty(docTypeId))
            {
                queryText =
                    "SELECT * " +
                    "FROM ApprovalsTenantNotifications f " +
                    "WHERE f.ApprovalRequest.DocumentTypeId = @docTypeId";
                parametersToPass.Add(new string[] { "@docTypeId", docTypeId });
                if (!string.IsNullOrEmpty(fromDate))
                {
                    queryText += " AND f.EnqueuedTimeUtc >= @fromDate";
                    parametersToPass.Add(new string[] { "@fromDate", fromDate });
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    queryText += " AND f.EnqueuedTimeUtc <= @toDate";
                    parametersToPass.Add(new string[] { "@toDate", toDate });
                }
            }
            else
            {
                queryText =
                "SELECT f.id " +
                "FROM ApprovalsTenantNotifications f";

                if (!string.IsNullOrEmpty(fromDate))
                {
                    queryText += " WHERE f.EnqueuedTimeUtc >= @fromDate";
                    parametersToPass.Add(new string[] { "@fromDate", fromDate });
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    queryText += " AND f.EnqueuedTimeUtc <= @toDate";
                    parametersToPass.Add(new string[] { "@toDate", toDate });
                }
            }

            query = new QueryDefinition(queryText);
            for (int i = 0; i < parametersToPass.Count; i++)
            {
                query.WithParameter(parametersToPass[i][0], parametersToPass[i][1]);
            }

            if (string.IsNullOrEmpty(collectionName))
            {
                return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();
            }
            else
            {
                _cosmosDbHelper.SetTarget(this._configuration[ConfigurationKey.CosmosDbNameAuditAgent.ToString()], collectionName, this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]);
                return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public List<dynamic> GetReceivedRequestsByDocumentNumbers(Dictionary<string, string> parameters, string partitionKeyValue, string collectionName = "")
    {
        try
        {
            string queryText = "";
            QueryDefinition query = null;
            List<string[]> parametersToPass = new List<string[]>();

            string docTypeId = parameters.ContainsKey("@docTypeId") ? parameters["@docTypeId"] : null;
            string documentNumbers = parameters.ContainsKey("@docNumbers") ? parameters["@docNumbers"] : null;

            if (!string.IsNullOrEmpty(docTypeId))
            {
                queryText =
                    "SELECT * " +
                    "FROM ApprovalsTenantNotifications f " +
                    "WHERE f.ApprovalRequest.DocumentTypeId = @docTypeId";
                parametersToPass.Add(new string[] { "@docTypeId", docTypeId });

                if (!string.IsNullOrEmpty(documentNumbers))
                {
                    queryText += " AND f.ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber IN ({0})";
                    var listOfDocumentNumbers = documentNumbers.Split(',').ToList();

                    // IN clause: with list of parameters:
                    // first: use a list (or array) of string, to keep  the names of parameter          
                    // second: loop through the list of input parameters ()
                    var namedParameters = new List<string>();
                    var loopIndex = 0;

                    foreach (var docNumber in listOfDocumentNumbers)
                    {
                        var paramName = "@namedParam_" + loopIndex;
                        namedParameters.Add(paramName);

                        parametersToPass.Add(new string[] { paramName, docNumber.Trim() });
                        loopIndex++;
                    }

                    // now format the query, pass the list of parameter into that
                    if (namedParameters.Count > 0)
                        queryText = string.Format(queryText, string.Join(" , ", namedParameters));
                }

                query = new QueryDefinition(queryText);
                for (int i = 0; i < parametersToPass.Count; i++)
                {
                    query.WithParameter(parametersToPass[i][0], parametersToPass[i][1]);
                }
            }

            if (string.IsNullOrEmpty(collectionName))
            {
                return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();
            }
            else
            {
                _cosmosDbHelper.SetTarget(this._configuration[ConfigurationKey.CosmosDbNameAuditAgent.ToString()],
                    collectionName, this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]);
                return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();

            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}