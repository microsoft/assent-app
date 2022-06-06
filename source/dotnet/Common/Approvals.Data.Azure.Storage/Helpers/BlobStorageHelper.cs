// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Data.Azure.Storage.Interface;
    using global::Azure;
    using global::Azure.Storage;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using global::Azure.Storage.Sas;
    using Microsoft.AspNetCore.Http;

    public class BlobStorageHelper : IBlobStorageHelper
    {
        #region Variables

        private readonly BlobServiceClient _blobServiceClient;

        #endregion Variables

        #region Constructor

        public BlobStorageHelper(string storageAccountName, string storageAccountKey)
        {
            var serviceUri = $"https://{storageAccountName}.blob.core.windows.net";
            var storageCredentialsAccountAndKey = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            _blobServiceClient = new BlobServiceClient(new Uri(serviceUri), storageCredentialsAccountAndKey);
        }

        public BlobStorageHelper(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Gets the container SAS Token
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string GetContainerSasToken(string containerName, DateTimeOffset offset)
        {
            UserDelegationKey userDelegationKey = _blobServiceClient.GetUserDelegationKey(DateTimeOffset.UtcNow, offset);

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                Resource = "c", //Value b is for generating token for a Blob and c is for container
                StartsOn = DateTime.UtcNow.AddMinutes(-1),
                ExpiresOn = offset
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read); //multiple permissions can be added by using | symbol

            var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName);

            return sasToken.ToString();// usage - ?{sasToken}&restype=container&comp=list");

            /* Note : If you want to list the items inside container and view those details in a browser based on the generated SAS token
             * then two additional query parameters has to be appended to the token
             * the Query parameters are "restype=container&comp=list"
             */
        }

        /// <summary>
        /// Gets the Blob Uri
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string GetBlobUri(string containerName, string blobName)
        {
            return string.Format(@"https://{0}.blob.core.windows.net/{1}/{2}", _blobServiceClient.AccountName, containerName, blobName);
        }

        /// <summary>
        /// Check if container name exist at storage account
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>bool</returns>
        public async Task<bool> DoesExist(string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            return await (await GetBlobContainerClient(containerName, storageAccountName, storageAccountKey)).GetBlobClient(blobName).ExistsAsync();
        }

        /// <summary>
        /// List blobs hierarchical listing
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="prefix"></param>
        /// <param name="segmentSize"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public async Task<List<BlobItem>> ListBlobsHierarchicalListing(string containerName, string? prefix, int? segmentSize, BlobContainerClient container = null)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (container == null)
            {
                container = await GetBlobContainerClient(containerName);
            }
            // Call the listing operation and return pages of the specified size.
            var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                .AsPages(default, segmentSize);

            List<BlobItem> lstBlobItem = new List<BlobItem>();

            // Enumerate the blobs returned for each page.
            await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
            {
                // A hierarchical listing may return both virtual directories and blobs.
                foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                {
                    if (blobhierarchyItem.IsPrefix)
                    {
                        // Write out the prefix of the virtual directory.
                        Console.WriteLine("Virtual directory prefix: {0}", blobhierarchyItem.Prefix);

                        // Call recursively with the prefix to traverse the virtual directory.
                        var blobChildList = await ListBlobsHierarchicalListing(containerName, blobhierarchyItem.Prefix, null, container);
                        lstBlobItem.AddRange(blobChildList);
                    }
                    else
                    {
                        lstBlobItem.Add(blobhierarchyItem.Blob);
                    }
                }
            }
            return lstBlobItem;
        }

        /// <summary>
        /// Save the byte arrray to blob
        /// </summary>
        /// <param name="data">Byte array data</param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>Task</returns>
        public async Task UploadByteArray(byte[] data, string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            MemoryStream stream = new MemoryStream(data);
            await UploadStreamData(stream, containerName, blobName, storageAccountName, storageAccountKey);
        }

        /// <summary>
        /// Save the stream data to blob
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>Task</returns>
        public async Task UploadStreamData(Stream stream, string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            var blobContainerClient = await GetBlobContainerClient(containerName, storageAccountName, storageAccountKey);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var fileExists = await blobClient.ExistsAsync();
            if (!fileExists)
            {
                // added overwrite as true to handle race conditions
                await blobClient.UploadAsync(stream, true);
            }
        }

        /// <summary>
        /// Saves text to blob
        /// </summary>
        /// <param name="data">Text data </param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>Task</returns>
        public async Task UploadText(string data, string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            await UploadStreamData(stream, containerName, blobName, storageAccountName, storageAccountKey);
        }

        /// <summary>
        /// Upload file to blob
        /// </summary>
        /// <param name="files"></param>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <param name="filename"></param>
        public async Task UploadFileToBlob(List<IFormFile> files, string blobName, string containerName, string filename)
        {
            foreach (var file in files)
            {
                var content = string.Empty;

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    content = reader.ReadToEnd();
                }
                string mimeType = file.ContentType;
                filename = (string.IsNullOrWhiteSpace(filename)) ? file.FileName : filename;
                var blobFullName = (!string.IsNullOrWhiteSpace(blobName) ? string.Format("{0}/{1}", blobName, filename) : filename);
                await UploadText(content, containerName, blobFullName);
            }
        }

        /// <summary>
        /// Upload file to blob async
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="fileData"></param>
        /// <param name="fileMimeType"></param>
        /// <param name="blobFolderName"></param>
        /// <param name="containerName"></param>
        public async Task UploadFileToBlobAsync(string strFileName, string fileData, string fileMimeType, string blobFolderName, string containerName)
        {
            if (strFileName != null && fileData != null)
            {
                var blobName = (!string.IsNullOrWhiteSpace(blobFolderName) ? string.Format("{0}/{1}", blobFolderName, strFileName) : strFileName);
                await UploadText(fileData, containerName, blobName);
            }
        }

        /// <summary>
        /// Upload image file to blob async
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="file"></param>
        /// <param name="fileMimeType"></param>
        /// <param name="containerName"></param>
        public async Task UploadImageFileToBlobAsync(string strFileName, IFormFile file, string fileMimeType, string containerName)
        {
            if (strFileName != null)
            {
                await UploadStreamData(file.OpenReadStream(), containerName, strFileName);
            }
        }

        /// <summary>
        /// Download text data from blob
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>string</returns>
        public async Task<string> DownloadText(string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            // convert byteArray to stream
            byte[] byteArray = (await DownloadStreamData(containerName, blobName, storageAccountName, storageAccountKey)).ToArray();
            MemoryStream stream = new MemoryStream(byteArray);

            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Download blob from storage
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>Byte array data</returns>
        public async Task<byte[]> DownloadByteArray(string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            return (await DownloadStreamData(containerName, blobName, storageAccountName, storageAccountKey)).ToArray();
        }

        /// <summary>
        /// Download stream data from blob
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>Memory Stream</returns>
        public async Task<MemoryStream> DownloadStreamData(string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            var blobContainerClient = await GetBlobContainerClient(containerName, storageAccountName, storageAccountKey);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            using (var stream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(stream);
                return stream;
            }
        }

        /// <summary>
        /// Deletes data from blob
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="storageAccountKey">Storage Account Key</param>
        /// <returns>Task</returns>
        public async Task DeleteBlob(string containerName, string blobName, string storageAccountName = "", string storageAccountKey = "")
        {
            var blobContainerClient = await GetBlobContainerClient(containerName, storageAccountName, storageAccountKey);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var fileExists = await blobClient.ExistsAsync();
            if (fileExists)
            {
                await blobClient.DeleteAsync();
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Get Blob Conatiner
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="storageAccountName"></param>
        /// <param name="storageAccountKey"></param>
        /// <returns>CloudBlobContainer</returns>
        private async Task<BlobContainerClient> GetBlobContainerClient(string containerName, string storageAccountName = "", string storageAccountKey = "")
        {
            var blobServiceClient = GetBlobServiceClient(storageAccountName, storageAccountKey);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            return blobContainerClient;
        }

        /// <summary>
        /// Get blob client
        /// </summary>
        /// <param name="storageAccountName"></param>
        /// <param name="storageAccountKey"></param>
        /// <returns>BlobServiceClient</returns>
        private BlobServiceClient GetBlobServiceClient(string storageAccountName, string storageAccountKey)
        {
            BlobServiceClient blobServiceClient = null;
            if (!string.IsNullOrWhiteSpace(storageAccountKey) && !string.IsNullOrWhiteSpace(storageAccountName))
            {
                var serviceUri = $"https://{storageAccountName}.blob.core.windows.net";
                var storageCredentialsAccountAndKey = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                blobServiceClient = new BlobServiceClient(new Uri(serviceUri), storageCredentialsAccountAndKey);
            }
            if (blobServiceClient == null)
            {
                blobServiceClient = _blobServiceClient;
            }
            return blobServiceClient;
        }

        #endregion Private Methods
    }
}