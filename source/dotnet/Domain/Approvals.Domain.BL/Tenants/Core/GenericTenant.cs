// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Extension;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class GenericTenant
    /// </summary>
    /// <seealso cref="TenantBase" />
    public class GenericTenant : TenantBase
    {
        #region CONSTRUCTOR

        public GenericTenant(
            ApprovalTenantInfo tenantInfo,
            ILogProvider logger,
            IPerformanceLogger performanceLogger,
            IApprovalSummaryProvider approvalSummaryProvider,
            IConfiguration config,
            INameResolutionHelper nameResolutionHelper,
            IApprovalDetailProvider approvalDetailProvider,
            IFlightingDataProvider flightingDataProvider,
            IApprovalHistoryProvider approvalHistoryProvider,
            IBlobStorageHelper blobStorageHelper,
            IAuthenticationHelper authenticationHelper,
            IHttpHelper httpHelper)
            : base(
                  tenantInfo,
                  logger,
                  performanceLogger,
                  approvalSummaryProvider,
                  config,
                  nameResolutionHelper,
                  approvalDetailProvider,
                  flightingDataProvider,
                  approvalHistoryProvider,
                  blobStorageHelper,
                  authenticationHelper,
                  httpHelper)
        {
        }

        public GenericTenant(
            ApprovalTenantInfo tenantInfo,
            string alias,
            string clientDevice,
            string aadToken,
            ILogProvider logger,
            IPerformanceLogger performanceLogger,
            IApprovalSummaryProvider approvalSummaryProvider,
            IConfiguration config,
            INameResolutionHelper nameResolutionHelper,
            IApprovalDetailProvider approvalDetailProvider,
            IFlightingDataProvider flightingDataProvider,
            IApprovalHistoryProvider approvalHistoryProvider,
            IBlobStorageHelper blobStorageHelper,
            IAuthenticationHelper authenticationHelper,
            IHttpHelper httpHelper)
            : base(
                  tenantInfo,
                  alias,
                  clientDevice,
                  aadToken,
                  logger,
                  performanceLogger,
                  approvalSummaryProvider,
                  config,
                  nameResolutionHelper,
                  approvalDetailProvider,
                  flightingDataProvider,
                  approvalHistoryProvider,
                  blobStorageHelper,
                  authenticationHelper,
                  httpHelper)
        {
        }

        #endregion CONSTRUCTOR

        protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
        {
            return String.Format(urlFormat, approvalIdentifier.DocumentNumber, approvalIdentifier.FiscalYear, docTypeId);
        }

        protected override string GetDetailURLUsingAttachmentId(string urlFormat, string attachmentId)
        {
            return String.Format(urlFormat, HttpUtility.UrlEncode(attachmentId));
        }

        public override JArray ExtractApproverChain(string responseString, string currentApprover, string loggedInUser)
        {
            return new JArray();
        }

        protected override string GetSummaryUrl(ApprovalTenantInfo tenantInfo, string documentSummaryUrl, ApprovalRequestExpression approvalRequest, string docTypeId = "")
        {
            return string.Format(documentSummaryUrl, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo), approvalRequest.ApprovalIdentifier.FiscalYear, docTypeId);
        }

        protected override string FutureApproverChainOperationName()
        {
            return string.Empty;
        }

        public override JObject ExecutePostAuthSum(JObject authSumObject)
        {
            if (authSumObject.Property("ApproverNotes") == null)
            {
                Dictionary<string, string> placeHolderDict = new Dictionary<string, string>();
                if (authSumObject.TryGetValue("AdditionalData", out JToken AdditionalDataValue))
                {
                    JSONHelper.ConvertJsonToDictionary(placeHolderDict, AdditionalDataValue.ToString());
                    if (placeHolderDict.TryGetValue("ApproverNotes", out string ApproverNotes))
                    {
                        authSumObject.Add("ApproverNotes", ApproverNotes);
                    }
                }
            }
            return authSumObject;
        }
    }
}