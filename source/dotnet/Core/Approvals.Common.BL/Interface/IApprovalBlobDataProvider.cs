// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Model;

public interface IApprovalBlobDataProvider
{
    /// <summary>
    /// Gets the approval details from BLOB.
    /// </summary>
    /// <param name="filteredSummaryRows">The filtered summary rows.</param>
    /// <returns></returns>
    Task<ApprovalDetailsEntity> GetApprovalDetailsFromBlob(ApprovalDetailsEntity filteredSummaryRows);

    /// <summary>
    /// Adds the approval details.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="blobPointer">The BLOB pointer.</param>
    Task AddApprovalDetails(ApprovalDetailsEntity row, string blobPointer);

    /// <summary>
    /// Deletes the BLOB data.
    /// </summary>
    /// <param name="blobPointer">The BLOB pointer.</param>
    /// <param name="containerName">Name of the container.</param>
    Task DeleteBlobData(string blobPointer, string containerName = Constants.ApprovalAzureBlobContainerName);

    /// <summary>
    /// Gets the BLOB by document identifier.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="documentNumber">The document number.</param>
    /// <returns>return httpResponse message</returns>
    Task<HttpResponseMessage> GetBlobByDocumentId(string alias, string tenantId, string documentNumber);

    /// <summary>
    /// Upload file to blob
    /// </summary>
    /// <param name="blobPointer">The blobPointer</param>
    /// <param name="containerName">The containerNam</param>
    /// <param name="stream">The stream content of file</param>
    Task UploadFileToBlob(string blobPointer, string containerName, Stream stream);

    /// <summary>
    /// Gets Tenant Image in Base64 format
    /// </summary>
    /// <param name="fileName"> fileName to retrieve from blob storage </param>
    /// <returns> Image </returns>
    Task<string> GetTenantImageBase64(string fileName);
}