// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.Model
{
    public enum LogDataKey
    {
        None = 0,
        AADClientID,
        DocDbName,
        DocDbCollectionName,

        SubscriptionName,

        ValidationResults,

        BrokerMessage,
        UserActionsString,

        ApprovalCountResults,

        TenantId,
        TenantName,
        Tenants,
        DocumentNumbers,
        DocumentNumber,
        DisplayDocumentNumber,
        PartitionKey,
        FiscalYear,
        RowKey,

        UserAlias,
        UserEmail,
        Approver,
        Approvers,
        OperationType,
        DeleteForAliases,
        OperationDateTime,

        DocumentTypeId,

        EntryUtcDateTime,
        LoggingDateTimeUtc,

        SortColumn,
        SortDirection,

        Months,

        SearchText,
        PageNum,

        ResponseStatusCode,
        ResponseContent,
        Operation,
        TableOperation,
        LocalTime,
        _TrackingEvent,
        _CorrelationId,
        _ActivityId,
        ErrorMessage,

        //WatchDog
        MaxFailureCount,

        BatchSize,
        ApprovalBaseUrl,

        MessageId,

        //email for health monitoring
        Subject,

        // Used to log retry count to fetch summary
        Counter,

        UriType,
        BrokerMessageProperty,
        Uri,
        BaseAddress,
        MachineName,
        FailureData,
        PayloadId,
        PayloadValidationResult,

        // NotificationWorker
        NotificationTemplateKey,

        DetailStatus,

        // Payload Race Condition
        RaceConditionSleepTime,

        // Standardized Log Properties
        SessionId,

        Tcv,
        DXcv,
        ClientDevice,
        ComponentName,
        IsCriticalEvent,

        BusinessProcessName,
        EventId,
        EventType,
        EventName,

        // Telemetry Specific Keys
        UserRoleName,

        ActionOrComponentUri,
        AppAction,
        StartDateTime,
        EndDateTime,
        Duration,
        Xcv,
        ReceivedTcv,
        TenantTelemetryData,
        Attachments,
        IsEmailSentWithDetail,
        SummaryCount,

        //Support Portal Logging
        FeatureId,

        ManagerAlias,
        DelegatedToAlias,
        DelegationAccessType,
        DelegationIsHidden,

        EnvironmentName,
        ServiceOffering,
        ServiceLine,
        Service,

        ComponentId,
        GlobalTcv,

        TotalWatchdogNotificationsToBeSent,
        TotalActionableEmailsSent,
        TotalNormalEmailsSent,
        WatchdogFailureCount,

        TemplateType,
        AADTokenType,
        BlobId,
        TeamsNotificationJson,
        TeamsNotificationCorrelationId
    }

    public enum TrackingEvent
    {
        #region worker

        #region Loadbalance

        //Load Balance events. Success Marked with a 2xx value. warnings use 3xx. errors use 700
        WebsiteInitializeFail = 200,

        InvalidApprover = 724,

        #endregion Loadbalance

        AuditAgentDocDbOperationFailed = 8000,

        #region Startup

        /// <summary>
        /// This error occurred when worker role failed to configure service bus topic, queue etc. If any component have any issue which cause the failure of starting Worker role.
        /// <para></para>
        /// <para>Impact: Worker role is recycling or stopped.</para>
        /// <para>Recommended Action: Restart the Worker role instance.</para>
        /// </summary>
        WorkerStartup = 301,

        FetchTenantInfo = 100010,

        #endregion Startup

        #region Message Processing

        NotificationProcessingFail = 314,

        /// <summary>
        /// This message logged in two scenarios:
        /// <para>Warning: When the data is already exists and worker role tries to add the same data again into Summary table.</para>
        /// <para>Exception: When worker role failed to create record into summary table</para>
        /// <para></para>
        /// <para>Impact: Worker role is not able to create summary record. Due to that user is not able to see that pending request on summary page.</para>
        /// <para>Recommended Action: Check if the message successfully completed in retry. If it fails then reprocess that message again.</para>
        /// </summary>
        TopicMessageProcessCreate = 315,

        /// <summary>
        /// This message is getting logged in two scenarios
        /// <para>Warning: Before retrying the message, worker role logs warning with approval request notification.</para>
        /// <para>Error: When any message failed to process. (Except: MSInovice data not found exception)</para>
        /// <para></para>
        /// <para>Impact: Message is not getting completed scucessfully. Due to that it will go to retry.</para>
        /// <para>Recommended Action: Check in retry topic. if it is in deadletter then process that message again.</para>
        /// </summary>
        ARXProcessingFail = 316,

        /// <summary>
        /// This message logged in two scenario:
        /// <para>Warning: When the data is not exists and worker role tries to delete the data from Summary table.</para>
        /// <para>Error: When worker role failed to remove record from summary table</para>
        /// <para></para>
        /// <para>Impact: Summary record is not getting removed from the table. Due to that it is still available in summary table (still it is available in summary table, it is not visible to the end user as "LobPending" flag is getting updated).</para>
        /// <para>Recommended Action: Check in retry topic deadletter. If message exists then process it once again.</para>
        /// </summary>
        TopicMessageProcessDelete = 317,

        /// <summary>
        /// This information is getting logged when the message moved to retry topic.
        /// <para></para>
        /// <para>Impact: Summary record is not getting removed from the table. Due to that it is still available in summary table (still it is available in summary table, it is not visible to the end user as "LobPending" flag is getting updated).</para>
        /// <para>Recommended Action: Check in retry topic deadletter. If message exists then process it once again.</para>
        /// </summary>
        MoveMessageToRetry = 318,

        /// <summary>
        /// This message would be logged when the update of the summary data fails post an action and it can be ignored since the record might have been removed already.
        /// </summary>
        UpdateSummaryFail = 319,

        /// <summary>
        /// This message would be logged when the update of the summary data succeeded post an action.
        /// </summary>
        UpdateSummarySuccess = 320,

        MoveMessageToSecondaryOperationTopicFail = 330,
        ApprovalDetailRemoveFail = 331,
        MoveMessageToNotificationTopicFail = 333,
        ApprovalSummaryRemovalSkipped = 334,
        UpdateSummaryForOfflineApproval = 340,
        SummaryNotFoundInTenantSystem = 341,
        UpdateSummaryIsOutOfSyncChallenged = 344,
        ApprovalDetailRemovalSkipped = 345,

        /// <summary>
        /// This message would be logged when Approvals failed to complete message processing and now message is moved to dead-letter of the main topic
        /// Tenant Id is added to this, to indicate for which tenant message processing failed
        /// e.g for Lob App having TenantID=1 the value will be 50001
        /// </summary>
        MoveMessageToDeadletterFromMainTopic = 50000,

        /// <summary>
        /// This message would be logged when Approvals failed to complete message processing and now message is moved to dead-letter of the retry topic
        /// Tenant Id is added to this, to indicate for which tenant message processing failed
        /// e.g for Lob App having TenantID=1 the value will be 51001
        /// </summary>
        MoveMessageToDeadletterFromRetryTopic = 51000,

        NewMessageRecievedInRetryTopic = 56000,
        NewMessageRecievedInNotificationTopic = 58000,

        MessageCompleteSuccessFromRetryTopic = 59000,

        #endregion Message Processing

        #region Message Processing Checkpoints

        EmailSent = 1007,
        NotificationValid = 1008,
        DeviceNotificationSent = 1009,
        DeleteProcessFailed = 1021,
        SummaryInsertedOrReplaced = 1028,
        DeleteProcessForRaceCondition = 1029,
        NotificationFrameworkSendEmailCompleted = 1030,

        #endregion Message Processing Checkpoints

        #region Notification

        /// <summary>
        /// This error logged when Worker role tries to process the notification message (end user notifications like Windows Phone, Windows 8 and Email).
        /// <para></para>
        /// <para>Impact: User will not get the notifications</para>
        /// <para>Recommended Action: Reprocess the notificaiton message from Approvals queue.</para>
        /// </summary>
        ProcessNotificationFail = 401,

        /// <summary>
        /// This exception is getting logged when worker role is failed to validate the notification message.
        /// <para></para>
        /// <para>Impact: User will not get the notifications</para>
        /// <para>Recommended Action: Contact the Approvals team to check the notification message content.</para>
        /// </summary>
        ProcessNotificationValidationFail = 402,

        ProcessNotificationComplete = 403,

        /// <summary>
        /// When worker role failed to send an email notification to Notification framework.
        /// <para></para>
        /// <para>Impact: Email will not route to the end user</para>
        /// <para>Recommended Action: Check Approvals and Notification framework heath. If required then restart Notification framework azure resource instances.</para>
        /// </summary>
        SendEmailNotificationFail = 412,

        /// <summary>
        /// When worker role failed to send an actionable email notification.
        /// </summary>
        SendEmailNotificationWithApprovalFunctionalityFail = 413,

        SendDeviceNotificationFail = 415,

        /// <summary>
        /// When worker role failed to fetch attachments foractionable email notification.
        /// </summary>
        SendEmailNotificationFailedToLoadAttachments = 416,

        /// <summary>
        /// This error occurred when the WatchDogReminder process encounters an unexpected error.
        /// <para></para>
        /// <para>Impact: Some or all watchdog reminders did not get sent</para>
        /// <para>Recommended Action: Check the paf email system for errors. Verify that azure table storage is available. Will retry automatically after watchdog send period.</para>
        /// </summary>
        WatchDogReminderFailed = 450,

        /// <summary>
        /// The watchdog reminders were sent without error.
        /// <para></para>
        /// <para>Impact: normal</para>
        /// <para>Recommended Action: none</para>
        /// </summary>
        WatchDogRemindersSent = 451,

        /// <summary>
        /// The built in failure counter for the watchdog notification process has been reached causing the process to exit.
        /// <para></para>
        /// <para>Impact: Some or all watchdog reminders did not get sent</para>
        /// <para>Recommended Action: Check the paf email system for errors. Verify that azure table storage is available. Will retry automatically after watchdog send period.</para>
        /// </summary>
        WatchDogReminderFailureCapReached = 453,

        /// <summary>
        /// Reminder time update failure exception. Attempting to refresh and retry update
        /// <para></para>
        /// <para>Impact: Extra calls were made to table storage</para>
        /// <para>Recommended Action: None</para>
        /// </summary>
        WatchDogReminderUpdateException = 454,

        /// <summary>
        /// An exception occurred while processing a reminder notification. Will continue processing
        /// <para></para>
        /// <para>Impact: The listed reminder may not have been sent or may send again in the future </para>
        /// <para>Recommended Action: If many of these exceptions occur check the paf notification system, table storage, and the notificationdetails.reminder data for correctness. </para>
        /// </summary>
        WatchDogReminderSingleReminderFailed = 455,

        /// <summary>
        /// An exception occurred while processing a actionable reminder notification. Will continue processing with normal email
        /// </summary>
        WatchDogReminderEmailNotificationWithApprovalFunctionalityFail = 1201,

        /// <summary>
        /// An exception occurred while processing a actionable reminder notification along with attachments. Will continue processing with normal email
        /// </summary>
        WatchDogReminderEmailNotificationFailedToLoadAttachments = 1202,

        /// <summary>
        /// This error occurred when the WatchDogReminder process encounters an unexpected error that reaches the main worker role.
        /// <para></para>
        /// <para>Impact: Some or all watchdog reminders did not get sent. Some other major issue is likely at play.</para>
        /// <para>Recommended Action: Check the paf email system for errors. Verify that azure table storage is available. Will retry automatically after watchdog send period. Contact engineering if this happens more than once.</para>
        /// </summary>
        WorkerRoleWatchDogError = 456,

        WatchDogSendRemindersCallInitiated = 460,

        WatchDogReminderNotificationDetailFetched = 461,

        WatchDogReminderRetrieveSummaryRowsSuccess = 462,

        WatchDogReminderRetrieveSummaryRowsFail = 463,

        WatchDogUpdateReminderInfoComplete = 464,

        #endregion Notification

        #region Email Notification

        ValidateIfEamilSentWithDetailsFail = 98,

        NormalEmailNotificationSent = 9001,
        ActionableEmailNotificationSent = 9002,
        SendEmailNotification_UserDelegation = 9003,
        MobileFriendlyTemplateGenerationSuccessful = 9004,
        AdaptiveCardTemplatingSuccessful = 9006,

        ActionableEmailNotificationFail = 9101,
        NormalEmailNotificationFail = 9102,
        SendEmailNotificationFail_UserDelegation = 9103,
        MobileFriendlyTemplateGenerationFailure = 9104,

        #endregion Email Notification

        #endregion worker

        #region web api

        /// <summary>
        /// This error occurred when WebAPI instance fail to get summary information for summary page or bulk view page.
        /// <para></para>
        /// <para>Impact: End user will get an exception on client side and not able to see summary information</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiSummaryFail = 501,

        /// <summary>
        /// This error occurred when WebAPI instance fail to get summary count information for summary page or bulk review page.
        /// <para></para>
        /// <para>Impact: End user will not get the total summary count</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiSummaryCountFail = 502,

        /// <summary>
        /// This event Id is exclusively for the exposed webapi for employee experience change.
        /// <para></para>
        /// <para>Impact: User will access the url and provide Id (which is documentTypeId).</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiSummaryByIdFail = 503,

        /// <summary>
        /// This error occurred when WebAPI instance fail to get summary count information for the specified tenant.
        /// <para></para>
        /// <para>Impact: End user will not get the summary count for the specified tenant</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiTenantWiseSummaryCountFail = 504,

        /// <summary>
        /// The web API create summary data list fail
        /// </summary>
        WebApiCreateSummaryDataListFail = 506,

        /// <summary>
        /// This error occurred when WebAPI instance fail to get summary information from tenant system.
        /// <para></para>
        /// <para>Impact: End user will get an exception on client side and not able to see summary information</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiExternalSummaryFail = 507,

        /// <summary>
        /// This error occurred when WebAPI instance fail to get details information from tenant system.
        /// <para></para>
        /// <para>Impact: End user will get an exception on client side and not able to see details information</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiExternalDetailsFail = 508,

        /// <summary>
        /// This error occurred when WebAPI instance fail to get action details information from tenant system.
        /// <para>Impact: End user will get an exception on client side and not able to see action details information</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiExternalActionDetailsFail = 509,

        /// <summary>
        /// This error occurred when WebAPI instance fail to load the detail information for particular record. It might be Approvals details information issue or tenant services issue.
        /// <para></para>
        /// <para>Impact: User is not able to see the detail information.</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiDetailFail = 511,

        /// <summary>
        /// This error occurred when user tries to take an action on pending approval request and failed to complete that process.
        /// <para></para>
        /// <para>Impact: User is not able to approve / reject the request</para>
        /// <para>Recommended Action: Check Tenant service is up and running. If tenant service is running then Contact Approvals team with detail</para>
        /// </summary>
        WebApiDetailDocumentDownloadFail = 512,

        /// <summary>
        /// This error occurred when Web API instance tries to get information / execute action from tenant API.
        /// <para></para>
        /// <para>Impact: User is not able to see / taken an action</para>
        /// <para>Recommended Action: Check the tenant service is up and running.</para>
        /// </summary>
        TenantApiFail = 513,

        WebApiReadDetailsFail = 515,
        WebApiUserImageFail = 517,
        WebApiBulkDocumentDownloadFail = 520,

        /// <summary>
        /// This error occurred when WebAPI failed to load the history information. When mobile service is down then WebAPI log this error code.
        /// <para></para>
        /// <para>Impact: User is not able to see history information / approval chain information on detail page.</para>
        /// <para>Recommended Action: Check mobile service is up and running. Check TransactionHistory database is accessible. If still problem persists then contact Approvals team.</para>
        /// </summary>
        WebApiHistoryDataLoadFail = 521,

        /// <summary>
        /// This error occurred when user tries to download the history data and Web API fails to generate the data. When mobile service is down then WebAPI log this error code.
        /// <para></para>
        /// <para>Impact: User is not able to export history data.</para>
        /// <para>Recommended Action: Check mobile service is up and running. Check TransactionHistory database is accessible. If still problem persists then contact Approvals team.</para>
        /// </summary>
        WebApiHistoryDownloadFail = 523,

        /// <summary>
        /// This error occurred when WebAPI failed to get history data count. When mobile service is down then WebAPI log this error code.
        /// <para></para>
        /// <para>Impact: User is not able to see history count on history page.</para>
        /// <para>Recommended Action: Check mobile service is up and running. Check TransactionHistory database is accessible. If still problem persists then contact Approvals team.</para>
        /// </summary>
        WebApiHistoryDataCountFail = 524,

        /// <summary>
        /// This message logged failure scenario for Document Approval Status api
        /// </summary>
        WebApiDocumentStatusFail = 525,

        /// <summary>
        /// This error occurred when WebAPI instance fail to get summary count information from tenant system.
        /// <para></para>
        /// <para>Impact: End user will get an exception on client side and not able to see summary count information</para>
        /// <para>Recommended Action: Contact Approvals team with detail</para>
        /// </summary>
        WebApiExternalSummaryCountFail = 526,

        /// <summary>
        /// This message logged in two scenario:
        /// <para>Warning: When Approvals successfully execute the action.</para>
        /// <para>Error: When Approvals fails to execute the action.</para>
        /// <para></para>
        /// <para>Impact: If error then action (approve, reject etc...) is not completed successfully.</para>
        /// <para>Recommended Action: Check the detailed exception and contact to Tenant team if required.</para>
        /// </summary>
        WebApiDocumentActionFail = 531,

        SummaryRowUpdateFailed = 532,

        ProcessUserActionsFailed = 533,

        ProcessUserActionsSuccess = 534,

        /// <summary>
        /// This error occurred when not able to reach azure table storage for retrieving tenantdowntimemessages.
        /// <para></para>
        /// <para>Impact: User will not be albe to see any downtime messages.</para>
        /// <para>Recommended Action: Check if azure table storage is accessible & tenantdowntimemessages has messages.</para>
        /// </summary>
        WebApiGetAllDownTimeMessagesFail = 545,

        WebApiImpersonationReadFail = 551,
        WebApiImpersonationSettingsCreateFail = 552,
        WebApiImpersonationSettingsReadFail = 553,
        WebApiImpersonationSettingsUpdateFail = 554,
        WebApiImpersonationSettingsDeleteFail = 555,
        WebApiImpersonationSettingsTenantInfoFail = 556,
        WebApiAboutFail = 580,
        WebApiHelpFail = 590,
        WebApiDetailAllDocumentDownloadFail = 600,
        WebApiDetailPreviewDocumentFail = 601,
        WebApiAdaptiveDetailFail = 604,
        WebApiSaveEditableDetailsFail = 606,
        WebApiUserPreferenceFail = 607,
        WebApiSummaryDataMappingFail = 608,

        //Outlook AAD related Tracking Events (DocumentAction and AutoRefresh)
        WebApiOutlookAADActionFail = 7005,

        WebApiOutlookAADAutoRefreshFail = 7006,

        WebApiOutlookAADActionSuccess = WebApiOutlookAADActionFail * 1000,
        WebApiOutlookAADAutoRefreshSuccess = WebApiOutlookAADAutoRefreshFail * 1000,

        #region Web API Positive events

        //EventId = respective events x 1000

        WebApiSummarySuccess = WebApiSummaryFail * 1000,
        WebApiExternalSummarySuccess = WebApiExternalSummaryFail * 1000,
        WebApiDetailSuccess = WebApiDetailFail * 1000,
        WebApiExternalDetailsSuccess = WebApiExternalDetailsFail * 1000,
        WebApiReadDetailsSuccess = WebApiReadDetailsFail * 1000,
        WebApiAboutSuccess = WebApiAboutFail * 1000,
        WebApiHelpSuccess = WebApiHelpFail * 1000,
        WebApiDocumentActionSuccess = WebApiDocumentActionFail * 1000,
        WebApiSummaryCountSuccess = WebApiSummaryCountFail * 1000,
        WebApiExternalSummaryCountSuccess = WebApiExternalSummaryCountFail * 1000,
        WebApiTenantWiseSummaryCountSuccess = WebApiTenantWiseSummaryCountFail * 1000,
        WebApiHistoryDataCountSuccess = WebApiHistoryDataCountFail * 1000,
        WebApiHistoryDataLoadSuccess = WebApiHistoryDataLoadFail * 1000,
        WebApiHistoryDownloadSuccess = WebApiHistoryDownloadFail * 1000,
        WebApiGetAllDownTimeMessagesSuccess = WebApiGetAllDownTimeMessagesFail * 1000,
        WebApiDetailDocumentDownloadSuccess = WebApiDetailDocumentDownloadFail * 1000,
        WebApiDetailPreviewDocumentSuccess = WebApiDetailPreviewDocumentFail * 1000,
        WebApiBulkDocumentDownloadSuccess = WebApiBulkDocumentDownloadFail * 1000,
        WebApiImpersonationReadSuccess = WebApiImpersonationReadFail * 1000,
        WebApiImpersonationSettingsCreateSuccess = WebApiImpersonationSettingsCreateFail * 1000,
        WebApiImpersonationSettingsReadSuccess = WebApiImpersonationSettingsReadFail * 1000,
        WebApiImpersonationSettingsUpdateSuccess = WebApiImpersonationSettingsUpdateFail * 1000,
        WebApiImpersonationSettingsDeleteSuccess = WebApiImpersonationSettingsDeleteFail * 1000,
        WebApiImpersonationSettingsTenantInfoSuccess = WebApiImpersonationSettingsTenantInfoFail * 1000,
        WebApiExternalSummaryWithNotFoundStatus = WebApiExternalSummarySuccess * 100,
        WebApiExternalDetailWithNotFoundStatus = WebApiDetailSuccess * 100,
        WebApiExternalActionDetailsSuccess = WebApiExternalActionDetailsFail * 1000,

        /// <summary>
        /// This message logged success scenario for Document Approval Status api
        /// </summary>
        WebApiDocumentStatusSuccess = WebApiDocumentStatusFail * 1000,

        WebApiDetailAllDocumentDownloadSuccess = WebApiDetailAllDocumentDownloadFail * 1000,
        WebApiAdaptiveDetailSuccess = WebApiAdaptiveDetailFail * 1000,
        WebApiSaveEditableDetailsSuccess = WebApiSaveEditableDetailsFail * 1000,
        WebApiUserPreferenceSuccess = WebApiUserPreferenceFail * 1000,

        #endregion Web API Positive events

        #region BL Tracking Events

        //800 series, in sync with Web API events
        DetailFetchFailure = 811,

        DetailFetchInitiated = 814,
        DetailFetchSuccess = 813,

        DocumentActionFailure = 830,
        DocumentActionSuccess = 831,
        DocumentDownloadFailure = 833,

        AdaptiveTemplateFetchSuccess = 834,
        AdaptiveTemplateFetchFailure = 835,

        #endregion BL Tracking Events

        #region DAL Tracking Events

        //900 series, in sync with Web API events
        HistoryDataLoadFail = 921,

        HistoryDataCountFail = 924,
        AzureStorageAddRequestDetailsSuccess = 619,

        #endregion DAL Tracking Events

        #endregion web api

        #region common

        /// <summary>
        /// This error occurred when Approvals application tries to resolve the alias.
        /// <para></para>
        /// <para>Impact: Alias is not getting resolved.</para>
        /// <para>Recommended Action: Check the error and contact Approvals Eng team if needed.</para>
        /// </summary>
        NameResolutionError = 601,

        /// <summary>
        /// This event is logged when alias is not microsoft alias.
        /// <para></para>
        /// <para></para>
        /// <para></para>
        /// </summary>
        WebApiNameResolutionFailure = 602,

        /// <summary>
        /// This error occurred when Approvals tried to get details of the request from Azure Table Storage.
        /// <para></para>
        /// <para>Impact: Unknown.</para>
        /// <para>Recommended Action: Check if Azure storage is healthy and having the details of the particular request. Reach out to Engineering team for further investigation.</para>
        /// </summary>
        AzureStorageGetRequestDetailsFail = 611,

        /// <summary>
        /// This error occurred when Approvals tried to add details of the request to Azure Table Storage.
        /// <para></para>
        /// <para>Impact: Unknown.</para>
        /// <para>Recommended Action: Check if Azure storage is healthy. Reach out to Engineering team for further investigation.</para>
        /// </summary>
        AzureStorageAddRequestDetailsFail = 612,

        /// <summary>
        /// This error occurred when Approvals tried to remove details of the request from Azure Table Storage.
        /// <para></para>
        /// <para>Impact: Unknown.</para>
        /// <para>Recommended Action: Check if Azure storage is healthy and having the details of the particular request. Reach out to Engineering team for further investigation.</para>
        /// </summary>
        AzureStorageRemoveRequestDetailsFail = 613,

        #endregion common

        /// <summary>
        /// This error occurred while securty checks..
        /// <para></para>
        /// <para>Impact: Unknown.</para>
        /// <para>Recommended Action: Check if SQL Azure is healthy. Reach out to Engineering team for further investigation.</para>
        /// </summary>

        #region Security

        AcsSimpleWebToken = 807,
        AADTokenGenerationError = 808,
        SASTokenGenerationError = 809,
        AADTokenGenerationSuccessful = 810,

        #endregion Security

        #region LOGGING DB EVENTS

        ARXReceivedSuccessfullyByPayloadReceiver = 6772,
        ARXSentSuccessfullyToServiceBus = 6773,
        ARXValidationSuccess = 6774,
        ARXValidationFailed = 6775,
        ARXReceivedByMainWorker = 6776, // Pending
        ARXFailedToProcessInMainTopic = 6777, // Failed
        ARXProcessedSuccessfullyInMainTopic = 6778, // Complete
        ARXProcessingStartedByAuditAgent = 6780,
        ARXLoggedByAuditAgent = 6781,
        ARXProcessingCompleteByAuditAgent = 6782,

        #endregion LOGGING DB EVENTS

        ApprovalActionFailedOnTenantSide = 6786,
        UserTriggeredDetailInitiated = 6787,
        UserTriggeredDetailSuccessful = 6788,
        UserTriggeredDetailFail = 6789,

        ARXReceivedSuccessfullyByServiceBusInRetryTopic = 6790, // Pending
        ARXProcessedSuccessfullyInRetryTopic = 6791, // Complete
        ARXFailedToProcessInRetryTopic = 6792, // Failed

        ARXDeleteProcessStarts = 6793,  // Pending
        ARXDeleteProcessCompletes = 6794,  // Pending

        CreateProcessStarts = 6795,  // Pending
        CreateProcessCompleted = 6796,  // Pending

        ARXUpdateProcessStarts = 6797,  // Pending
        ARXUpdateProcessCompletes = 6798,  // Pending

        BrokeredMessageMovedToRetryTopic = 6799,  // Pending
        BrokeredMessageMovedToSecondaryTopic = 6800,  // Pending
        BrokeredMessageMovedToNotificationTopic = 6801,  // Pending

        SummaryDeletedInBackground = 6802,  // Pending
        SummaryInsertedInBackground = 6803,  // Pending
        DetailsDeletedInBackground = 6804,  // Pending
        DetailsInsertedInBackground = 6805,  // Pending
        DetailsDataPrefetchIntitiated = 6806,  // Pending
        DetailsDataPrefetchSuccess = 6807,  // Pending
        DetailsDataPrefetchFailed = 6808,  // Pending
        DetailsDataPreattachedInARX = 6809,  // Pending

        ARXReceivedByNotificationWorker = 6814, // Pending
        ARXFailedByNotificationWorker = 6816, // Failed

        ApprovalActionInitiated = 6817,
        ApprovalActionSuccessfullyCompleted = 6818,
        ApprovalActionFailed = 6819,

        ARXBusinessRulesValidationStarted = 6820,
        ARXBusinessRulesValidationCompleted = 6821,
        ARXServerSideValidationStarted = 6822,
        ARXServerSideValidationCompleted = 6823,

        PullTenantGetSummarySuccess = 6824,
        PullTenantGetSummaryFailed = 6825,
        PullTenantGetDetailsSuccess = 6826,
        PullTenantGetDetailsFailed = 6827,

        ImpersonationGetSuccess = 6828,
        ImpersonationGetFailed = 6829,

        GetTenantActionDetailsSuccess = 6830,
        GetTenantActionDetailsFailed = 6831,

        #region PAYLOAD RECEIVER LOGGING

        PayloadProcessingFailure = 200002,
        PayloadAccepted = 200001,
        PayloadValidationFailure = 200003,

        #endregion PAYLOAD RECEIVER LOGGING

        #region AUDIT AGENT TRACKING EVENTS

        AuditMessageProcessingFailed = 8319,

        #endregion AUDIT AGENT TRACKING EVENTS

        // NOTE: EventId 10000 series (10001-10005) & (10051-10058) is being used in External Logging in DataPoint
        DownloadAndStoreAttachmentsFail = 904,

        DownloadAndStoreAttachmentsSuccessOrSkipped = 905,
        AttachmentDeleteFailed = 907,

        // NOTE: Feature Usage Tracking EventIds (40000-49000)

        /// <summary>
        /// The action audit failure
        /// </summary>
        LogActionAuditFailure = 8401,

        /// <summary>
        /// The action audit success
        /// </summary>
        LogActionAuditSuccess = 8402,

        #region Outlook

        OutlookBlobTemplateByFileName = 20000,

        #endregion Outlook

        #region ActionableEmailMessages

        ActionableEmailAPIDetailsFetch = 4001,
        ActionableEmailGetAttachments = 4002,

        #endregion ActionableEmailMessages

        DelegationInsert = 9902,
        DelegationUpdate = 9903,
        DelegationInsertFailed = 9910,
        DelegationUpdateFailed = 9911,

        /// <summary>
        /// The history insert success event
        /// </summary>
        HistoryInsertSuccess = 9914,

        /// <summary>
        /// The history insert failure event
        /// </summary>
        HistoryInsertFailure = 9915,

        BatchUpdateSummaryInitiated = 9916,
        BatchUpdateSummarySuccess = 9917,
        BatchUpdateSummaryFailed = 9918,

        BatchUpdateDetailsInitiated = 9919,
        BatchUpdateDetailsSuccess = 9920,
        BatchUpdateDetailsFailed = 9921,

        TenantApiComplete = 541002,

        TeamsNotificationAPISuccess = 541003,
        TeamsNotificationAPIFail = 541004
    }
}