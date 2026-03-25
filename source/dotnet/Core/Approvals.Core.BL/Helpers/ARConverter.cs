// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using AutoMapper;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Model.MapperProfiles;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// The ARConverter class
/// </summary>
public class ARConverter : IARConverter
{
    protected readonly IPerformanceLogger _performanceLogger = null;

    protected readonly IConfiguration _config = null;

    protected readonly INameResolutionHelper _nameResolutionHelper = null;

    private static readonly IMapper _mapper;

    // Static constructor to initialize the AutoMapper instance once
    static ARConverter()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AdobeSignToARXProfile>());
        _mapper = config.CreateMapper();
    }

    /// <summary>
    /// Constructor of ARConverter
    /// </summary>
    /// <param name="_performanceLogger"></param>
    public ARConverter(IPerformanceLogger performanceLogger, IConfiguration config, INameResolutionHelper nameResolutionHelper)
    {
        _performanceLogger = performanceLogger;
        _config = config;
        _nameResolutionHelper = nameResolutionHelper;
    }

    /// <summary>
    /// Get the Approval Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns>List of Approval Request Expression</returns>
    public virtual List<ApprovalRequestExpressionExt> GetAR(byte[] request, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo)

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
                    arnExtended.Approvers = arnExtended.Approvers.Select(a => new Approver() { DetailTemplate = a.DetailTemplate, OriginalApprovers = a.OriginalApprovers, Name = a.Name, Alias = a.Alias.ToLower(), CanEdit = a.CanEdit, IsBackupApprover = a.IsBackupApprover }).ToList();
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

    /// <summary>
    /// Converts an event payload object to ARX object to support external partner integrations
    /// </summary>
    /// <typeparam name="T">Type of the event payload</typeparam>
    /// <param name="request"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public ApprovalRequestExpressionExt MapEventToARX<T>(byte[] request, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (tenantInfo == null) throw new ArgumentNullException(nameof(tenantInfo));

        using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "AR Conversion Time"), new Dictionary<LogDataKey, object>()))
        {
            try
            {
                using var stream = new MemoryStream(request);
                using var streamReader = new StreamReader(stream);
                var messageBody = streamReader.ReadToEnd();

                var sourceEventPayload = messageBody.FromJson<T>();
                if (sourceEventPayload == null)
                {
                    throw new InvalidDataException($"Deserialization of {typeof(T).Name} failed.");
                }

                string senderName = string.Empty;
                if (sourceEventPayload is AdobeSignEvent adobeSignEvent)
                {
                    if (!string.IsNullOrWhiteSpace(adobeSignEvent.Agreement.SenderEmail))
                    {
                        var sender = _nameResolutionHelper.GetUserByMail(adobeSignEvent.Agreement.SenderEmail).Result;
                        senderName = sender?.GivenName + " " + sender?.Surname;
                    }

                    if (!string.IsNullOrWhiteSpace(adobeSignEvent.ActingUserEmail))
                        adobeSignEvent.ActingUserEmail = (_nameResolutionHelper.GetUserByMail(adobeSignEvent.ActingUserEmail).Result)?.UserPrincipalName;

                    if (!string.IsNullOrWhiteSpace(adobeSignEvent.ParticipantUserEmail))
                        adobeSignEvent.ParticipantUserEmail = (_nameResolutionHelper.GetUserByMail(adobeSignEvent.ParticipantUserEmail).Result)?.UserPrincipalName;

                    if (adobeSignEvent.Agreement.ParticipantSetsInfo != null && adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets.Any())
                    {
                        foreach (var participantSet in adobeSignEvent.Agreement.ParticipantSetsInfo.ParticipantSets)
                        {
                            if (participantSet.MemberInfos.Any())
                            {
                                foreach (var member in participantSet.MemberInfos)
                                {
                                    var user = (_nameResolutionHelper.GetUserByMail(member.Email).Result);
                                    if (user != null)
                                    {
                                        member.Email = user.UserPrincipalName;
                                        member.Name = user.DisplayName;
                                    }
                                }
                            }
                        }
                    }

                    var adobeArx = _mapper.Map<ApprovalRequestExpressionExt>(adobeSignEvent);
                    if (adobeArx != null)
                    {
                        adobeArx.DocumentTypeId = Guid.Parse(tenantInfo.DocTypeId);
                        adobeArx.SummaryData.DocumentTypeId = tenantInfo.DocTypeId;
                        if (!string.IsNullOrWhiteSpace(senderName) && adobeArx.SummaryData != null)
                        {
                            adobeArx.SummaryData.Title = string.Format("Request to {0} - {1}", adobeSignEvent.Agreement.SignatureType, adobeSignEvent.EventResourceType);
                        }
                    }

                    return adobeArx;
                }

                // Use cached, thread-safe AutoMapper instance
                var arx = _mapper.Map<ApprovalRequestExpressionExt>(sourceEventPayload);
                if (arx != null)
                {
                    arx.DocumentTypeId = Guid.Parse(tenantInfo.DocTypeId);
                    arx.SummaryData.DocumentTypeId = tenantInfo.DocTypeId;
                }
                return arx;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to map event payload to ARX.", ex);
            }
        }
    }
}