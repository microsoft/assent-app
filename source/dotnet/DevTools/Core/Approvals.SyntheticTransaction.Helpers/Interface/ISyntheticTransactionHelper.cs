// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Model;

    public interface ISyntheticTransactionHelper
    {
        Dictionary<string, object> GetPlaceholderDetails(string strJson);

        Task<string> GetSchemaFile(string blobName);

        Task<string> GetUISchemaFile(string blobName);

        void UploadDataToBlob(string content);

        Task<string> GenerateSchemaFromSamplePayload(string payload);

        string UpdatePayloadValue(string payload, ApprovalTenantInfo tenantEntity, string Approver);

        void GetEnvironmentName(ref List<string> envNames);

        bool InsertSyntheticDetail(string payload, ApprovalTenantInfo tenant, string approver);
    }
}