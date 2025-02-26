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
    public virtual List<dynamic> GetDocuments(List<string[]> parameters, string partitionKeyValue, string collectionName = "")
    {
        string queryText;
        QueryDefinition query;
        List<string[]> parametersToPass = new List<string[]>();
        if (!parameters[0][2].Contains(','))    //if the parameter name is document number and multiple documents enetred
        {
            queryText = $"SELECT * FROM ApprovalsTenantNotifications f WHERE f.{parameters[0][0]} = @docNumber";
            parametersToPass.Add(new string[] { parameters[0][0], "@docNumber", parameters[0][2] });
        }
        else
        {
            List<string> docNumbers = parameters[0][2].Split(',').Select(doc => doc.Trim()).ToList();
            string documentsParam = string.Join(", ", docNumbers.Select((doc, index) => $"@doc{index}"));
            queryText = $"SELECT * FROM ApprovalsTenantNotifications f WHERE f.{parameters[0][0]} IN ({documentsParam})";
            for (int i = 0; i < docNumbers.Count; i++)
            {
                parametersToPass.Add(new string[3] { parameters[0][0], $"@doc{i}", docNumbers[i] });
            }
        }
        for (int i = 1; i < parameters.Count; i++)
        {
            queryText += $" AND f.{parameters[i][0]} = @param{i}";
            parametersToPass.Add(new string[] { parameters[i][0], $"@param{i}", parameters[i][2] });
        }

        query = new QueryDefinition(queryText);
        for (int i = 0; i < parametersToPass.Count; i++)
        {
            query.WithParameter(parametersToPass[i][1], parametersToPass[i][2]);
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

    public List<dynamic> GetReceivedRequests(List<string[]> parameters, string partitionKeyValue, string collectionName = "")
    {
        try
        {
            string queryText = "";
            QueryDefinition query;
            List<string[]> parametersToPass = new List<string[]>();
            if (parameters[0][0].Equals("ApprovalRequest.DocumentTypeId"))
            {
                queryText =
                    "SELECT * " +
                    "FROM ApprovalsTenantNotifications f " +
                    "WHERE f." + parameters[0][0] + " = @docTypeId";
                parametersToPass.Add(new string[] { parameters[0][0], "@docTypeId", parameters[0][2] });
                if (!String.IsNullOrEmpty(parameters[1][1]))
                {
                    queryText += " AND f." + parameters[1][0] + " >= @fromDate";
                    parametersToPass.Add(new string[] { parameters[1][0], "@fromDate", parameters[1][2] });
                }
                if (!String.IsNullOrEmpty(parameters[2][1]))
                {
                    queryText += " AND f." + parameters[2][0] + " <= @toDate";
                    parametersToPass.Add(new string[] { parameters[2][0], "@toDate", parameters[2][2] });
                }
            }
            else
            {
                queryText =
                "SELECT f.id " +
                "FROM ApprovalsTenantNotifications f";

                if (!String.IsNullOrEmpty(parameters[0][1]))
                {
                    queryText += " WHERE f." + parameters[0][0] + " >= @fromDate";
                    parametersToPass.Add(new string[] { parameters[0][0], "@fromDate", parameters[0][2] });
                }
                if (!String.IsNullOrEmpty(parameters[1][1]))
                {
                    queryText += " AND f." + parameters[1][0] + " <= @toDate";
                    parametersToPass.Add(new string[] { parameters[1][0], "@toDate", parameters[1][2] });
                }
            }

            query = new QueryDefinition(queryText);
            for (int i = 0; i < parametersToPass.Count; i++)
            {
                query.WithParameter(parametersToPass[i][1], parametersToPass[i][2]);
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

    public List<dynamic> GetReceivedRequestsByDocumentNumbers(List<string[]> parameters, string partitionKeyValue, string collectionName = "")
    {
        try
        {
            string queryText = "";
            QueryDefinition query = null;
            List<string[]> parametersToPass = new List<string[]>();
            if (parameters[0][0].Equals("ApprovalRequest.DocumentTypeId"))
            {
                queryText =
                    "SELECT * " +
                    "FROM ApprovalsTenantNotifications f " +
                    "WHERE f." + parameters[0][0] + " = @docTypeId";
                parametersToPass.Add(new string[] { parameters[0][0], "@docTypeId", parameters[0][2] });

                queryText += " AND f." + parameters[1][0] + " IN ({0})";
                var listOfDocumentNumbers = parameters[1][2].Split(',').ToList();

                // IN clause: with list of parameters:
                // first: use a list (or array) of string, to keep  the names of parameter          
                // second: loop through the list of input parameters ()
                var namedParameters = new List<string>();
                var loopIndex = 0;

                foreach (var docNumber in listOfDocumentNumbers)
                {
                    var paramName = "@namedParam_" + loopIndex;
                    namedParameters.Add(paramName);

                    parametersToPass.Add(new string[] { "", paramName, docNumber.Trim() });

                    loopIndex++;
                }

                // now format the query, pass the list of parameter into that
                if (namedParameters.Count > 0)
                    queryText = string.Format(queryText, string.Join(" , ", namedParameters));

                query = new QueryDefinition(queryText);
                for (int i = 0; i < parametersToPass.Count; i++)
                {
                    query.WithParameter(parametersToPass[i][1], parametersToPass[i][2]);
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