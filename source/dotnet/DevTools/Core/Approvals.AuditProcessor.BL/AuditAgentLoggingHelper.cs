// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.BL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.AuditProcessor.BL.Interface;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Newtonsoft.Json.Linq;

    public class AuditAgentLoggingHelper : IAuditAgentLoggingHelper
    {
        /// <summary>
        /// The approvalTenantInfo helper
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="approvalTenantInfoHelper"></param>
        /// <param name="logProvider"></param>
        public AuditAgentLoggingHelper(IApprovalTenantInfoHelper approvalTenantInfoHelper, ILogProvider logProvider)
        {
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _logProvider = logProvider;
        }

        /// <summary>
        /// Takes a BrokeredMessage and a TrackingEvent and fetches the message details so it can be meaningfully logged
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="brokeredMessage"></param>
        public async Task Log(TrackingEvent trackingEvent, Message brokeredMessage, DateTime loggingTime, List<ApprovalRequestExpressionExt> approvalNotificationARXObj = null)
        {
            Message newBrokeredMessage = brokeredMessage.Clone();
            string brokeredMessageID = newBrokeredMessage.MessageId;
            if (approvalNotificationARXObj != null)
            {
                ApprovalTenantInfo tenant = (await _approvalTenantInfoHelper.GetTenants(false)).Where(t => t.DocTypeId.Equals(approvalNotificationARXObj.FirstOrDefault().DocumentTypeId.ToString(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                string exceptionMessage = (tenant == null) ? "Tenant Entity is null and hence the ARN cannot be processed further" : "";
                foreach (ApprovalRequestExpressionExt approvalRequestNotificationExt in approvalNotificationARXObj)
                {
                    var arxJSON = approvalRequestNotificationExt.ToJson();
                    Log(trackingEvent, brokeredMessageID, arxJSON, tenant, loggingTime, exceptionMessage);
                }
            }
            else
            {
                Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();

                #region Populate logData

                logData[LogDataKey._ActivityId] = brokeredMessageID;
                logData[LogDataKey.BrokerMessage] = newBrokeredMessage.UserProperties;
                logData[LogDataKey.LocalTime] = loggingTime;

                #endregion Populate logData

                _logProvider.LogInformation(trackingEvent, logData);
            }
        }

        /// <summary>
        /// Logs to either the logging docdb or AI depending on event id configuration
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="brokeredMessageID"></param>
        /// <param name="arn"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="exceptionMessage"></param>
        public void Log(TrackingEvent trackingEvent, string brokeredMessageID, string arn, ApprovalTenantInfo tenantInfo, DateTime loggingTime, string exceptionMessage = "")
        {
            JToken approvalRequest = (arn).ToJToken();

            #region Data Validation / Formatting

            JToken fiscalYear = approvalRequest["ApprovalIdentifier"]["FiscalYear"].HasValues ? approvalRequest["ApprovalIdentifier"]["FiscalYear"] : "";

            string operationType = string.Empty;
            switch ((int)approvalRequest["Operation"])
            {
                case 1:
                    operationType = "Create";
                    break;

                case 2:
                    operationType = "Update";
                    break;

                case 3:
                    operationType = "Delete";
                    break;
            }

            Dictionary<string, object> failureData = null;
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                failureData = new Dictionary<string, object>
                {
                    { "ID", trackingEvent },
                    { "Message", exceptionMessage }
                };
            }

            StringBuilder approverList = new StringBuilder();
            if (approvalRequest["Approvers"] != null)
            {
                for (int i = 0; i < approvalRequest["Approvers"].ToList().Count; i++)
                {
                    if (i > 0) { approverList.Append(", "); }
                    approverList.Append(approvalRequest["Approvers"][i]["Alias"]);
                }
            }

            #endregion Data Validation / Formatting

            Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey._ActivityId,  brokeredMessageID },
                { LogDataKey.DocumentNumber,  approvalRequest["ApprovalIdentifier"]["DocumentNumber"].ToString() },
                { LogDataKey.FiscalYear,  fiscalYear.ToString() },
                { LogDataKey.DisplayDocumentNumber,  approvalRequest["ApprovalIdentifier"]["DisplayDocumentNumber"].ToString() },
                { LogDataKey.TenantId,  tenantInfo != null ? tenantInfo.DocTypeId : "" },
                { LogDataKey.TenantName,  tenantInfo != null ? tenantInfo.AppName : "" },
                { LogDataKey.Operation,  approvalRequest["Operation"].ToString() },
                { LogDataKey.LocalTime,  loggingTime },
                { LogDataKey.OperationType,  operationType },
                { LogDataKey.Approver,  approverList.ToString() },
                { LogDataKey.FailureData,  failureData },
                { LogDataKey.Xcv,  approvalRequest["Telemetry"]["Xcv"].ToString() },
                { LogDataKey.Tcv,  approvalRequest["Telemetry"]["Tcv"].ToString() },
                { LogDataKey.DXcv,  approvalRequest["ApprovalIdentifier"]["DocumentNumber"].ToString() }
            };

            // Log the record to docDB
            _logProvider.LogInformation(trackingEvent, logData);
        }
    }
}