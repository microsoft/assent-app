// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json;

/// <summary>
/// Attachment helper.
/// </summary>
public class AttachmentHelper : IAttachmentHelper
{
    /// <summary>
    /// Blob storage helper.
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// Tenant informational helper.
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The approval detail provider.
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogProvider _logger = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;


    /// <summary>
    /// Starting index for the file name.
    /// </summary>
    private readonly int startingIdIndex = 1000;

    /// <summary>
    /// The validation factory
    /// </summary>
    private readonly IValidation _validation = null;

    /// <summary>
    /// The flighting data provider
    /// </summary>
    private readonly IFlightingDataProvider _flightingDataProvider;

    /// <summary>
    /// Constructor for attachment helper.
    /// </summary>
    /// <param name="blobStorageHelper">Blob storage helper.</param>
    /// <param name="approvalTenantInfoHelper">Tenant information helper.</param>
    /// <param name="approvalDetailProvider">Approval details helper.</param>
    /// <param name="logger"> Logging helper.</param>
    /// <param name="performanceLogger">Performance activity helper.</param>
    /// <param name="validation">Validation from the Domain.Tenant validation.</param>
    public AttachmentHelper(
        IBlobStorageHelper blobStorageHelper,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IApprovalDetailProvider approvalDetailProvider,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IValidation validation,
        IFlightingDataProvider flightingDataProvider)
    {
        _blobStorageHelper = blobStorageHelper;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _approvalDetailProvider = approvalDetailProvider;
        _logger = logger;
        _performanceLogger = performanceLogger;
        _validation = validation;
        _flightingDataProvider = flightingDataProvider;
    }

    /// <summary>
    /// Upload attachment files into the blob storage.
    /// </summary>
    /// <param name="files">Files to be uploaded.</param>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="documentNumber">Document number of the request.</param>
    /// <param name="sessionId">Session id.</param>
    /// <param name="tcv">Transaction id.</param>
    /// <param name="xcv">Correlation id.</param>
    /// <returns>Upload status.</returns>
    public async Task<List<AttachmentUploadStatus>> UploadAttachments(List<AttachmentUploadInfo> files, int tenantId, string documentNumber, string userAlias, string sessionId, string tcv, string xcv)
    {
        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(tcv))
        {
            tcv = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(xcv))
        {
            xcv = documentNumber;
        }

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.DisplayDocumentNumber, documentNumber }
        };

        #endregion Logging Prep

        var fileUploadStatuses = new List<AttachmentUploadStatus>();
        var approvalResponse = new ApprovalResponse();

        try
        {
            List<Attachment> attachmentsSummary = new List<Attachment>();

            // Get the tenant infomation from the configuration.
            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

            if (tenantInfo != null)
            {
                logData[LogDataKey.TenantName] = tenantInfo.AppName;
                logData[LogDataKey.DocumentTypeId] = tenantInfo.DocTypeId;

                var transactionalDetails = _approvalDetailProvider.GetAllApprovalsDetails(tenantId, documentNumber);

                if (transactionalDetails != null && transactionalDetails.Any())
                {
                    // Filter to get only the row which has TransactionalDetails 
                    var existingAttachmentsRecord = transactionalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.AttachmentsOperationType, StringComparison.InvariantCultureIgnoreCase));

                    if (existingAttachmentsRecord != null)
                    {
                        attachmentsSummary = JsonConvert.DeserializeObject<List<Attachment>>(existingAttachmentsRecord?.JSONData);
                    }
                }

                _logger.LogInformation(TrackingEvent.AttachmentUploadBegin, logData);

                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, Constants.AttachmentUpload), logData))
                {
                    string fileID = string.Empty;
                    var attachmentProperties = tenantInfo.IsUploadAttachmentsEnabled ? (_flightingDataProvider.IsFeatureEnabledForUser(userAlias, (int)FlightingFeatureName.UploadAttachment) ? tenantInfo?.AttachmentProperties?.FromJson<AttachmentProperties>() : null) : null;
                    if (attachmentProperties is null)
                    {
                        var e2EErrorInformation = new ApprovalResponseErrorInfo()
                        {
                            ErrorMessages = new List<string>() { $"Attachments upload feature not enabled for Tenant {tenantInfo.AppName} or User {userAlias}." },
                            ErrorType = ApprovalResponseErrorType.UnintendedTransientError
                        };
                        fileUploadStatuses.Add(new AttachmentUploadStatus()
                        {
                            Name = "",
                            ActionResult = false,
                            E2EErrorInformation = e2EErrorInformation,
                            DisplayMessage = "Attachment upload failed unintendedly.",
                            Telemetry = new ApprovalsTelemetry() { BusinessProcessName = tenantInfo.BusinessProcessName, Tcv = tcv, Xcv = xcv }
                        });
                        return fileUploadStatuses;
                    }

                    foreach (var file in files)
                    {
                        string fileName = file.Name;

                        try
                        {
                            var result = _validation.ValidateAttachmentUpload(file, attachmentsSummary, attachmentProperties, files);

                            if (!result.ActionResult)
                            {
                                var e2EErrorInformation = new ApprovalResponseErrorInfo()
                                {
                                    ErrorMessages = result.ErrorMessages,
                                    ErrorType = ApprovalResponseErrorType.IntendedError
                                };

                                fileUploadStatuses.Add(new AttachmentUploadStatus()
                                {
                                    Name = fileName,
                                    ActionResult = false,
                                    E2EErrorInformation = e2EErrorInformation,
                                    DisplayMessage = "Attachment upload failed.",
                                    Telemetry = new ApprovalsTelemetry() { BusinessProcessName = tenantInfo.BusinessProcessName, Tcv = tcv, Xcv = xcv }
                                });

                                continue;
                            }

                            fileID = attachmentsSummary
                                .Where(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => x.ID)
                                .FirstOrDefault();

                            if (file != null)
                            {
                                if (file.FileSize > 0)
                                {
                                    var bytes = Convert.FromBase64String(file.Base64Content);
                                    string fileurl = string.Empty;

                                    using (MemoryStream stream = new MemoryStream(bytes))
                                    {
                                        // Upload file in the blob store.
                                        fileurl = await _blobStorageHelper.UploadStreamData(stream, attachmentProperties?.AttachmentContainerName, $"{documentNumber}/{fileName}");
                                    }

                                    // If the file does exists.
                                    if (string.IsNullOrEmpty(fileID))
                                    {
                                        List<string> listOfAttachmentIDs = attachmentsSummary.Select(a => a.ID).ToList();
                                        fileID = GenerateId(listOfAttachmentIDs).ToString();

                                        attachmentsSummary.Add(new Attachment()
                                        {
                                            ID = fileID,
                                            Url = fileurl,
                                            Name = fileName,
                                            IsPreAttached = false
                                        });

                                        // Add ATTACH details into ApprovalDetails Table
                                        ApprovalDetailsEntity approvalDetailsEntity = new ApprovalDetailsEntity()
                                        {
                                            PartitionKey = documentNumber,
                                            RowKey = Constants.AttachmentsOperationType,
                                            ETag = global::Azure.ETag.All,
                                            JSONData = attachmentsSummary.ToJson(),
                                            TenantID = int.Parse(tenantInfo.RowKey)
                                        };
                                        ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
                                        {
                                            Xcv = xcv,
                                            Tcv = tcv,
                                            BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddAttachments, Constants.BusinessProcessNameUserTriggered)
                                        };

                                        _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approvalDetailsEntity, tenantInfo, telemetry);
                                    }

                                    fileUploadStatuses.Add(new AttachmentUploadStatus()
                                    {
                                        Name = fileName,
                                        ActionResult = true,
                                        ID = fileID,
                                        Url = fileurl,
                                        FileSize = file.FileSize
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var e2EErrorInformation = new ApprovalResponseErrorInfo()
                            {
                                ErrorMessages = new List<string>() { ex.Message },
                                ErrorType = ApprovalResponseErrorType.UnintendedTransientError
                            };

                            fileUploadStatuses.Add(new AttachmentUploadStatus()
                            {
                                Name = fileName,
                                ActionResult = false,
                                E2EErrorInformation = e2EErrorInformation,
                                DisplayMessage = "Attachment upload failed unintendedly.",
                                Telemetry = new ApprovalsTelemetry() { BusinessProcessName = tenantInfo.BusinessProcessName, Tcv = tcv, Xcv = xcv }
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(TrackingEvent.AttachmentUploadFailure, ex, logData);
            throw;
        }

        return fileUploadStatuses;
    }

    /// <summary>
    /// Generate Id for the file attachment.
    /// </summary>
    /// <param name="attachmentsSummary"></param>
    /// <returns></returns>
    private int GenerateId(List<string> listOfAttachmentIDs)
    {
        if (listOfAttachmentIDs == null || listOfAttachmentIDs.Count == 0)
        {
            return startingIdIndex;
        }

        int maxIndex = int.Parse(listOfAttachmentIDs.OrderByDescending(c => c).FirstOrDefault());
        return ++maxIndex;
    }

    /// <summary>
    /// Get the attachment details before posting the details to the tenant api.
    /// </summary>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="documentNumber">Document number.</param>
    /// <returns></returns>
    public object GetAttachmentDetailsForTenantNotification(int tenantId, string documentNumber)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.DisplayDocumentNumber, documentNumber }
        };

        var attachmentsList = new Dictionary<string, string>();

        try
        {
            List<Attachment> attachmentsSummary = new List<Attachment>();

            // Get the tenant infomation from the configuration.
            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

            if (tenantInfo != null)
            {
                logData[LogDataKey.TenantName] = tenantInfo.AppName;
                logData[LogDataKey.DocumentTypeId] = tenantInfo.DocTypeId;

                // Get the transaction details for the given document id.
                var transactionalDetails = _approvalDetailProvider.GetAllApprovalsDetails(tenantId, documentNumber);

                if (transactionalDetails != null && transactionalDetails.Any())
                {
                    // Filter to get only the row which has TransactionalDetails 
                    var existingAttachmentsRecord = transactionalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.AttachmentsOperationType, StringComparison.InvariantCultureIgnoreCase));

                    if (existingAttachmentsRecord != null)
                    {
                        attachmentsSummary = JsonConvert.DeserializeObject<List<Attachment>>(existingAttachmentsRecord?.JSONData);

                        if (attachmentsSummary.Any())
                        {
                            foreach (var attachment in attachmentsSummary)
                            {
                                attachmentsList.Add(attachment.ID, attachment.Url);
                            }

                            _logger.LogInformation(TrackingEvent.AttachmentConsolidationForTenantNotificationSuccess, logData);
                        }
                    }
                }
            }

            return attachmentsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(TrackingEvent.AttachmentConsolidationForTenantNotificationFailure, ex, logData);
            throw;
        }
    }
}
