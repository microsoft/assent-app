// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using AutoMapper;
using Microsoft.Approvals.Framework.Services.Interfaces.DataContracts;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using MS.IT.CFE.FIN.MSApprovals.Contracts.DataContracts;
using MS.IT.CFE.FIN.MSApprovals.Model;
using Constants = Contracts.Constants;
using ReminderDetail = MS.IT.CFE.FIN.MSApprovals.Contracts.DataContracts.ReminderDetail;
using Microsoft.CFS.Approvals.Utilities.Interface;

/// <summary>
/// The ARConverter class
/// </summary>
public class ARConverterInternal : ARConverter
{
    /// <summary>
    /// Constructor of ARConverter
    /// </summary>
    /// <param name="_performanceLogger"></param>
    /// <param name="config"></param>
    public ARConverterInternal(IPerformanceLogger performanceLogger, IConfiguration config, INameResolutionHelper nameResolutionHelper) : base(performanceLogger, config, nameResolutionHelper)
    {
    }

    /// <summary>
    /// Get the Approval Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns>List of Approval Request Expression</returns>
    public override List<Contracts.DataContracts.ApprovalRequestExpressionExt> GetAR(byte[] message, ServiceBusReceivedMessage sbMessage, ApprovalTenantInfo tenantInfo)
    {
        using (var arConverterTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "AR Conversion Time"), new Dictionary<LogDataKey, object>()))
        {
            #region Decompress Brokered Message

            var stream = new MemoryStream();
            if (sbMessage.ApplicationProperties.ContainsKey("ApprovalRequestVersion") && sbMessage.ApplicationProperties["ApprovalRequestVersion"].ToString() == _config[Microsoft.CFS.Approvals.Contracts.ConfigurationKey.ApprovalRequestVersion.ToString()])
            {
                stream = new MemoryStream(message);
            }
            else
            {
                using (var dataStream = new MemoryStream(message))
                {
                    using (var compressionStream = new GZipStream(new MemoryStream(message), CompressionMode.Decompress))
                    {
                        compressionStream.CopyTo(stream);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }

            #endregion Decompress Brokered Message

            var requestNotifications = new List<ApprovalRequestExpressionExt>();
            var streamReader = new StreamReader(stream);
            var messageBody = streamReader.ReadToEnd();
            stream.Position = 0;

            var approvalRequestExpressions = new List<Contracts.DataContracts.ApprovalRequestExpressionExt>();
            if (sbMessage.ApplicationProperties.ContainsKey("ApprovalRequestVersion") && sbMessage.ApplicationProperties["ApprovalRequestVersion"].ToString() == _config[Microsoft.CFS.Approvals.Contracts.ConfigurationKey.ApprovalRequestVersion.ToString()])
            {
                var approvalInfo = messageBody.FromJson<Contracts.DataContracts.ApprovalRequestExpressionExt>();
                approvalRequestExpressions.Add(approvalInfo);
            }
            else
            {
                if (messageBody.Contains("ApprovalRequestExpressionExt"))
                {
                    #region Processing ApprovalRequestExpressionExt

                    if (messageBody.Contains("ArrayOfApprovalRequestExpressionExt"))
                    {
                        var serializer = new DataContractSerializer((new List<ApprovalRequestExpressionExt>()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        requestNotifications = (List<ApprovalRequestExpressionExt>)serializer.ReadObject(reader);
                    }
                    else
                    {
                        var serializer = new DataContractSerializer((new ApprovalRequestExpressionExt()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        var approvalInfo = (ApprovalRequestExpressionExt)serializer.ReadObject(reader);
                        requestNotifications.Add(approvalInfo);
                    }

                    #endregion Processing ApprovalRequestExpressionExt
                }
                else if (messageBody.Contains("ApprovalRequestExpression"))
                {
                    #region Processing ARX

                    var arxs = new List<ApprovalRequestExpression>();
                    if (sbMessage.ApplicationProperties.ContainsKey("ApprovalRequestVersion") && sbMessage.ApplicationProperties["ApprovalRequestVersion"].ToString() == "1")
                    {
                        if (messageBody.Contains("ArrayOfApprovalRequestExpression"))
                        {
                            var serializer = new DataContractSerializer((new List<ApprovalRequestExpression>()).GetType());
                            var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                            arxs = (List<ApprovalRequestExpression>)serializer.ReadObject(reader);
                        }
                        else
                        {
                            var serializer = new DataContractSerializer((new ApprovalRequestExpressionV1()).GetType());
                            var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                            var approvalInfo = (ApprovalRequestExpression)serializer.ReadObject(reader);
                            arxs.Add(approvalInfo);
                        }
                    }

                    requestNotifications.AddRange(arxs.Select(arx => new ApprovalRequestExpressionExt() { ActionDetail = arx.ActionDetail, Approvers = arx.Approvers, AdditionalData = arx.AdditionalData, ApprovalIdentifier = arx.ApprovalIdentifier, DeleteFor = arx.DeleteFor, DocumentTypeId = arx.DocumentTypeId, IsCreateOperationComplete = false, IsDeleteOperationComplete = false, IsHistoryLogged = false, IsNotificationSent = false, NotificationDetail = arx.NotificationDetail, Operation = arx.Operation, SummaryData = arx.SummaryData, DetailsData = arx.DetailsData, OperationDateTime = arx.OperationDateTime, Telemetry = arx.Telemetry, RefreshDetails = arx.RefreshDetails }));

                    #endregion Processing ARX
                }
                else if (messageBody.Contains("ApprovalRequestNotificationExt"))
                {
                    #region Processing ApprovalRequestNotificationExt

                    var arns = new List<ApprovalRequestNotificationExt>();
                    if (messageBody.Contains("ArrayOfApprovalRequestNotificationExt"))
                    {
                        var serializer = new DataContractSerializer((new List<ApprovalRequestNotificationExt>()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        arns = (List<ApprovalRequestNotificationExt>)serializer.ReadObject(reader);
                    }
                    else
                    {
                        var serializer = new DataContractSerializer((new ApprovalRequestNotificationExt()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        var approvalInfo = (ApprovalRequestNotificationExt)serializer.ReadObject(reader);
                        arns.Add(approvalInfo);
                    }

                    requestNotifications.AddRange(arns.Select(arn => AutoMapARNToARX(arn)));

                    #endregion Processing ApprovalRequestNotificationExt
                }
                else if (messageBody.Contains("ApprovalRequestNotification"))
                {
                    #region Processing ARN

                    var arns = new List<ApprovalRequestNotification>();
                    if (messageBody.Contains("ArrayOfApprovalRequestNotification"))
                    {
                        var serializer = new DataContractSerializer((new List<ApprovalRequestNotification>()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        arns = (List<ApprovalRequestNotification>)serializer.ReadObject(reader);
                    }
                    else
                    {
                        var serializer = new DataContractSerializer((new ApprovalRequestNotification()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        var approvalInfo = (ApprovalRequestNotification)serializer.ReadObject(reader);
                        arns.Add(approvalInfo);
                    }

                    requestNotifications.AddRange(arns.Select(arn => this.AutoMapARNToARX(arn)));

                    #endregion Processing ARN
                }
                else if (messageBody.Contains("ApprovalRequestInfo"))
                {
                    #region Processing ARI

                    var aris = new List<ApprovalRequestInfo>();

                    if (messageBody.Contains("ArrayOfApprovalRequestInfo"))
                    {
                        var serializer = new DataContractSerializer((new List<ApprovalRequestInfo>()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        aris = (List<ApprovalRequestInfo>)serializer.ReadObject(reader);
                    }
                    else
                    {
                        var serializer = new DataContractSerializer((new ApprovalRequestInfo()).GetType());
                        var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                        var approvalInfo = (ApprovalRequestInfo)serializer.ReadObject(reader);
                        aris.Add(approvalInfo);
                    }

                    requestNotifications.AddRange(aris.Select(ari => this.ConvertARItoARX(ari)));

                    #endregion Processing ARI
                }

                approvalRequestExpressions = requestNotifications.ToJson().FromJson<List<Contracts.DataContracts.ApprovalRequestExpressionExt>>();
            }
            foreach (var arnExtended in approvalRequestExpressions)
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
                    arnExtended.Approvers = arnExtended.Approvers.Select(a => new Contracts.DataContracts.Approver() { DetailTemplate = a.DetailTemplate, OriginalApprovers = a.OriginalApprovers, Name = a.Name, Alias = a.Alias.ToLower(), CanEdit = a.CanEdit, Id = a.Id, UserPrincipalName = a.UserPrincipalName, IsBackupApprover = a.IsBackupApprover }).ToList();
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
                    arnExtended.Telemetry = new Contracts.DataContracts.ApprovalsTelemetry()
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

            return approvalRequestExpressions;
        }
    }

    #region ARConverter Methods

    /// <summary>
    /// Converts ARN object to ARX object to have backward compatibility with previous versions of SDK
    /// </summary>
    /// <param name="arn"></param>
    /// <returns></returns>
    private ApprovalRequestExpressionExt AutoMapARNToARX(ApprovalRequestNotification arn)
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ActionDetails, ActionDetail>();
            cfg.CreateMap<NotificationDetails, NotificationDetail>();
            cfg.CreateMap<MS.IT.CFE.FIN.MSApprovals.Core.Contracts.DataContracts.ReminderDetails, ReminderDetail>();
            cfg.CreateMap<ActionUser, NameAliasEntity>();
            cfg.CreateMap<Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalIdentifier,
                MS.IT.CFE.FIN.MSApprovals.Contracts.DataContracts.ApprovalIdentifier>();
            cfg.CreateMap<ApprovalRequestNotification, ApprovalRequestExpressionExt>()
               .ForMember(dest => dest.NotificationDetail, act => act.MapFrom(src => src.Notification));
        });

        var mapper = new Mapper(config);
        var requestNotification = mapper.Map<ApprovalRequestExpressionExt>(arn);

        if (requestNotification.ActionDetail != null && requestNotification.ActionDetail.ActionBy != null)
        {
            requestNotification.ActionDetail.ActionBy = new NameAliasEntity() { Alias = arn.ActionDetail.ActionBy.Alias, Name = string.Empty };
        }

        requestNotification.AdditionalData.Add("Approver", arn.ActionDetail.ActionBy.Alias);

        if (requestNotification.ActionDetail.NewApprover != null)
        {
            requestNotification.ActionDetail.NewApprover = new NameAliasEntity() { Alias = arn.ActionDetail.NewApprover.Alias, Name = string.Empty };
        }
        requestNotification.DeleteFor = new List<string>();

        return requestNotification;
    }

    /// <summary>
    /// Converts ARI object to ARX object to have backward compatibility with previous versions of SDK
    /// </summary>
    /// <param name="ari"></param>
    /// <returns></returns>
    private ApprovalRequestExpressionExt ConvertARItoARX(ApprovalRequestInfo ari)
    {
        var requestNotification = new ApprovalRequestExpressionExt();
        var documentNumber = string.Empty;
        var fiscalYear = string.Empty;

        var firstOrDefault = ari.DocumentKeys.FirstOrDefault(k => k.Key.ToUpper().Equals("DOCUMENTNUMBER"));
        if (firstOrDefault != null)
        {
            documentNumber = firstOrDefault.Value;
        }

        var documentKey = ari.DocumentKeys.FirstOrDefault(k => k.Key.ToUpper().Equals("FISCALYEAR"));
        if (documentKey != null)
        {
            fiscalYear = documentKey.Value;
        }

        var additionalActionData = new Dictionary<string, string>();
        requestNotification.ActionDetail = new ActionDetail();
        if (ari.ActionDetails != null)
        {
            foreach (var additionalData in ari.ActionDetails.Where(k => k.Key.Equals("COMMENT", StringComparison.InvariantCultureIgnoreCase) == false
                && k.Key.Equals("PLACEMENT", StringComparison.InvariantCultureIgnoreCase) == false).Select(k => new KeyValuePair<string, string>(k.Key, k.Value)))
            {
                additionalActionData.Add(additionalData.Key, additionalData.Value);
            }

            requestNotification.ActionDetail.AdditionalData = additionalActionData;
            requestNotification.ActionDetail.Comment = ari.ActionDetails.FirstOrDefault(k => k.Key.Equals("COMMENT", StringComparison.InvariantCultureIgnoreCase)) != null ? ari.ActionDetails.FirstOrDefault(k => k.Key.Equals("COMMENT", StringComparison.InvariantCultureIgnoreCase)).Value : string.Empty;
            requestNotification.ActionDetail.Placement = ari.ActionDetails.FirstOrDefault(k => k.Key.Equals("PLACEMENT", StringComparison.InvariantCultureIgnoreCase)) != null ? ari.ActionDetails.FirstOrDefault(k => k.Key.Equals("PLACEMENT", StringComparison.InvariantCultureIgnoreCase)).Value : string.Empty;
        }

        requestNotification.ActionDetail.ActionBy = new NameAliasEntity() { Alias = string.IsNullOrEmpty(ari.OriginalApprover) ? ari.Approver : ari.OriginalApprover, Name = string.Empty };
        requestNotification.ActionDetail.Date = ari.ActionDate;
        if (ari.ActionTaken != null)
        {
            requestNotification.ActionDetail.Name = ari.ActionTaken;
        }

        requestNotification.AdditionalData = new Dictionary<string, string>();

        if (ari.Requestor != null)
        {
            requestNotification.AdditionalData.Add("Requestor", ari.Requestor);
        }

        if (ari.RoutingId != null)
        {
            requestNotification.AdditionalData.Add("RoutingId", ari.RoutingId.ToString());
        }

        requestNotification.AdditionalData.Add("Approver", ari.Approver);
        requestNotification.ActionDetail.NewApprover = new NameAliasEntity() { Alias = ari.Approver, Name = string.Empty };

        requestNotification.ApprovalIdentifier = new MS.IT.CFE.FIN.MSApprovals.Contracts.DataContracts.ApprovalIdentifier()
        {
            DocumentNumber = documentNumber,
            FiscalYear = fiscalYear
        };

        // DataContractVersion = "1.0",
        requestNotification.DeleteFor = new List<string>();
        requestNotification.DocumentTypeId = ari.DocumentTypeId;
        requestNotification.NotificationDetail = new NotificationDetail()
        {
            Bcc = ari.BCC,
            Cc = ari.CC,
            SendNotification = ari.SendNotification,
            TemplateKey = ari.NotificationTemplateKey,
            To = ari.To
        };
        if (Enum.Parse(typeof(Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestOperation), ari.Operation.ToString()).ToString().Equals(Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestOperation.Complete.ToString()))
        {
            requestNotification.Operation = MS.IT.CFE.FIN.MSApprovals.Contracts.DataContracts.ApprovalRequestOperation.Delete;
        }
        else
        {
            requestNotification.Operation = (MS.IT.CFE.FIN.MSApprovals.Contracts.DataContracts.ApprovalRequestOperation)Enum.Parse(typeof(Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestOperation), ari.Operation.ToString());
        }

        return requestNotification;
    }

    #endregion ARConverter Methods
}