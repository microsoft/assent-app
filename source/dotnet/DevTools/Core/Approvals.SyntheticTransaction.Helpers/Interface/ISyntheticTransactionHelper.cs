// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

public interface ISyntheticTransactionHelper
{
    Dictionary<string, object> GetPlaceholderDetails(string strJson, string tcv);

    Task<string> GetSchemaFile(string blobName, string tcv);

    Task<string> GetUISchemaFile(string blobName, string tcv);

    void UploadDataToBlob(string content, string tcv);

    Task<string> GenerateSchemaFromSamplePayload(string payload, string tcv);

    string UpdatePayloadValue(string payload, ApprovalTenantInfo tenantEntity, string approver, string tcv);

    void GetEnvironmentName(ref List<string> envNames);

    Task<bool> InsertSyntheticDetail(string payload, ApprovalTenantInfo tenant, string approver, string tcv);
}