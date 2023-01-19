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
            this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]).Wait();
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
        string documents = null;
        string queryText;
        QueryDefinition query;
        List<string[]> parametersToPass = new List<string[]>();
        if (!parameters[0][2].Contains(','))    //if the parameter name is document number and multiple documents enetred
        {
            queryText =  "SELECT * " +
                        "FROM ApprovalsTenantNotifications f " +
                        "WHERE f." + parameters[0][0] + "='" + parameters[0][2] + "'";
            parametersToPass.Add(parameters[0]);
        }
        else
        {
            string[] DocList = parameters[0][2].Split(',');
            for (int j = 0; j < DocList.Length; j++)
                DocList[j] = DocList[j].Trim();
            List<string> DocNumbers = new List<string>(DocList);
            foreach (string docNumber in DocNumbers)
            {
                documents += ("'" + docNumber + "', ");
            }
            documents = documents.Remove(documents.Length - 2, 2);
            queryText = "SELECT * " +
                        "FROM ApprovalsTenantNotifications f " +
                        "WHERE f." + parameters[0][0] + " IN(" + documents + ")";
        }
        for (int i = 1; i < parameters.Count; i++)
        {
            queryText += " AND f." + parameters[i][0] + "=" + parameters[i][1] + "";
            parametersToPass.Add(parameters[i]);
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
            _cosmosDbHelper.SetTarget(this._configuration[ConfigurationKey.CosmosDbNameAuditAgent.ToString()],
                collectionName, this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]).Wait();
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
                    "WHERE f." + parameters[0][0] + " = " + parameters[0][1];
                parametersToPass.Add(parameters[0]);
                if (!String.IsNullOrEmpty(parameters[1][1]))
                {
                    queryText += " AND f." + parameters[1][0] + " >= " + parameters[1][1];
                    parametersToPass.Add(parameters[1]);
                }
                if (!String.IsNullOrEmpty(parameters[2][1]))
                {
                    queryText += " AND f." + parameters[2][0] + " <= " + parameters[2][1];
                    parametersToPass.Add(parameters[2]);
                }
            }
            else
            {
                queryText =
                "SELECT f.id " +
                "FROM ApprovalsTenantNotifications f";

                if (!String.IsNullOrEmpty(parameters[0][1]))
                {
                    queryText += " WHERE f." + parameters[0][0] + " >= " + parameters[0][1];
                    parametersToPass.Add(parameters[0]);
                }
                if (!String.IsNullOrEmpty(parameters[1][1]))
                {
                    queryText += " AND f." + parameters[1][0] + " <= " + parameters[1][1];
                    parametersToPass.Add(parameters[1]);
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
                _cosmosDbHelper.SetTarget(this._configuration[ConfigurationKey.CosmosDbNameAuditAgent.ToString()],
                collectionName, this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]).Wait();
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
                    "WHERE f." + parameters[0][0] + " = " + parameters[0][1];
                parametersToPass.Add(parameters[0]);

                queryText += " AND f." + parameters[1][0] + " IN({0})";
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
                    collectionName, this._configuration[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]).Wait();
                return _cosmosDbHelper.GetAllDocumentsAsync<dynamic>(query, partitionKeyValue).Result.ToList();

            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
