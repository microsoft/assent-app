// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;

    /// <summary>
    /// The Payload Receiver class
    /// </summary>
    public class PayloadReceiver : IPayloadReceiver
    {
        /// <summary>
        /// The payload validator
        /// </summary>
        private readonly IPayloadValidator _payloadValidator = null;

        /// <summary>
        /// The payload delivery
        /// </summary>
        private readonly IPayloadDelivery _payloadDelivery = null;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider = null;

        public PayloadReceiver(IPayloadValidator payloadValidator, IPayloadDelivery payloadDelivery, IPerformanceLogger performanceLogger, ILogProvider logProvider)
        {
            _payloadValidator = payloadValidator;
            _payloadDelivery = payloadDelivery;
            _performanceLogger = performanceLogger;
            _logProvider = logProvider;
        }

        /// <summary>
        /// Orchestrator for Payload Processing Logic
        /// </summary>
        /// <param name="payloadId"></param>
        /// <param name="payloadType"></param>
        /// <param name="payload"></param>
        /// <param name="payloadDestinationInfo"></param>
        /// <param name="tenant"></param>
        /// <param name="xcv"></param>
        /// <param name="tcv"></param>
        /// <param name="approvalRequestOperationType"></param>
        /// <param name="businessProcessName"></param>
        /// <param name="tenantTelemetry"></param>
        /// <returns></returns>
        public virtual PayloadProcessingResult ProcessPayload
        (
            Guid payloadId,
            Type payloadType,
            string payload,
            PayloadDestinationInfo payloadDestinationInfo,
            ApprovalTenantInfo tenant,
            out string xcv,
            out string tcv,
            out string approvalRequestOperationType,
            out string businessProcessName,
            out Dictionary<string, string> tenantTelemetry
        )
        {
            PayloadProcessingResult payloadProcessingResult = new PayloadProcessingResult(payloadId, null);
            using (
                     _performanceLogger.StartPerformanceLogger("PerfLog", Constants.PayloadReceiver, string.Format(Constants.PerfLogAction, "ProcessPayload", tenant.AppName), new Dictionary<LogDataKey, object>())
                  )
            {
                #region Check for pre-requisites

                if (payloadType == null)
                {
                    throw new ArgumentException("Payload Type is neither provided nor configured", "ApprovalExpressionRequestVersion");
                }

                if (string.IsNullOrEmpty(payload))
                {
                    throw new ArgumentException("An empty payload cannot be processed", "ApprovalExpressionRequest");
                }

                #endregion Check for pre-requisites

                #region Deserialize ARX JSON and throw error if deserialization failed

                // Reconstruct the ApprovalRequestExpression object from JSON serialized payload
                ApprovalRequestExpression approvalRequestExpression = DeserializeAndReconstructPayload(payloadType, payload);

                // If an ApprovalRequestExpression cannot be reconstructed, throw an exception
                if (approvalRequestExpression == null)
                {
                    throw new ArgumentException("The payload provided could not be de-serialized into ApprovalRequestExpression object, though no exception occurred", "ApprovalRequestExpressionPayload");
                }

                if (approvalRequestExpression.Telemetry == null)
                {
                    approvalRequestExpression.Telemetry = new ApprovalsTelemetry()
                    {
                        Xcv = approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber,
                        Tcv = Guid.NewGuid().ToString(),
                        BusinessProcessName = string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequestExpression.Operation),
                        TenantTelemetry = null
                    };
                }
                if (string.IsNullOrEmpty(approvalRequestExpression.Telemetry.Xcv))
                {
                    approvalRequestExpression.Telemetry.Xcv = approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber;
                }
                if (string.IsNullOrEmpty(approvalRequestExpression.Telemetry.Tcv))
                {
                    approvalRequestExpression.Telemetry.Tcv = Guid.NewGuid().ToString();
                }
                if (string.IsNullOrEmpty(approvalRequestExpression.Telemetry.BusinessProcessName))
                {
                    approvalRequestExpression.Telemetry.BusinessProcessName = string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameARConverter, approvalRequestExpression.Operation);
                }
                if (approvalRequestExpression.Telemetry.TenantTelemetry == null)
                {
                    approvalRequestExpression.Telemetry.TenantTelemetry = new Dictionary<string, string>();
                }

                xcv = approvalRequestExpression.Telemetry.Xcv;
                tcv = approvalRequestExpression.Telemetry.Tcv;
                businessProcessName = approvalRequestExpression.Telemetry.BusinessProcessName;
                tenantTelemetry = approvalRequestExpression.Telemetry.TenantTelemetry;
                approvalRequestOperationType = approvalRequestExpression.Operation.ToString();

                LogMessageProgress(
                       new List<ApprovalRequestExpressionV1> { (ApprovalRequestExpressionV1)approvalRequestExpression },
                       TrackingEvent.ARXReceivedSuccessfullyByPayloadReceiver,
                       null,
                       _logProvider,
                       payloadId,
                       tenant,
                       CriticalityLevel.Yes);

                #endregion Deserialize ARX JSON and throw error if deserialization failed

                #region For given ARX - Convert ARX into required type and validate

                List<System.ComponentModel.DataAnnotations.ValidationResult> validateResults = ValidateARX(approvalRequestExpression, tenant, payloadId);

                #endregion For given ARX - Convert ARX into required type and validate

                #region IF validation errors, log and return - ELSE send payload as brokered message and return guid

                // Check if results collection has members in it. If errors exist, then results should be shared back
                // And no message should be sent to Service Bus
                if (validateResults != null && validateResults.Count > 0)
                {
                    #region IF Validation errors occurred - Assign and log payload validation errors

                    // Assign the results to PayloadProcessingResult property "PayloadValidationResults"
                    payloadProcessingResult.PayloadValidationResults = validateResults;

                    // TODO:: Log using activity id i.e. brokered message id
                    LogMessageProgress(
                        new List<ApprovalRequestExpressionV1> { (ApprovalRequestExpressionV1)approvalRequestExpression },
                        TrackingEvent.ARXValidationFailed,
                        new FailureData() { Message = validateResults.Count.ToString() + " business rules failed", ID = ((int)TrackingEvent.ARXValidationFailed).ToString() },
                        _logProvider,
                        payloadId,
                        tenant,
                        CriticalityLevel.No
                        );

                    #endregion IF Validation errors occurred - Assign and log payload validation errors
                }
                else
                {
                    #region ELSE IF Validation Successful - Process payload

                    LogMessageProgress(
                                        new List<ApprovalRequestExpressionV1> { (ApprovalRequestExpressionV1)approvalRequestExpression },
                                        TrackingEvent.ARXValidationSuccess,
                                        null,
                                        _logProvider,
                                        payloadId,
                                        tenant,
                                        CriticalityLevel.No
                                        );

                    // Send the Brokered Message into configured target
                    // Assign a GUID to brokered message as id and return the same to the tenant as response
                    // Add retry logic if send fails - 5 count
                    using (
                            _performanceLogger.StartPerformanceLogger("PerfLog", Constants.PayloadReceiver, string.Format(Constants.PerfLogAction, "GenerateAndSendBrokeredMsg", tenant.AppName), new Dictionary<LogDataKey, object>())
                            )
                    {
                        if (!(_payloadDelivery.SendPayload(approvalRequestExpression, payloadDestinationInfo, payloadId)).Result)
                        {
                            throw new Exception("Payload could not be sent");
                        }
                    }

                    LogMessageProgress(
                                        new List<ApprovalRequestExpressionV1> { (ApprovalRequestExpressionV1)approvalRequestExpression },
                                        TrackingEvent.ARXSentSuccessfullyToServiceBus,
                                        null,
                                        _logProvider,
                                        payloadId,
                                        tenant,
                                        CriticalityLevel.Yes
                                        );

                    #endregion ELSE IF Validation Successful - Process payload
                }

                #endregion IF validation errors, log and return - ELSE send payload as brokered message and return guid
            }

            return payloadProcessingResult;
        }

        /// <summary>
        /// Validate Approval Request Expression
        /// </summary>
        /// <param name="approvalRequestExpression"></param>
        /// <param name="tenant"></param>
        /// <param name="payloadId"></param>
        /// <returns></returns>
        public virtual List<System.ComponentModel.DataAnnotations.ValidationResult> ValidateARX(ApprovalRequestExpression approvalRequestExpression, ApprovalTenantInfo tenant, Guid payloadId)
        {
            // If payload is successfully de-serialized into ApprovalRequestExpression, run business rules
            // Run the type conversion
            // This needs to be changed to use the type passed in and avoid using ApprovalRequestExpressionV1 which is temp being used
            ApprovalRequestExpressionV1 arx = approvalRequestExpression as ApprovalRequestExpressionV1; // TODO:: Code to make payload type detection and usage dynamic
            List<System.ComponentModel.DataAnnotations.ValidationResult> results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            // Run the validations
            using (
                    _performanceLogger.StartPerformanceLogger("PerfLog", Constants.PayloadReceiver, string.Format(Constants.PerfLogAction, "ARXValidation", tenant.AppName), new Dictionary<LogDataKey, object>())
                    )
            {
                LogMessageProgress(
                    new List<ApprovalRequestExpressionV1> { (ApprovalRequestExpressionV1)approvalRequestExpression },
                    TrackingEvent.ARXBusinessRulesValidationStarted,
                    null,
                    _logProvider,
                    payloadId,
                    tenant,
                    CriticalityLevel.No
                    );

                results = _payloadValidator.Validate(arx);

                LogMessageProgress(
                    new List<ApprovalRequestExpressionV1> { (ApprovalRequestExpressionV1)approvalRequestExpression },
                    TrackingEvent.ARXBusinessRulesValidationCompleted,
                    null,
                    _logProvider,
                    payloadId,
                    tenant,
                    CriticalityLevel.No
                    );
            }

            return results;
        }

        /// <summary>
        /// Log Message Progress
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="trackingEvent"></param>
        /// <param name="failureData"></param>
        /// <param name="_logProvider"></param>
        /// <param name="activityId"></param>
        /// <param name="tenant"></param>
        /// <param name="criticalityLevel"></param>
        protected void LogMessageProgress(List<ApprovalRequestExpressionV1> expressions, TrackingEvent trackingEvent, FailureData failureData, ILogProvider _logProvider, Guid activityId, ApprovalTenantInfo tenant, CriticalityLevel criticalityLevel)
        {
            foreach (var expression in expressions)
            {
                StringBuilder approverList = new StringBuilder();
                if (expression.Approvers != null && expression.Approvers.Count > 0)
                {
                    for (int i = 0; i < expression.Approvers.Count; i++)
                    {
                        if (i > 0) { approverList.Append(", "); }
                        approverList.Append(expression.Approvers[i].Alias);
                    }
                }
                Dictionary<LogDataKey, object> tenantLogData = new Dictionary<LogDataKey, object>();

                if (expression != null)
                {
                    tenantLogData.Add(LogDataKey.OperationType, expression.Operation.ToString());
                    if (expression.ApprovalIdentifier != null && !string.IsNullOrEmpty(expression.ApprovalIdentifier.DocumentNumber))
                    {
                        tenantLogData.Add(LogDataKey.DocumentNumber, expression.ApprovalIdentifier.DocumentNumber);
                    }

                    if (expression.ApprovalIdentifier != null && !string.IsNullOrEmpty(expression.ApprovalIdentifier.DisplayDocumentNumber))
                    {
                        tenantLogData.Add(LogDataKey.DisplayDocumentNumber, expression.ApprovalIdentifier.DisplayDocumentNumber);
                        tenantLogData.Add(LogDataKey.DXcv, expression.ApprovalIdentifier.DisplayDocumentNumber);
                    }
                    if (expression.ApprovalIdentifier != null && !string.IsNullOrEmpty(expression.ApprovalIdentifier.FiscalYear))
                    {
                        tenantLogData.Add(LogDataKey.FiscalYear, expression.ApprovalIdentifier.FiscalYear);
                    }

                    if (approverList != null && approverList.Length > 0)
                    {
                        tenantLogData.Add(LogDataKey.Approver, approverList.ToString());
                    }
                }

                tenantLogData.Add(LogDataKey.Xcv, expression.Telemetry != null && !string.IsNullOrEmpty(expression.Telemetry.Xcv) ? expression.Telemetry.Xcv : activityId.ToString());
                tenantLogData.Add(LogDataKey.Tcv, expression.Telemetry != null && !string.IsNullOrEmpty(expression.Telemetry.Tcv) ? expression.Telemetry.Tcv : activityId.ToString());
                tenantLogData.Add(LogDataKey.ReceivedTcv, expression.Telemetry != null && !string.IsNullOrEmpty(expression.Telemetry.Tcv) ? expression.Telemetry.Tcv : activityId.ToString());
                tenantLogData.Add(LogDataKey.TenantTelemetryData, expression.Telemetry != null && expression.Telemetry.TenantTelemetry != null ? expression.Telemetry.TenantTelemetry : new Dictionary<string, string>());
                tenantLogData.Add(LogDataKey.BusinessProcessName, string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameSendPayload, expression.Operation));
                tenantLogData.Add(LogDataKey.TenantId, tenant.DocTypeId);
                tenantLogData.Add(LogDataKey.TenantName, tenant.AppName);
                tenantLogData.Add(LogDataKey.LocalTime, DateTime.UtcNow);
                tenantLogData.Add(LogDataKey._CorrelationId, activityId);
                tenantLogData.Add(LogDataKey.FailureData, failureData);
                tenantLogData.Add(LogDataKey.IsCriticalEvent, criticalityLevel.ToString());

                _logProvider.LogInformation(trackingEvent, tenantLogData);
            }
        }

        /// <summary>
        /// De-serializes and reconstructs the ApprovalRequestExpression object based on derived type provided
        /// Marking this method as virtual so that overriding this behavior is allowed, if required in future
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="payloadType"></param>
        /// <returns></returns>
        public virtual ApprovalRequestExpression DeserializeAndReconstructPayload(Type payloadType, string payload)
        {
            ApprovalRequestExpression approvalRequestExpression = null;
            using (
                      _performanceLogger.StartPerformanceLogger(
                                                    "PerfLog",
                                                    "DeserializePayload",
                                                    string.Format(Constants.PerfLogCommon, "Detail"),
                                                    new Dictionary<LogDataKey, object>()
                                                )
                    )
            {
                // Converting the payload into ARX
                approvalRequestExpression = payload.FromJson(payloadType) as ApprovalRequestExpression;

                // Changes the case of aliases to lowercase and OpertaionType to Uppercase
                ARXCaseConversion(approvalRequestExpression);
            }

            return approvalRequestExpression;
        }

        /// <summary>
        /// Changes the case of aliases to lowercase and OpertaionType to Uppercase
        /// TODO:: A better implementation is possible and should be taken up for refactoring as time permits
        /// </summary>
        /// <param name="approvalRequestExpression"></param>
        private void ARXCaseConversion(ApprovalRequestExpression approvalRequestExpression)
        {
            // If ARX is null, then the function can return as there is nothing to modify
            // This check is also to ensure each subsequent condition in this function need not check for nulls
            // Not throwing a costly exception and simply returning a null expecting caller to handle nulls
            if (approvalRequestExpression == null)
            {
                return;
            }

            #region Case Conversion

            if (approvalRequestExpression.Approvers != null && approvalRequestExpression.Approvers.Count > 0)
            {
                for (int i = 0; i < approvalRequestExpression.Approvers.Count; i++)
                {
                    approvalRequestExpression.Approvers[i].Alias = approvalRequestExpression.Approvers[i].Alias.ToLower();
                }
            }
            if (approvalRequestExpression.DeleteFor != null && approvalRequestExpression.DeleteFor.Count > 0)
            {
                approvalRequestExpression.DeleteFor = approvalRequestExpression.DeleteFor.ConvertAll(a => a.ToLower());
            }

            if (approvalRequestExpression.SummaryData != null && approvalRequestExpression.SummaryData.ApprovalHierarchy != null)
            {
                for (int i = 0; i < approvalRequestExpression.SummaryData.ApprovalHierarchy.Count; i++)
                {
                    for (int j = 0; j < approvalRequestExpression.SummaryData.ApprovalHierarchy[i].Approvers.Count; j++)
                    {
                        approvalRequestExpression.SummaryData.ApprovalHierarchy[i].Approvers[j].Alias = approvalRequestExpression.SummaryData.ApprovalHierarchy[i].Approvers[j].Alias.ToLower();
                    }
                }
            }
            if (approvalRequestExpression.SummaryData != null && approvalRequestExpression.SummaryData.Submitter != null)
            {
                approvalRequestExpression.SummaryData.Submitter.Alias = approvalRequestExpression.SummaryData.Submitter.Alias.ToLower();
            }
            if (approvalRequestExpression.DetailsData != null && approvalRequestExpression.DetailsData.Count > 0)
            {
                Dictionary<string, string> detailsData = new Dictionary<string, string>();

                foreach (var item in approvalRequestExpression.DetailsData)
                {
                    detailsData.Add(item.Key.ToUpper(), item.Value);
                }
                approvalRequestExpression.DetailsData = detailsData;
            }

            #endregion Case Conversion
        }
    }
}