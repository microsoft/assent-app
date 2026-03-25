// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class OneAskEDDA
/// </summary>
/// <seealso cref="GenericTenant" />
public class OneAskEDDA : GenericTenant
{
    #region CONSTRUCTOR

    public OneAskEDDA(
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

    public OneAskEDDA(
        ApprovalTenantInfo tenantInfo,
        string alias,
        string clientDevice,
        string oauth2Token,
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
        IHttpHelper httpHelper,
        string objectId,
        string domain)
        : base(
              tenantInfo,
              alias,
              clientDevice,
              oauth2Token,
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
              httpHelper,
              objectId,
              domain)
    {
    }

    #endregion CONSTRUCTOR

    /// <summary>
    /// Replace Detail Url's Placeholder value.
    /// </summary>
    /// <param name="urlFormat">Url Format.</param>
    /// <param name="approvalIdentifier">Approval Identifier.</param>
    /// <param name="page">Page</param>
    /// <param name="xcv">xcv</param>
    /// <param name="tcv">tcv</param>
    /// <param name="businessProcessName">Business Process Name.</param>
    /// <param name="docTypeId">DocumentType Id.</param>
    /// <returns></returns>
    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        return String.Format(urlFormat, docTypeId, approvalIdentifier.DisplayDocumentNumber);
    }

    /// <summary>
    /// Replace Summary Url's Placeholder value
    /// </summary>
    /// <param name="tenantInfo">Tenant Information.</param>
    /// <param name="documentSummaryUrl">Document Summary Url.</param>
    /// <param name="approvalRequest">Approval Request.</param>
    /// <param name="docTypeId">DocumentType Id.</param>
    /// <returns>Summary Url</returns>
    protected override string GetSummaryUrl(ApprovalTenantInfo tenantInfo, string documentSummaryUrl, ApprovalRequestExpression approvalRequest, string docTypeId = "")
    {
        return string.Format(documentSummaryUrl, docTypeId, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo));
    }

    #region Adaptive Card Methods

    /// <summary>
    /// Bind Detail template to Email HTMl Body
    /// </summary>
    /// <param name="templateList">email templates</param>
    /// <param name="reportData">report Data</param>
    /// <returns>string</returns>
    public override string BindDetailTemplateToActionableEmailBody(IDictionary<string, string> templateList, JObject reportData)
    {
        string template = templateList[Constants.MAINTEMPLATE];

        template = BindApprover(template, reportData);
        template = BindMessages(template, reportData);
        template = BindCustomAttribute(template, reportData);
        template = BindNotes(template, reportData);
        template = BindLineItems(template, reportData, 1000, templateList);
        template = BindSimpleData(template, reportData, true);

        return template;
    }

    /// <summary>
    /// Bind Line items
    /// </summary>
    /// <param name="template">email template</param>
    /// <param name="lineItems">line items oject</param>
    /// <param name="templateWidth">width of email template</param>
    /// <param name="templateList">template list</param>
    /// <param name="azureConfigurationHelper">configuration helper</param>
    /// <param name="logger">logger</param>
    /// <returns>email template with line items incorporated in it</returns>
    public override string BindLineItems(string template,
        JToken reportData,
        int templateWidth,
        IDictionary<string, string> templateList)
    {
        string pricingConcessionsTemplate = templateList[Constants.PRICINGCONCESSIONSTEMPLATE];
        string nonPricingConcessionsTemplate = templateList[Constants.NONPRICINGCONCESSIONSTEMPLATE];
        string skuItemTemplate = templateList[Constants.SKUITEMTEMPLATE];

        List<string> pricingConcession = new List<string>();
        string lineItemHtml;
        if (reportData?.SelectToken("PricingDetailItems") is JArray pricingConcessions && pricingConcessions?.Children().Count() > 0)
        {
            foreach (JObject row in pricingConcessions)
            {
                lineItemHtml = pricingConcessionsTemplate;
                lineItemHtml = BindSimpleData(lineItemHtml, row);
                pricingConcession.Add(lineItemHtml);
            }
            template = template?.Replace("{{displayPricingConcessionHearer}}", string.Empty);
        }
        else
            template = template?.Replace("{{displayPricingConcessionHearer}}", "none");

        template = template?.Replace("{{PricingItems}}", string.Join("\r\n", pricingConcession));

        List<string> NonpricingConcession = new List<string>();
        if (reportData?.SelectToken("NonPricingDetailItems") is JArray nonPricingConcessions && nonPricingConcessions?.Children().Count() > 0)
        {
            foreach (JObject row in nonPricingConcessions)
            {
                lineItemHtml = nonPricingConcessionsTemplate;
                lineItemHtml = BindSimpleData(lineItemHtml, row);
                NonpricingConcession.Add(lineItemHtml);
            }
            template = template?.Replace("{{displayNonPricingConcessionHearer}}", string.Empty);
        }
        else
            template = template?.Replace("{{displayNonPricingConcessionHearer}}", "none");

        template = template?.Replace("{{NonPricingItems}}", string.Join("\r\n", NonpricingConcession));

        List<string> skuItem = new List<string>();
        if (reportData?.SelectToken("SkuItems") is JArray skuItems && skuItems?.Children().Count() > 0)
        {
            foreach (JObject row in skuItems)
            {
                lineItemHtml = skuItemTemplate;
                lineItemHtml = BindSimpleData(lineItemHtml, row);
                skuItem.Add(lineItemHtml);
            }
            template = template?.Replace("{{displaySkuItemHearer}}", string.Empty);
        }
        else
            template = template?.Replace("{{displaySkuItemHearer}}", "none");

        template = template?.Replace("{{SkuItems}}", string.Join("\r\n", skuItem));

        return template;
    }

    #endregion Adaptive Card Methods
}