// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Newtonsoft.Json.Linq;

/// <summary>
/// Orchestration: POST method which receives the payload from tenant services and sends that to Approvals ServiceBus after all validation checks are successful.
/// </summary>
/// <param name="documentTypeId">Unique TenantId (GUID) specifying a particular Tenant for which the Payload is received.</param>
/// <param name="request">Http Request.</param>
/// <returns>Asynchronous task of Http Response Message.</returns>
public class PayloadReceiverManager : IPayloadReceiverManager
{
    /// <summary>
    /// The payload receiver
    /// </summary>
    private readonly IPayloadReceiver _payloadReceiver = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// The approval tenantinfo helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

    /// <summary>
    /// The approval request expression helper
    /// </summary>
    private readonly IApprovalRequestExpressionHelper _approvalRequestExpressionHelper = null;

    /// <summary>
    /// The Tenant Factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory = null;

    /// <summary>
    /// Constructor of PayloadReceiverManager
    /// </summary>
    /// <param name="payloadReceiver"></param>
    /// <param name="logProvider"></param>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="approvalRequestExpressionHelper"></param>
    /// <param name="tenantFactory"></param>
    public PayloadReceiverManager(IPayloadReceiver payloadReceiver,
        ILogProvider logProvider,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IApprovalRequestExpressionHelper approvalRequestExpressionHelper,
        ITenantFactory tenantFactory)
    {
        _payloadReceiver = payloadReceiver;
        _logProvider = logProvider;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _approvalRequestExpressionHelper = approvalRequestExpressionHelper;
        _tenantFactory = tenantFactory;
    }

    /// <summary>
    /// Orchestration: POST method which receives the payload from tenant services and sends that to Approvals ServiceBus after all validation checks are successful
    /// </summary>
    /// <param name="documentTypeId">Unique TenantId (GUID) specifying a particular Tenant for which the Payload is received</param>
    /// <param name="payload">Data Payload</param>
    /// <returns>Http Response Message</returns>
    public async Task<JObject> ManagePost(string documentTypeId, string payload)
    {
        JObject response = new JObject();

        // Creating a brokered message so that Message Id can be retrieved
        // Message Id being auto generated, it cannot be assigned
        // And hence creating brokered message early in payload processing cycle

        // Using the Brokered Message Message Id as Activity Id
        Guid activityId = Guid.NewGuid();

        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.DocumentTypeId, documentTypeId },
            { LogDataKey._CorrelationId, activityId },

            // Adding property to denote this event is a critical event for tracking
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes }
        };

        try
        {
            #region TenantId pre-condition check

            if (string.IsNullOrWhiteSpace(documentTypeId))
            {
                // Return a response about payload issues
                throw new InvalidDataException("TenantId is required for processing the payload. Please pass the TenantId as query param.");
            }

            // Additionally verifying if the TenantId provided is valid or not
            // This check is fast due to cached Tenant Info Table data and hence doing it early i.e. ahead of payload related validations
            ApprovalTenantInfo tenantInfo = (await _approvalTenantInfoHelper.GetTenants(false)).FirstOrDefault(t => t.DocTypeId.Equals(documentTypeId, StringComparison.InvariantCultureIgnoreCase));

            // Check if Tenant is empty which can occur if TenantId is incorrectly passed
            if (tenantInfo == null)
            {
                throw new InvalidDataException("TenantId sent is incorrect as no matching value can be found in Approvals. The payload cannot be processed in absence of Tenant Identifier");
            }

            logData.Add(LogDataKey.TenantId, tenantInfo.TenantId);
            logData.Add(LogDataKey.TenantName, tenantInfo.AppName);

            #endregion TenantId pre-condition check

            #region Read the payload content

            if (string.IsNullOrEmpty(payload))
            {
                // Return a response about payload issues
                throw new InvalidDataException("An empty payload was sent");
            }

            #endregion Read the payload content

            #region Get latest ARX type

            // Get the latest ApprovalRequestExpression Type
            Type payloadType = _approvalRequestExpressionHelper.GetCurrrentApprovalRequestExpressionType(documentTypeId);

            if (payloadType == null)
            {
                // Return a response about payload issues
                throw new InvalidDataException("Payload type could not be retrieved for the TenantId provided. Payload cannot be processed. Please try again passing correct TenantId or contact Approvals Service Engineering Team if problem persists.");
            }

            #endregion Get latest ARX type

            #region Process Payload - Validate and Send

            if (!string.IsNullOrWhiteSpace(payload))
            {
                string businessProcessName = string.Empty;
                // Process payload
                PayloadProcessingResult payloadProcessingResult = _payloadReceiver.ProcessPayload(activityId, payloadType, payload, tenantInfo, out string xcv, out string tcv, out string approvalRequestOperationType, out businessProcessName, out Dictionary<string, string> tenantTelemetry);

                if (payloadProcessingResult != null)
                {
                    ITenant tenant = _tenantFactory.GetTenant(tenantInfo);

                    // This method call can be overwritten in inherited classes
                    JObject payloadProcessingResponse = tenant.PayloadProcessingResponse(payloadProcessingResult);

                    if (payloadProcessingResponse != null)
                    {
                        response = payloadProcessingResponse;
                    }

                    logData.Add(LogDataKey.PayloadId, payloadProcessingResult.PayloadId);
                    logData.Add(LogDataKey.Xcv, xcv);
                    logData.Add(LogDataKey.Tcv, tcv);
                    logData.Add(LogDataKey.ReceivedTcv, tcv);
                    logData.Add(LogDataKey.TenantTelemetryData, tenantTelemetry);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(businessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequestOperationType));

                    // Added Logging for Validation Failure cases
                    if (payloadProcessingResult.PayloadValidationResults != null && payloadProcessingResult.PayloadValidationResults.Count > 0)
                    {
                        logData.Add(LogDataKey.PayloadValidationResult, payloadProcessingResult.PayloadValidationResults);
                        _logProvider.LogError(TrackingEvent.PayloadValidationFailure, new Exception(TrackingEvent.PayloadValidationFailure.ToString()), logData);
                    }
                    else
                    {
                        _logProvider.LogInformation(TrackingEvent.PayloadAccepted, logData);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Payload processing result is null or empty. Payload cannot be processed. Please contact Approvals Service Engineering Team.");
                }
            }

            #endregion Process Payload - Validate and Send
        }
        catch (Exception ex)
        {
            logData[LogDataKey.IsCriticalEvent] = CriticalityLevel.Yes.ToString();
            _logProvider.LogError(TrackingEvent.PayloadProcessingFailure, ex, logData);

            throw new WebException("Payload processing failed. Please contact Approvals Engineering Team. Activity Id: " + activityId.ToString(), WebExceptionStatus.SendFailure);
        }

        return response;
    }
}