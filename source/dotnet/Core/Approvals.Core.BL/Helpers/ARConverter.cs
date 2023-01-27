// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.ServiceBus;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The ARConverter class
/// </summary>
public class ARConverter : IARConverter
{
    protected readonly IPerformanceLogger _performanceLogger = null;

    protected readonly IConfiguration _config = null;

    /// <summary>
    /// Constructor of ARConverter
    /// </summary>
    /// <param name="_performanceLogger"></param>
    public ARConverter(IPerformanceLogger performanceLogger, IConfiguration config)
    {
        _performanceLogger = performanceLogger;
        _config = config;
    }

    /// <summary>
    /// Get the Approval Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns>List of Approval Request Expression</returns>
    public virtual List<ApprovalRequestExpressionExt> GetAR(byte[] request, Message message, ApprovalTenantInfo tenantInfo)

    {
        using (var arConverterTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "AR Conversion Time"), new Dictionary<LogDataKey, object>()))
        {
            var stream = new MemoryStream(request);

            var streamReader = new StreamReader(stream);
            var messageBody = streamReader.ReadToEnd();

            stream.Position = 0;

            var approvalInfo = messageBody.FromJson<ApprovalRequestExpressionExt>();
            var arxs = new List<ApprovalRequestExpression>() { approvalInfo };
            var requestNotifications = new List<ApprovalRequestExpressionExt>();
            requestNotifications.AddRange(arxs.Select(arx => new ApprovalRequestExpressionExt() { ActionDetail = arx.ActionDetail, Approvers = arx.Approvers, AdditionalData = arx.AdditionalData, ApprovalIdentifier = arx.ApprovalIdentifier, DeleteFor = arx.DeleteFor, DocumentTypeId = arx.DocumentTypeId, IsCreateOperationComplete = false, IsDeleteOperationComplete = false, IsHistoryLogged = false, IsNotificationSent = false, NotificationDetail = arx.NotificationDetail, Operation = arx.Operation, SummaryData = arx.SummaryData, DetailsData = arx.DetailsData, OperationDateTime = arx.OperationDateTime, Telemetry = arx.Telemetry, RefreshDetails = arx.RefreshDetails }));

            foreach (var arnExtended in requestNotifications)
            {
                if (arnExtended.ApprovalIdentifier != null && string.IsNullOrEmpty(arnExtended.ApprovalIdentifier.DisplayDocumentNumber) && !string.IsNullOrEmpty(arnExtended.ApprovalIdentifier.DocumentNumber))
                {
                    arnExtended.ApprovalIdentifier.DisplayDocumentNumber = arnExtended.ApprovalIdentifier.DocumentNumber;
                }

                if (arnExtended.DeleteFor != null)
                {
                    /// Filter duplicate Aliases from property 'DeleteFor' and reset the property. [Fix for Bug-3595872]
                    arnExtended.DeleteFor = arnExtended.DeleteFor.Select(a => a.ToLower()).Distinct().ToList();
                }

                if (arnExtended.Approvers != null)
                {
                    arnExtended.Approvers = arnExtended.Approvers.Select(a => new Approver() { Delegation = a.Delegation, DetailTemplate = a.DetailTemplate, OriginalApprovers = a.OriginalApprovers, Name = a.Name, Alias = a.Alias.ToLower(), CanEdit = a.CanEdit }).ToList();
                }

                if (arnExtended.ActionDetail != null && arnExtended.ActionDetail.ActionBy != null && arnExtended.ActionDetail.ActionBy.Alias != null)
                {
                    arnExtended.ActionDetail.ActionBy.Alias = arnExtended.ActionDetail.ActionBy.Alias.ToLower();
                }

                if (arnExtended.AdditionalData == null)
                {
                    arnExtended.AdditionalData = new Dictionary<string, string>();
                }

                if (!arnExtended.AdditionalData.Keys.Contains(Constants.RoutingIdColumnName))
                {
                    arnExtended.AdditionalData[Constants.RoutingIdColumnName] = Guid.NewGuid().ToString();
                }

                if (arnExtended.Telemetry == null)
                {
                    arnExtended.Telemetry = new ApprovalsTelemetry()
                    {
                        Xcv = arnExtended.ApprovalIdentifier?.DisplayDocumentNumber,
                        Tcv = Guid.NewGuid().ToString(),
                        BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameARConverter, arnExtended.Operation)
                    };
                }

                if (string.IsNullOrEmpty(arnExtended.Telemetry.Xcv))
                {
                    arnExtended.Telemetry.Xcv = arnExtended.ApprovalIdentifier?.DisplayDocumentNumber;
                }

                if (string.IsNullOrEmpty(arnExtended.Telemetry.Tcv))
                {
                    arnExtended.Telemetry.Tcv = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(arnExtended.Telemetry.BusinessProcessName))
                {
                    arnExtended.Telemetry.BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameARConverter, arnExtended.Operation);
                }
            }

            return requestNotifications;
        }
    }
}