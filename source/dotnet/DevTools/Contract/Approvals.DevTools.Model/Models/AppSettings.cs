// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    /// <summary>
    /// AppSettings class
    /// </summary>
    public class AppSettings
    {
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string ServiceBusConnection { get; set; }
        public string AIBaseURL { get; set; }
        public string GraphAPIAuthenticationURL { get; set; }
        public string GraphAPIAuthString { get; set; }
        public string GraphAPIClientId { get; set; }
        public string GraphAPIClientSecret { get; set; }
        public string GraphAPIResource { get; set; }
        public string AIScope { get; set; }
        public bool SubmitToAPI { get; set; }
        public string ServiceBusTopics { get; set; }
        public string PayloadProcessingFunctionURL { get; set; }
        public string FunctionAppConfiguration { get; set; }
        public string TestTenantConfiguration { get; set; }
        public string StorageConnection { get; set; }
        public string TenantIconBlobUrl { get; set; }
        public string SamplePayloadBlobContainer { get; set; }
        public string TenantIconBlob { get; set; }
        public string ApprovalSummaryTable { get; set; }

    }
}
