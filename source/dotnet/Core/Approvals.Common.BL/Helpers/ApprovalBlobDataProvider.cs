// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// The Approval Blob Data Provider class
    /// </summary>
    public class ApprovalBlobDataProvider : IApprovalBlobDataProvider
    {
        /// <summary>
        /// The blob helper
        /// </summary>
        private readonly IBlobStorageHelper _blobStorageHelper = null;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider = null;

        /// <summary>
        /// Constructor of ApprovalBlobDataProvider
        /// </summary>
        /// <param name="blobStorageHelper"></param>
        /// <param name="logProvider"></param>
        public ApprovalBlobDataProvider(IBlobStorageHelper blobStorageHelper, ILogProvider logProvider)
        {
            _blobStorageHelper = blobStorageHelper;
            _logProvider = logProvider;
        }

        #region Provider Methods

        #region Methods for Details data.

        /// <summary>
        /// Get details data from blob.
        /// </summary>
        /// <param name="filteredSummaryRows"></param>
        /// <returns></returns>
        public async Task<ApprovalDetailsEntity> GetApprovalDetailsFromBlob(ApprovalDetailsEntity filteredSummaryRows)
        {
            filteredSummaryRows.JSONData = await _blobStorageHelper.DownloadText(Constants.ApprovalAzureBlobContainerName, filteredSummaryRows.BlobPointer);
            return filteredSummaryRows;
        }

        /// <summary>
        /// Add Blob entry for details call
        /// </summary>
        /// <param name="row"></param>
        /// <param name="blobPointer"></param>
        /// <returns></returns>
        public async Task AddApprovalDetails(ApprovalDetailsEntity row, string blobPointer)
        {
            await _blobStorageHelper.UploadText(row.JSONData.ToString(), Constants.ApprovalAzureBlobContainerName, blobPointer);
        }

        #endregion Methods for Details data.

        #region Common methods for both summary and details.

        /// <summary>
        /// Delete the blob entry for specified blob pointer.
        /// Common for both summary and details data.
        /// </summary>
        /// <param name="blobPointer"></param>
        /// <param name="containerName"></param>
        public async Task DeleteBlobData(string blobPointer, string containerName = Constants.ApprovalAzureBlobContainerName)
        {
            await _blobStorageHelper.DeleteBlob(containerName, blobPointer);
        }

        /// <summary>
        /// Gets the BLOB by document identifier.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="documentNumber">The document number.</param>
        /// <returns>
        /// return httpResponse message
        /// </returns>
        public async Task<HttpResponseMessage> GetBlobByDocumentId(string alias, string tenantId, string documentNumber)
        {
            ///Form the blob pointer
            string blobNameFormat = "{0}|{1}|{2}";
            string blobName = string.Format(blobNameFormat, alias, tenantId, documentNumber);

            bool isBlobValid = await _blobStorageHelper.DoesExist(Constants.NotificationImagesBlobName, blobName).ConfigureAwait(false);

            var message = new HttpResponseMessage(isBlobValid ? System.Net.HttpStatusCode.OK :
                                                System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(isBlobValid ? _blobStorageHelper.GetBlobUri(Constants.NotificationImagesBlobName, blobName) : string.Empty)
            };

            // Set content headers
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return message;
        }

        /// <summary>
        /// Gets Tenant Image in Base64 format
        /// </summary>
        /// <param name="fileName"> fileName to retrieve from blob storage </param>
        /// <returns> Image </returns>
        public async Task<string> GetTenantImageBase64(string fileName)
        {
            try
            {
                if (await _blobStorageHelper.DoesExist(Constants.TenantImagesBlobContainerName, fileName))
                {
                    MemoryStream ms = await _blobStorageHelper.DownloadStreamData(Constants.TenantImagesBlobContainerName, fileName);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.FetchTenantInfo, ex);
            }
            return "";
        }

        /// <summary>
        /// Upload file to blob
        /// </summary>
        /// <param name="blobPointer">The blobPointer</param>
        /// <param name="containerName">The containerNam</param>
        /// <param name="stream">The stream content of file</param>
        public async Task UploadFileToBlob(string blobPointer, string containerName, Stream stream)
        {
            await _blobStorageHelper.UploadStreamData(stream, containerName, blobPointer);
        }

        #endregion Common methods for both summary and details.

        #endregion Provider Methods
    }
}