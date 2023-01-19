// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts
{
    using System;

    public static class Constants
    {
        #region ApprovalRequestExpression

        public const int ARXisNullEventId = 1001;
        public const string ARXisNullMessage = "ApprovalRequestExpression should have a value but is null.";

        public const int ARXApproversCountZeroEventId = 1002;
        public const string ARXApproversCountZeroMessage = "ApprovalRequestExpression.Approvers list cannot be null and the collection cannot be empty.";

        public const int ARXDocTypeIdNullorEmptyEventId = 1003;
        public const string ARXDocTypeIdNullorEmptyMessage = "ApprovalRequestExpression.DocumentTypeId cannot be empty or null.";

        public const int ARXandSummaryTenantIdMatchEventId = 1004;
        public const string ARXandSummaryTenantIdMatchMessage = "ApprovalRequestExpression.DocumentTypeId and SummaryJSON.DocumentTypeId should match.";

        public const int ARXApproversNullEventId = 1005;
        public const string ARXApproversNullMessage = "ApprovalRequestExpression.Approver cannot be null.";

        public const string DeleteForNullMessage = "ApprovalRequestExpression.DeleteFor cannnot be null";
        public const string DocTypeIdEmpty = "ApprovalRequestExpression.DocumentTypeId cannot be empty";

        #endregion ApprovalRequestExpression

        #region ApprovalIdentifier

        public const int ApprovalIdentifierNullEventId = 2001;
        public const string ApprovalIdentifierNullMessage = "ApprovalIdentifier cannot be null.";

        public const int ApprovalIdentifierDocNumberEventId = 2002;
        public const string ApprovalIdentifierDocNumberMessage = "ApprovalIdentifier.DocumentNumber cannot be null or empty.";

        public const int ApprovalIdentifierDispDocNumberEventId = 2003;
        public const string ApprovalIdentifierDispDocNumberMessage = "ApprovalIdentifier.DisplayDocumentNumber cannot be null or empty.";

        #endregion ApprovalIdentifier

        #region Approver

        public const int ApproverNullEventId = 3001;
        public const string ApproverNullMessage = "Approver cannot be null.";

        #endregion Approver

        #region ActionDetail

        public const int ActionDetailNullEventId = 4001;
        public const string ActionDetailNullMessage = "ActionDetail cannot be null.";

        public const int ActionDetailNameEventId = 4002;
        public const string ActionDetailNameMessage = "ActionDetail.Name cannot be null or empty.";

        public const int ActionDetailDateEventId = 4003;
        public const string ActionDetailDateMessage = "ActionDetail.Date cannot be null or empty.";

        public const int ActionDetailActionByEventId = 4004;
        public const string ActionDetailActionByMessage = "ActionDetail.ActionBy cannot be null.";

        #endregion ActionDetail

        #region NameAliasEntity

        public const int NameAliasEntityNullEventId = 5001;
        public const string NameAliasEntityNullMessage = "NameAliasEntity cannot be null.";

        public const int NameAliasEntityAliasNullEventId = 5002;
        public const string NameAliasEntityAliasNullMessage = "NameAliasEntity.Alias cannot be null or empty.";

        public const int NameAliasEntityAliasInvalidCharsEventId = 5003;
        public const string NameAliasEntityAliasInvalidCharsMessage = "NameAliasEntity.Alias has invalid characters.";

        public const int NameAliasEntityNameNullEventId = 5004;
        public const string NameAliasEntityNameNullMessage = "NameAliasEntity.Name cannot be null or empty.";

        #endregion NameAliasEntity

        #region ApproverHeirarchy

        public const int ApproverHeirarchyNullEventId = 6001;
        public const string ApproverHeirarchyNullMessage = "ApproverHeirarchy cannot be null.";

        public const int ApproverHeirarchyCountEventId = 6002;
        public const string ApproverHeirarchyCountMessage = "ApproverHeirarchy.Approvers collection cannot be empty.";

        public const string ApprovalHierarchyZeroCount = "ApprovalHierarchy collection cannot be empty";

        #endregion ApproverHeirarchy

        #region NotificationDetail

        public const int NotificationDetailTemplateKeyNullEventId = 7001;
        public const string NotificationDetailTemplateKeyNullMessage = "NotificationDetail.TemplateKey cannot be null or empty.";

        public const int NotificationDetailToEventId = 7003;
        public const string NotificationDetailToMessage = "NotificationDetail.To cannot be null or empty.";

        #endregion NotificationDetail

        #region ReminderDetail

        public const string ReminderDetailDataMessage = "Either Reminder dates or Frequence and Expiration are mandatory";

        public const int ReminderDetailReminderTemplateEventId = 8001;
        public const string ReminderDetailReminderTemplateMessage = "ReminderTemplate cannot be null or empty.";

        public const int ReminderDetailDateEventId = 8002;
        public const string ReminderDetailDateMessage = "ReminderTemplate.ReminderDates collection cannot be null or empty.";

        public const int ReminderDetailFrequencyEventId = 8003;
        public const string ReminderDetailFrequencyMessage = "ReminderTemplate.Frequency cannot be zero.";

        #endregion ReminderDetail

        #region BusinessRule

        public const int ARXActionDetailEventId = 9001;
        public const string ARXActionDetailMessage = "ApprovalRequestExpression.ActionDetail cannot be null.";

        #endregion BusinessRule

        #region SummaryJson

        public const int SummaryJsonDocTypeIdNullorEmptyEventId = 10001;
        public const string SummaryJsonDocTypeIdNullorEmptyMessage = "DocumentTypeId cannot be null or empty.";

        public const int SummaryJsonTitleNullorEmptyEventId = 10002;
        public const string SummaryJsonTitleNullorEmptyMessage = "SummaryJson.Title cannot be null or empty.";

        public const int SummaryJsonUnitOfMeasureNullorEmptyEventId = 10003;
        public const string SummaryJsonUnitOfMeasureNullorEmptyMessage = "SummaryJson.UnitOfMeasure cannot be null or empty.";

        public const int SummaryJsonUnitValueNullorEmptyEventId = 10004;
        public const string SummaryJsonUnitValueNullorEmptyMessage = "SummaryJson.UnitValue cannot be null or empty.";

        public const int SummaryJsonSubmitterNullorEmptyEventId = 10005;
        public const string SummaryJsonSubmitterNullorEmptyMessage = "SummaryJson.Submitter cannot be null or empty.";

        public const int SummaryJsonSubmittedDateNullorEmptyEventId = 10006;
        public const string SummaryJsonSubmittedDateNullorEmptyMessage = "SummaryJson.SubmittedDate cannot be null or empty.";

        public const int SummaryJsonApprovalIdentifierEventId = 10007;
        public const string SummaryJsonApprovalIdentifierMessage = "SummaryJson.ApprovalIdentifier cannot be null.";

        public const int SummaryJsonNullEventId = 10008;
        public const string SummaryJsonNullMessage = "SummaryJson cannot be null or empty.";

        #endregion SummaryJson

        public const string ApplicationName = "Approvals";
        public const string ServiceTreeAppId = "0685ecb0-8dfa-4ff5-9485-31d4ba8e867f";

        public const string AzureAppConfiguration = "AzureAppConfiguration";
        public const string AzureAppConfigurationUrl = "AzureAppConfigurationUrl";
        public const string AppConfigurationLabel = "AppConfigurationLabel";
        public const string MustUpdateConfig = "MustUpdateConfig";

        public const string CurrentApprover = "CurrentApprover";
        public const string AdaptiveDTL = "AdaptiveDTL";
        public const string FailureEmailNotificationTemplateKey = "EmailNotificationForDetailsFail";

        public const string TenantTypeProduction = "Prod";

        public const string DocumentNumber = "DocumentNumber";
        public const string DisplayDocumentNumber = "DisplayDocumentNumber";
        public const string ApprovalIdentifierDisplayDocNumber = "ApprovalIdentifier.DisplayDocumentNumber";
        public const string ApprovalIdentifier = "ApprovalIdentifier";
        public const string ActionKey = "Action";
        public const string ActionDetailsKey = "ActionDetails";
        public const string ActionDateKey = "ActionDate";
        public const string CommentKey = "Comment";
        public const string ReasonTextKey = "ReasonText";
        public const string ReasonCodeKey = "ReasonCode";
        public const string RequestVersion = "RequestVersion";
        public const string WireRequestID = "WireRequestID";
        public const string FiscalYear = "FiscalYear";
        public const string Approver = "Approver";
        public const string OriginalApprover = "OriginalApprover";
        public const string DocumentTypeId = "DocumentTypeId";
        public const string Operation = "Operation";
        public const string Requestor = "Requestor";
        public const string DocumentKeys = "DocumentKeys";
        public const string TargetPageKey = "_TargetPage";
        public const string DelayTimeKey = "_DelayTime";
        public const string TelemetryContractName = "appInsightsContract";
        public const string AppinsightsInstrumentationkey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string StorageAccountName = "StorageAccountName";
        public const string StorageAccountKey = "StorageAccountKey";
        public const string AuthKey = "AuthKey";
        public const string ClientID = "ClientID";
        public const string AADInstanceName = "AADInstanceName";
        public const string ResourceURL = "ResourceURL";
        public const string TenantID = "TenantID";
        public const string KeyVaultUri = "KeyVaultUri";
        public const string AppConfigurationSecrets = "AppConfigurationSecrets";

        public const string PullModelURLPlaceHolderStart = "\\|#";
        public const string PullModelURLPlaceHolderEnd = "#\\|";

        public static readonly Guid InvoiceDocumentTypeId = new Guid("f30862f8-9be9-482b-b312-d38c8f56c745");

        public const string HmacUri = "Uri type is Hmac";
        public const string AcsUri = "Uri type is AcsUri";

        public const string PerfLogActionWithInfo = "{0} - {1} - {2}";
        public const string PerfLogAction = "{0} - {1}";
        public const string PerfLogCommon = "{0}";

        public const string PerfLogWatchdogEmailProcessing = "Watchdog Emails Processing Time";

        public const string CommonTemplates = "Common";

        /// <summary>
        /// Send Email Notification With Actionable actions
        /// </summary>
        public const string EmailNotificationWithActionTemplateKey = "WithAction";

        /// <summary>
        /// Send email notification with details no action button
        /// </summary>
        public const string EmailNotificationWithDetailsTemplateKey = "WithDetails";

        public const string NotificationFrameworkProvider = "NotificationFrameworkProvider";

        public const string RoutingIdColumnName = "RoutingId";
        public const string CosmicApproverDetailsIdKey = "ApproverDetailId";

        public const string AzureTableRowKeyStandardPrefix = "{0}" + Constants.FieldsSeparator;
        public const string FieldsSeparator = "|";
        public const string ApprovalDetailsAzureTableName = "ApprovalDetails";
        public const string ApprovalSummaryAzureTableName = "ApprovalSummary";
        public const string ApprovalEmailNotificationTemplatesAzureTableName = "ApprovalEmailNotificationTemplates";
        public const string TransactionHistoryTableName = "TransactionHistory";
        public const string ApprovalAzureBlobContainerName = "approvalblobdata";
        public const string TenantImagesBlobContainerName = "tenanticons";
        public const string PrimaryMessageContainer = "primaryapprovalsmessage";
        public const string AuditAgentMessageContainer = "auditagentapprovalsmessage";
        public const string NotificationMessageContainer = "notificationapprovalsmessage";
        public const string FlightingAzureTableName = "Flighting";
        public const string FlightingUserPreferenceAzureTableName = "UserPreference";
        public const string FlightingFeatureAzureTableName = "FlightingFeature";
        public const string FlightingRingAzureTableName = "FlightingRing";
        public const string UserDelegationSettingsAzureTableName = "UserDelegationSetting";
        public const string UserDelegationSettingsHistoryAzureTableName = "UserDelegationSettingsHistory";
        public const string NotificationImagesBlobName = "notificationimages";
        public const string NotificationAttachmentsBlobName = "outlookattachments";
        public const string OutlookDynamicTemplates = "outlookdynamictemplates";
        public const string OutlookActionableEmailIcons = "outlookactionableemailicons";

        public const string DataString = "wrap_name={0}&wrap_password={1}&wrap_scope={2}";
        public const string ContentTypeKeyName = "Content-Type";
        public const string ContentTypeKeyValue = "application/x-www-form-urlencoded";
        public const string PostString = "POST";
        public const char SplitToken = '&';
        public const string Wrap_Access_TokenKeyName = "wrap_access_token=";
        public const char SplitToken2 = '=';
        public const string AuthorizationKeyString = "WRAP access_token=\"{0}\"";

        public const string OperationTypeAction = "ACT";
        public const string OperationTypeOutOfSync = "OSA";
        public const string ActionByAlias = "ActionByAlias";
        public const string WebClient = "Web";

        public const string WorkerRole = "Worker";
        public const string OutlookClient = "Outlook";
        public const string TeamsClient = "Teams";
        public const string ReactClient = "React";
        public const string PayloadReceiver = "PayloadReceiver";
        public const string ContentTypeJson = "application/json";
        public const string AdditionalData = "AdditionalData";
        public const string AuthenticationType = "AuthenticationType";
        public const string AuthorizationHeaderScheme = "Bearer";
        public const string ClientDeviceHeader = "ClientDevice";
        public const string FilterParameters = "FilterParameters";
        public const string AuthorizationHeader = "Authorization";
        public const string CookieHeader = "Cookie";
        public const string UserAlias = "UserAlias";
        public const string LoggedInUserAlias = "LoggedInUserAlias";
        public const string DelegatedUserAlias = "DelegatedUserAlias";
        public const string DetailOpsList = "DetailOpsList";
        public const string OperationType = "operationtype";
        public const string ServiceParameter = "ServiceParameter";
        public const string OperationTypeSummary = "Summary";
        public const string SourcePendingApproval = "PendingApproval";
        public const string digitalSignatureReplace = "digitalSignature=&";
        public const string DocumentDownloadAction = "DOC1";
        public const string DocumentPreviewAction = "DOCUMENTPREVIEW";
        public const string BulkDocumentDownloadAction = "BULKDOC";

        public const string SingleDownloadAction = "Download";

        public const string AuthSumOperationType = "authsum";
        public const string ApprovalChainOperation = "APPRCHAIN";
        public const string ProcurementDetailAction = "DTL";
        public const string HeaderOperationType = "HDR";
        public const string SummaryOperationType = "SUM";
        public const string EditedDetailsOperationType = "EditedDetails";
        public const string TransactionDetailsOperationType = "TransactionDetails";
        public const string AdditionalDetails = "ADDNDTL";

        public const string EditableField = "EditableField";
        public const string EditableFields = "EditableFields";
        public const string EditableFieldsAuditTrail = "EditableFieldsAuditTrail";
        public const string LineItems = "LineItems";

        public const string DetailPageType = "detail";

        public const string Messages = "Messages";
        public const string BudgetIndicator = "BudgetIndicator";
        public const string OverBudget = "OverBudget";
        public const string IsConfidential = "IsConfidential";
        public const string StatusAlert = "StatusAlert";

        public const string UserDelegationPostError = "The delegation cannot be saved at this time, please try again later or submit a support ticket.";
        public const string UserDelegationGetError = "Error fetching User Delegation settings.";
        public const string ExistingDelegateError = "There is an existing delegation for the selected permission, please remove existing delegation for the selected permission or choose a different permission option.";
        public const string InvalidDelegateAlias = "Delegate alias is invalid!";
        public const string UserDelegationEndDateError = "End Date should be equal to or greater than Start Date";

        public const string UserPreferencePostError = "The user preference cannot be saved at this time, please try again later or submit a support ticket.";
        public const string UserPreferenceGetError = "Error fetching User Preference details.";

        public const string ActionSuccessfulMessage = "Your action has been submitted for processing.";

        public const string GenericErrorMessage = "Action failed. Please try again later. If this issue persists, please send email to {0} with the {1}";
        public const string TenantDowntimeMessage = "{0} is currently undergoing maintenance. Please try after some time. If this issue persists, please send email to {1}";

        public const string TenantDetailFetchFailureMessage = "Error loading request details: We could not retrieve details data from the line of business application. Approval actions cannot be submitted at this time. Please refresh or try opening this page later.";

        public const string NotFlightedMornUIErrorMessage = "Tenant/ User not flighted to the modern UI experience";

        public const string DownloadAllAttachmentsFile = "AllAttachments.zip";

        public const string AdaptiveTemplateVersion = "1.0";

        #region SupportPortalConstants

        public const string TableNameTenantDownTimeMessages = "TenantDownTimeMessages";

        #endregion SupportPortalConstants

        #region Monitoring

        public const string CultureName = "en-US";
        public const string ApproverChainHeader = "<table>{0}</table>";
        public const string ApproverChainCurrent = "<tr><td> [current] <b>{0}</b> [{1}] {2}</td></tr>";
        public const string ApproverChain = "<tr><td> <b>{0}</b> [{1}] {2} - {3} {4}</td></tr>";
        public const string ApproverChainFuture = "<tr><td> <b>{0}</b> [{1}] {2} {3} </td></tr>";
        public const string ApproverChainNotes = "<tr> <!--[if (mso)]> <td style='margin-left:25px'> <![endif]--> <!--[if !(mso)]><!--> <td style='padding-left:25px'> <!--<![endif]-->  <i>{0}</i> {1} </td></tr>";

        #endregion Monitoring

        #region Telemetry & Logging

        public const string FeatureUsageEvent = "FeatureUsageEvent";
        public const string BusinessProcessEvent = "BusinessProcessEvent";
        public const string BusinessProcessNameSendPayload = "SendPayload";
        public const string BusinessProcessNameSendNotificationToUser = "SendNotificationToUser";
        public const string BusinessProcessNameGetAttachmentContentFromLob = "GetAttachmentContentFromLob";
        public const string BusinessProcessNameSendNotificationAll = "All";
        public const string BusinessProcessNameSendNotificationDeviceNotification = "DeviceNotification";
        public const string BusinessProcessNameSendNotificationEmail = "Email";
        public const string BusinessProcessNameSendNotificationWatchdogReminder = "WatchdogReminder";
        public const string BusinessProcessNameGetSummary = "GetSummary";
        public const string BusinessProcessNameGetSummaryFromTenant = "GetSummaryFromTenant";
        public const string BusinessProcessNameAddSummaryCopy = "AddSummaryCopy";
        public const string BusinessProcessNameSaveEditedDetails = "SaveEditedDetails";
        public const string BusinessProcessNameSumamryFromARX = "FromARX";
        public const string BusinessProcessNameSumamryFromBackChannel = "BackChannel";
        public const string BusinessProcessNameAddDetails = "AddDetails";
        public const string BusinessProcessNameGetDetails = "GetDetails";
        public const string BusinessProcessNameGetDocuments = "GetDocuments";
        public const string BusinessProcessNameGetDetailsFromTenant = "GetDetailsFromTenant";
        public const string BusinessProcessNameDetailsFromStorage = "FromStorage";
        public const string BusinessProcessNameDetailsPrefetched = "Prefetched";
        public const string BusinessProcessNameUserTriggered = "UserTriggered";
        public const string BusinessProcessNameDetailsWorkerTriggered = "WorkerTriggered";
        public const string BusinessProcessNameApprovalAction = "ApprovalAction";
        public const string BusinessProcessNameDocumentApprovalStatus = "DocumentApprovalStatus";
        public const string BusinessProcessNameARConverter = "ARConverter";

        public const string OutOfSyncAction = "OutOfSync";
        public const string UndoOutOfSyncAction = "UndoOutOfSync";
        public const string OfflineApproval = "OfflineApproval";

        public const string SuccessStatus = "Success";
        public const string FailedStatus = "Failed";

        public const string BusinessProcessNameGetActionDetailsFromTenant = "GetActionDetailsFromTenant";

        #endregion Telemetry & Logging

        #region Request/Response Headers Mappings

        public const string XcvMappingKey = "XcvMappingKey";
        public const string TcvMappingKey = "TcvMappingKey";
        public const string Xcv = "Xcv";
        public const string Tcv = "Tcv";
        public const string MessageId = "MessageId";
        public const string TenantId = "TenantId";

        #endregion Request/Response Headers Mappings

        #region OutlookConstants

        /// <summary>
        /// The Summary AdaptiveJSON Body
        /// </summary>
        public const string SUMMARYBODYTEMPLATE = "adaptiveSummaryBodyTemplate";

        /// <summary>
        /// The Attachment AdaptiveJSON Body
        /// </summary>
        public const string ATTACHMENTSTEMPLATE = "adaptiveAttachmentTemplate";

        /// <summary>
        /// The Common Message AdaptiveJSON Body
        /// </summary>
        public const string COMMONTEMPLATE = "commonMessage";

        /// <summary>
        /// The AdaptiveJSON Body
        /// </summary>
        public const string BODYTEMPLATE = "adaptiveBodyTemplate";

        /// <summary>
        /// The AdaptiveJSON Action
        /// </summary>
        public const string ACTIONTEMPLATE = "adaptiveActionTemplate";

        /// <summary>
        /// The AdaptiveJSON Footer
        /// </summary>
        public const string FOOTERTEMPLATE = "adaptiveFooterTemplate";

        /// <summary>
        /// The AdaptiveJson Action response
        /// </summary>
        public const string ACTIONRESPONSETEMPLATE = "adaptiveTemplateResponse";

        /// <summary>
        /// The AdaptiveJson Error Action response
        /// </summary>
        public const string ACTIONERRORRESPONSETEMPLATE = "adaptiveTemplateErrorResponse";

        /// <summary>
        /// The AdaptiveJSON for complete email body
        /// </summary>
        public const string CONFIGURATION = "configuration.json";

        /// <summary>
        /// The detailtemplate
        /// </summary>
        public const string MAINTEMPLATE = "mainTemplate.html";

        /// <summary>
        /// The attachment Template
        /// </summary>
        public const string ATTACHMENTTEMPLATE = "attachmentTemplate.html";

        /// <summary>
        /// The client device
        /// </summary>
        public const string ClientDevice = "Outlook";

        /// <summary>
        /// Default UserImage in base64String
        /// </summary>
        public const string DefaultUserImageBase64String = "iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAADy0lEQVRoQ+2ZZ0skQRCGa80Bc85ZUVQMCP5/8JsKBgRF15wVM+bAM3cre547013d6y3HFgzLMt3T9fbbFTs2MzPzLv+BxLJAMozFLCMZRohkGckykqYdSOvRysnJEZ5YLCavr6/y9vaWJhji30Zyc3OlvLxcysrKpKio6A8gj4+Pcnd3J5eXl/Ly8uIVlFdGampqpK2tLQAAC0ji9/39VwLB79PTkxwcHMjx8XHw34d4AZKfny8tLS3S2NgYMGAiADg/P5etra0AmKs4AykuLpbOzk6pqqpS6XJ7eyvxeFz4dREnIDDR0dEhdXV1n0fIVhmYwWY2Nzfl4eHBdvrneCcg9fX10t3dbXycUmkJmN3dXdnb2/t5INjC6OiolJSUqBdPnoidLC0tCZ5NI2pGqqurZWBgQH2kvlN2e3tb9vf3NTj0cWRoaEgqKytVi6aahI3Mz8+rvqlihNgwPT3tlY2E9nNzc6rjpQJCwJuYmFDtXNSk5eVlub6+jhr213sVEAx8bGzMejGTCSsrK4E7thUVkMLCQpmcnLRdy2g8nuvm5sZobPIgFRBsZGpqSvLy8qwXDJtAPJmdnZXn52fr76qAsEp/f7/U1tZaLxg2ASZgRCNqIKTpw8PDXj3X+vq6nJycaHDo4wirAQRAiVRdpcHvSff397KwsKAuvtSMsD7RnVyroKDABUNQZO3s7MjR0ZH6O05AyLeoQ3hM65CvmmLgFFgAcakanYCgFAC6urqkoaFBtZvEjLW1NScQLOwMJKF9U1OT8BBjomwGFnCxGDbpu49y1wsQWKmoqJDm5uag8WAChCYEme7FxYXawJ0DYvIHULy1tVVKS0uDABkFIjEXFrAJvBWsXF1dqY5mYpKaERSGAQzdNcLT86KrAkPa3pcKCEkjbR/crykDUdsNQxwz2OHY2Yo1EI4QXRNsIh1CmkKlaJvKWwEh8PX19RkZtBYkzMAILtmmq2IMBM/U09MTtH5+Qs7OzoTcy9RmjIG0t7cHhu3LJqI2A2YODw+DY2YSZ4yAYA90TFy9U5TyX9/DxurqauAEoiQSCN313t5erx4qSqnk96QwgMFFh0kkEF8Zro3yyWNJZWh0n56e6oFgD6TptEZ/yja+y44x/I2NjVDDD2UEmxgfHxea1f9SSGUousLaqaFASM1xuZkgdOvxYqkkFMjIyEhQymaCcH+yuLhoD4TjRO9KW/n5Bo8rpi+c6nYrJSPcQA0ODvrWx+l7uGGu676TlECoMYjmmSRcBFHbWwEhCOJ2M0lwwySTVkDScf/huilhncgPRNYrjbKdJSQAAAAASUVORK5CYII=";

        /// <summary>
        /// Default UserImage for Teams in base64String
        /// </summary>
        public const string DefaultTeamsUserImageBase64String = "PHN2ZyB3aWR0aD0iMzIiIGhlaWdodD0iMzIiIHZpZXdCb3g9IjAgMCAzMiAzMiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayI+CjxwYXRoIGZpbGwtcnVsZT0iZXZlbm9kZCIgY2xpcC1ydWxlPSJldmVub2RkIiBkPSJNMTYgMzJDMjQuODM2NiAzMiAzMiAyNC44MzY2IDMyIDE2QzMyIDcuMTYzNDQgMjQuODM2NiAwIDE2IDBDNy4xNjM0NCAwIDAgNy4xNjM0NCAwIDE2QzAgMjQuODM2NiA3LjE2MzQ0IDMyIDE2IDMyWiIgZmlsbD0idXJsKCNwYXR0ZXJuMCkiLz4KPGRlZnM+CjxwYXR0ZXJuIGlkPSJwYXR0ZXJuMCIgcGF0dGVybkNvbnRlbnRVbml0cz0ib2JqZWN0Qm91bmRpbmdCb3giIHdpZHRoPSIxIiBoZWlnaHQ9IjEiPgo8dXNlIHhsaW5rOmhyZWY9IiNpbWFnZTAiIHRyYW5zZm9ybT0ic2NhbGUoMC4wMDk2MTUzOCkiLz4KPC9wYXR0ZXJuPgo8aW1hZ2UgaWQ9ImltYWdlMCIgd2lkdGg9IjEwNCIgaGVpZ2h0PSIxMDQiIHhsaW5rOmhyZWY9ImRhdGE6aW1hZ2UvcG5nO2Jhc2U2NCxpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBR2dBQUFCb0NBWUFBQUFkSExXaEFBQUFDWEJJV1hNQUFCWWxBQUFXSlFGSlVpVHdBQUFBQVhOU1IwSUFyczRjNlFBQUFBUm5RVTFCQUFDeGp3djhZUVVBQUFUWFNVUkJWSGdCN1oxZFN4dEJGSWJIWWk3aWhRRU5hS0FLVmFoQ1c2cFNXbXpGMWw5ZVdrcDdaeFcxV0NGS1ZWQmhJOFNMNUNKQ3UrOG1zNTFzVTV0UDV4MzZQaER5UVdMQ1BqdXpaODQ1dTQ3OU9Dbi9OSUtXQjBaUUkwSGtTQkE1RWtTT0JKRWpRZVJJRURrU1JJNEVrU05CNUVnUU9SSkVqZ1NSSTBIa1NCQTVFa1NPQkpFalFlUklFRGtTUkk0RWtTTkI1RWdRT1JKRWpnU1JJMEhrU0JBNTR5WmdHbzJHS1IrZm1DaXFtRnE5Ym1xMVd2SjZMamR1Q3BNRlV5ck5tTkxzckptWW1EQ2hNaFppOHp4RWJIL2ROVkdsMHRYNzUrY2VtdVdseDBHS0NrNVErZmpZSEg0L2lrZlBiVStmeStWeWlhVEZoVWNtSklLYTRpQUdONWZpOUxTWm41K0w3NmZTRVZLdFZ1UGJqYm00dklwdmw4bHJtQTczOWcrU2U0Z0toV0JHRUViTzN2NjM5UGxFUG0vV1ZsZE1zVGg5NStjd0hYNzYvQ1crcjZldlBYdjZKSmlSRkVRVWg0M3NqcHhDWWRKc3ZkdjhweHlBVWJYMWRqTU9HaWJUMS9DM2JFREJUaENDM0dNT1JzN0c2L1hrbU5JdGVPL0dtL1ZZVmo1NWpta09RVVlJMEF2Q25uNTZkcDQreDRidVJZNEZuMWxiV1VtZkl3S0VLSGJvQmRtRFBCaDBUWU1wRVVHRkJXc29kdmdGWFZ5bGowdXpNMlpRRVBGWnNNQmxoMTRRUW1aTG9WQXdnNEp3M0ZLcjh3Y0s5SUlhdDc4WHBJamVCc1dkSXQzUW14VWxTOG1oRjlTK3h3OCtKYmxUWmovUjRIM0RMeWlmVHg5M214eTlpK3JOVGZyWVhieXlRaS9JelJhY25wNmJRYm00Y01MMjBxeGhoMTZRbXpQRENCb2tOTVlVaVFTcVpSaGgrNmloRjRUamhMdTQzTjdaNlNzRGdNOGdhV3BCalNpRStsQVFVZHphNm5PVEcyOVdSaEFhWTBQM0lzbktzV0UxL2xZb0pZY2dCR0ZQZHpjb2FqM3ZQM3pzS3FyRGxJajM0ak9XNWVXbFlLcXJRVlZVT3hYc2tKOUQ3d0Y2RU94Q0Z1S2l5blVjVkp6OUVmbEJ0QXAySTZSY2JwVzhiM3NzZVdOYWkwZE9hQ1h2WUp0R0lNa3RROXdGZ2d3Y3g5UTBjczhrWVhPOHJrSG9qQXlCSFZVUWdRVXUxbEFZTVNGa0RQNUcwSUwrQjVRc0pVZUN5SkVnY21nYUY5MCs2MkZrcmZzQjZ5Z0VGZk56YzRZRmlpQ2hVM09oVDlDZWhkWXVockNjWW9wamtnUDZ5ZmVOQ3U5VDNPblpXWnNjMitCKzMyc1h5RUJxYU8rZzJWNk0zNFFwMTNkYXlQc0ljbnZUMERPTkRlSmpZWW52WEZ4Y2FCUEMwSmJsWFpDYlpVYU54amZaQXFGdnFNSnNocFFNVzFwSTZ5Qnl2QXV5bFZMQWNFcUkreHNZUnBOM1FXNDdMOE9jNy80R2hyWXM3NEtHM1ZZMUtHN0ZsaUZvOFM0b1dmTzBwam5zdlQ1UENXbWVlZGRjazlsNmttLzhINE5hNnc4TFR2VDFJU25iNzREVFZKVHFhWUhGb1R2ZlExSzJPV1JVSktkRDd1eTJmUjhhVVZnYVMyZ3FxcDBTcGtoYVlrT05JcnRzcytjNGU5eTk1Z0w2RjE2OWZFR3pIcUlxZWR1VGU5M1RIZ0ZFNFZnMWpNdTYySEpHVmd6QWR5RGR4QVJsVHdLU2xvZEhSeDB6M0pDRkhyaGljU3E1eDU2T2EvTmt4V0ZFUWdET29rdjY1S0xyV0V6VThRb2wzVjV6d1FlMFRTUDI3TzVzdG51WVFBeUNBZWJPbnlDNmVqQ2kwRm9WUlZIUERZdFpFTkpEQ3FaTHhoR1RKYmkyS3h4RDBBT0h5NDhoRTQ3akZxUmwwMFIyeWtOMG1FeUxjY2JDdlo1UEtLZ3ZqaHhsczhtUklISWtpQndKSWtlQ3lKRWdjaVNJSEFraVI0TElrU0J5SklnY0NTSkhnc2lSSUhJa2lCd0pJa2VDeUpFZ2NpU0lIQWtpUjRMSWtTQnlKSWdjQ1NKSGdzaVJJSEtDL2xmUlhSSjBhL012eEtnMlZVWnpKUm9BQUFBQVNVVk9SSzVDWUlJPSIvPgo8L2RlZnM+Cjwvc3ZnPgo=";

        /// <summary>
        /// Email template not found message use while throwing an exception in Watchdog reminder email.
        /// </summary>
        public const string EmailTemplateNotFound = "Email template not found.";

        /// <summary>
        /// The Pricing Concessions Template
        /// </summary>
        public const string PRICINGCONCESSIONSTEMPLATE = "PricingConcessions.html";

        /// <summary>
        /// The Non-Pricing Concessions Template
        /// </summary>
        public const string NONPRICINGCONCESSIONSTEMPLATE = "NonPricingConcessions.html";

        /// <summary>
        /// The Sku item Template
        /// </summary>
        public const string SKUITEMTEMPLATE = "SkuItem.html";

        /// <summary>
        /// Correlation Header for the Teams notification
        /// </summary>
        public const string TeamsNotificationCorrelationHeader = "x-ms-client-request-id";

        #endregion OutlookConstants

        #region Outlook Quick Action Constants

        public const string CardCorrelationId = "Card-Correlation-Id";
        public const string ActionRequestId = "Action-Request-Id";
        public const string CardActionStatus = "CARD-ACTION-STATUS";
        public const string CardUpdateInBody = "CARD-UPDATE-IN-BODY";
        public const string InValidSenderClaim = "Alias - Sender Claim is InValid";
        public const string UserActionsStringIsNull = "UserActionsString is null - No user action(s) submitted";
        public const string ActionName = "ActionName";
        public const string Color = "Color";
        public const string MessageTitle = "MessageTitle";
        public const string ActionBody = "ActionBody";
        public const string JustificationKey = "Justification";
        public const string ActionBodyLineItemId = "LineItemID";
        public const string StatusGood = "Good";
        public const string UserImage = "UserImage";

        public const string OutlookSuccessMessage = "Your action was successfully submitted.";
        public const string OutlookGenericErrorMessage = "Failed to take action. Please try again.";
        public const string OutlookAutoRefreshGenericMessage = "Now review and approve right from within Outlook";
        public const string DocumentStatusAPIResponseIsNull = "DocumentStatus API response is null";
        public const string DocumentStatusAPIFailedWithStatusCode = "DocumentStatus API failed with status code:- {0}";

        public const string RegexSquareBracket = @"^\[.*?\]$";
        public const string RegexRemoveExtraSpace = "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+";

        public const string OldRequest = "OldRequest";
        public const string Pending = "Pending";
        public const string LobPending = "LobPending";
        public const string SubmittedForBackgroundApproval = "SubmittedForBackgroundApproval";
        public const string OutOfSyncRecord = "OutOfSyncRecord";

        public const string OldRequestMessage = "The request information has been updated in the source system. Please refer to the latest email to view and take an action.";
        public const string LobPendingMessage = "You have already taken an action for this request";
        public const string SubmittedForBackgroundMessage = "The request is currently submitted for background approval from Web client. Action cannot be taken on this request";
        public const string OutOfSyncMessage = "The request is out of sync from the tenant system. Action cannot be taken on this request";
        public const string ActionTakenMessage = "You have already taken action on this request. This request is no longer pending with you.";
        public const string AttachmentDownloadGenericFailedMessage = "Exception occurred while downloading attachment of documents.";

        /// <summary>
        /// The outlook auto refresh action response format for failure message
        /// </summary>
        public const string OutlookAutoRefreshActionResponseMessage = "{\"$schema\":\"https://adaptivecards.io/schemas/adaptive-card.json\",\"type\":\"AdaptiveCard\",\"correlationId\":\"#ApprovalIdentifier.DisplayDocumentNumber#\",\"hideOriginalBody\":true,\"version\":\"1.0\",\"fallbackText\":\"This device does not support yet, please upgrade your outlook version\",\"body\":[{\"columns\":[{\"width\":\"1\",\"items\":[{\"text\":\"#MessageTitle#\",\"wrap\":true,\"isVisible\":true,\"size\":\"Small\",\"type\":\"TextBlock\"}],\"verticalContentAlignment\":\"center\",\"padding\":\"small\",\"type\":\"Column\"}],\"spacing\":\"small\",\"type\":\"ColumnSet\"}]}";

        #endregion Outlook Quick Action Constants

        #region EditableFieldauditTrail

        public const string EditableFieldAuditLogs = "EditableFieldAuditLogs";
        public const string Fields = "Fields";
        public const string Id = "ID";

        #endregion EditableFieldauditTrail

        public const string HTTPMethodPatch = "PATCH";

        public const string DelegateOperationType = "DELEGATE";

        public const string TenantActionDetails = "ACTDTL";

        public const string SummaryTemplate = "SummaryTemplate";
        public const string DetailsTemplate = "DetailsTemplate";
    }
}