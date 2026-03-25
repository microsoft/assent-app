// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts
{
    using System.Runtime.Serialization;

    public enum DetailsComponentType
    {
        AdaptiveCard = 0,
        Microfrontend = 1
    }

    [DataContract(Name = "NotificationType", Namespace = "http://www.microsoft.com/document/routing/2010/11")]
    public enum NotificationType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        All = 1,

        [EnumMember]
        Tile = 2,

        [EnumMember]
        Toast = 3,

        [EnumMember]
        Raw = 4,

        [EnumMember]
        Badge = 5,
    }

    public enum ProcessSecondaryActions : int
    {
        ProcessSecondaryActionsOnMainChannel = 0,
        ProcessSecondaryActionsOnSecondaryChannel = 1,
        ProcessSecondaryActionsNone = 2
    }

    public enum ConfigurationKey
    {
        #region Common

        AcceptsTerms,
        ActionAlreadyTakenFromApprovalsMessage,
        ActionAlreadyTakenMessage,
        ActionAuditLogAzureTableName,
        AIAnalysisSystemMessage,
        AIAnalysisCompletionOptions,
        AIAnalysisModelName,
        AISummarizationAndInsightsSchema,
        AntiCorruptionMessage,
        APIUrlRoot,
        ApprovalEmailNotificationTemplatesAzureTableName,
        ApprovalRequestExpressionClass,
        ApprovalRequestVersion,
        ApprovalsAudienceUrl,
        ApprovalsBaseUrl,
        ApprovalsCoreServicesURL,
        ApprovalSummaryAzureTableName,
        ApprovalTenantInfo,
        SubmissionSummaryAzureTableName,
        ARConverterClass,
        ARConverterExternalClass,
        ArxQueueWaitTime,
        AttachmentSizeLimit, // AttachmentSizeLimit beyond which, it will send normal email without attachment.
        AuditLoggerClass,
        Authority,
        AzureSearchServiceName,
        AzureSearchServiceQueryApiKey,
        AzureSearchTransactionHistoryIndexName,
        AzureOpenAIModelName,
        AzureOpenAIDiscrepanciesModelName,
        AzureOpenAICompletionsOptions,
        AzureOpenAISystemMessage,
        AzureOpenAiDetailsPrompt,
        AzureOpenAiApiEndpoint,
        AzureOpenAiApiKey,
        BulkActionConcurrentCallMessage,
        ComponentName,
        CopilotDefaultPrompts,
        CosmosDbAuthKey,
        CosmosDbCollectionActionAuditLog,
        CosmosDbCollectionAuditAgent,
        CosmosDbEndPoint,
        CosmosDbNameActionAuditLog,
        CosmosDbNameAuditAgent,
        CosmosDbPartitionKeyPathAuditAgent,
        CosmosDbPartitionKeyPath,
        DaysForReminderMails, // No of days for which we need to send reminder mail
        DeepSearchModelName,
        DeepSearchScope,
        DeepSearchOperatorsAndLogic,
        DeepSearchIntentInterpretation,
        DeepSearchDataRules,
        DeepSearchExamples,
        DetailControllerExceptionMessage,
        DeviceDeepLinkUrl,
        DiscrepanciesCompletionOptions,
        DiscrepancyPrompt,
        DiscrepancyRiskScoreExplanationPrompt,
        DiscrepancySystemMessage,
        DocDBNameAuditAgent,
        DocDBCollectionAuditAgent,
        DomainName,
        EditableConfigurationTableName,
        EnableValidation,
        EnvironmentName,
        GraphAPIAuthString,
        GraphAPIClientId,
        GraphAPIClientSecret,
        GraphAPIResourceUri,
        GraphAPIMITokenEnabled,
        GraphAPIUri,
        GraphAPIUriFilter,
        HistoryPageSize,
        InvalidRequestException,
        IsAzureSearchEnabled,
        MainTopicFailCountThreshold,
        ManagedIdentityClientId,
        ManagedIdentityFederatedAudience,
        Message_NoTenantForDevice,
        Message_PageLessThan1,
        Message_ServiceNotRelayed,
        Message_SummaryDataNotAvailable,
        Message_UnAuthorizedUser,
        Message_URLNotDefined,
        Message_ValueEmptyGUID,
        Message_ValueNotExist,
        Message_ValueNullOrEmpty,
        MonthsOfHistoryDataValue,
        NoConsentPrompt,
        NotificationBCCEmailAddress,
        NotificationBroadcastUri, // Notification Framework related configuration keys.
        NotificationBroadcastUriMEO,
        NotificationFrameworkAuthKey,
        NotificationFrameworkClientId,
        NotificationFrameworkMaxRetries, // MaxRetries to Notification Framework.
        NotificationFrameworkResourceUrl, // ResourceUrl to make call to Notification Framework.
        NotificationTopicDelaySeconds,
        OCRAttachmentsContainer,
        ReassignmentContainer,
        OutOfSyncExplanation,
        QueueNameSecondary,
        QueueNameReassignment,
        QuickTourFeatures,
        ReceiptAcknowledgmentMessage,
        ReconciliationAwaitTime,
        RelayServiceDownMessage,
        RetryPolicyRetryCount,
        SaveChangesOptionsContinueOnError,
        SearchFiltersSchema,
        ServiceComponentId,
        ServiceLineName,
        ServiceName,
        ServiceOfferingName,
        ServiceParameterAuthKey,
        ServiceParameterClientID,
        SourcePrimary,
        SourceOCR,
        StorageAccountKey,
        StorageAccountName,
        SubscriptionNameNotification,
        SummaryPrioritizationCriteria,
        SupportEmailId,
        SyntheticTransactionsApproverAliasList,
        SyntheticTransactionsLoadBatchDelay,
        TeamsNotificationMITokenEnabled,

        /// <summary>
        /// Teams AppKey which will be used to generate token
        /// </summary>
        TeamsAppKey,

        /// <summary>
        /// Teams ClientId which will be used to generate token
        /// </summary>
        TeamsClientId,

        /// <summary>
        /// Teams endpoint url which will be used to generate token
        /// </summary>
        TeamsEndpointUrl,

        /// <summary>
        /// Teams resource url which will be used to generate token
        /// </summary>
        TeamsResourceUrl,

        /// <summary>
        /// Teams Tenant to generate OAuth 2.0 Token
        /// </summary>
        TeamsTenant,

        TenantAPICallTimeoutValueInMins, // TenantAPICallTimeoutValueInMins
        TenantDownTimeMessagesAzureTableName,
        TopicNameMain,
        TopicNameNotification,
        TopicNameRetry,
        TopicNameSecondary,
        TopicNameAuxiliary,
        TopicNameExternalMain,
        UnAuthorizedException,
        UrlPlaceholderTenants, // TenantIDs for which summary and detail URL contains placeholders with actual property names.
        UserMessageForComplianceAndAction,
        UserPreferenceAzureTableName,
        UserDelegationSettingsAzureTableName,
        UserDelegationSettingsHistoryAzureTableName,
        ValidateAliasUsingPayloadValidator,
        PayloadProcessingFunctionURL,
        WatchDogBatchSize,
        WatchDogMaxFailureCount,
        DelegationPlatformApi,
        DelegationPlatformResourceUrl,
        DelegationPlatformAppId,
        MSAInternalClientId,
        PayloadReceiverResourceUrl,
        PayloadReceiverUrl,

        #endregion Common

        #region Domain

        Message_ConfidentialPO,
        Message_OverBudget,
        MSAuthorizeUrl,
        Message_ReceiptsRequired,
        ApprovalsPluginResponseSchema,
        ApprovalsPluginSystemMessageScope,
        ApprovalsPluginSystemMessageTools,
        ApprovalsPluginSystemMessageResponseBehavior,
        ApprovalsPluginSystemMessageFormatting

        #endregion Domain
    }

    public enum LogType
    {
        None = 0,
        Information = 1,
        Error = 2,
        Warning = 3,
    }

    public enum AzureTableRowDeletionResult
    {
        None = 0,
        DeletionSuccessful = 1,
        DeletionFailed = 2,
        SkippedDueToNonExistence = 3,
        SkippedDueToRaceCondition = 4
    }

    public enum DelegationAccessLevel
    {
        Admin = 1,
        ReadOnly = 0
    }

    public enum ValidationEntityName : int
    {
        None = 0,
        ApprovalRequestExpression,
        ApprovalIdentifier
    }

    [DataContract(Name = "ApprovalRequestResultType", Namespace = "http://www.microsoft.com/document/routing/2010/11")]
    public enum ApprovalRequestResultType : int
    {
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        [EnumMember(Value = "Success")]
        Success = 1,

        [EnumMember(Value = "Error")]
        Error = 2,

        [EnumMember(Value = "Pending")]
        Queued = 3,
    }

    public enum EmailType : int
    {
        ActionableEmail = 0, // Send actionable email with details
        NormalEmail = 1 // Send normal plan text email without details
    }

    public enum SaveOptions
    {
        ContinueOnError = 0,
        ReplaceOnUpdate = 1
    }

    public enum OperationType
    {
        Summary = 1,
        Detail = 2,
        Action = 3,
        SummaryFetchInitiated = 4,
        DetailFetchInitiated = 5,
        ActionInitiated = 6,
        SummaryFetchComplete = 7,
        ActionComplete = 8,
        DetailFetchComplete = 9,
        TenantActionDetail = 10
    }

    public enum ActionSubmissionType
    {
        Single,
        PseudoBulk,
        Bulk,
        BulkExternal
    }

    public enum SerializerType : int
    {
        DataContractSerializer = 1
    }

    public enum CriticalityLevel : int
    {
        Yes = 0,
        No = 1
    }

    public enum NotifyEmailWithMobileFriendlyActionCard : int
    {
        DisableForAll = 0,
        EnableForFlightedUsers = 1,
        EnableForAll = 2
    }

    public enum AuthenticationModelType : int
    {
        SAS = 0,
        OAuth2 = 1,
        CorpSTS = 2,
        OAuth2OnBehalf = 3,
        UserOnBehalf = 4,
        ManagedIdentityToken = 5,
        OAuth2ClientCredentials = 6
    }

    public enum DataCallType
    {
        Summary = 0,
        Details = 1,
        All = 2
    }

    public enum FlightingFeatureName : int
    {
        UploadAttachment = 29,
        AttachmentCopilot = 30,
        DiscrepancySummarization = 33,
        AIAnalysis = 39,
        SubmitterView = 40,
        AdaptiveCardCopilot = 41
    }

    public enum FlightingFeatureStatus : int
    {
        Disabled = 1, // The feature is disabled
        EnabledForAll = 2, // Not a flighting feature; feature in production
        InFlighting = 3 // The feature is a flighting feature
    }

    public enum TemplateType
    {
        Summary = 0,
        Details = 1,
        Action = 2,
        Footer = 3,
        All = 4,
        Full = 5,
        DetailsAddOn = 6
    }

    public enum EnableModernAdaptiveUI : int
    {
        DisableForAll = 0,
        EnableForFlightedUsers = 1,
        EnableForAll = 2
    }

    public enum EnableMSTeamsNotification : int
    {
        DisableForAll = 0,
        EnableForFlightedUsers = 1,
        EnableForAll = 2
    }

    public enum TenantLevelFlighting : int
    {
        DisableForAll = 0,
        EnableForFlightedUsers = 1,
        EnableForAll = 2
    }

    public enum AdobeEventType : int
    {
        AGREEMENT_CREATED = 0,
        AGREEMENT_ACTION_REQUESTED = 1,

        AGREEMENT_ACTION_REPLACED_SIGNER = 2,
        AGREEMENT_PARTICIPANT_REPLACED = 3,
        AGREEMENT_ACTION_DELEGATED = 4,

        AGREEMENT_PARTICIPANT_COMPLETED = 5,
        AGREEMENT_ACTION_COMPLETED = 6,

        AGREEMENT_EXPIRATION_UPDATED = 7,
        AGREEMENT_MODIFIED = 8,
        AGREEMENT_DOCUMENTS_DELETED = 9,

        AGREEMENT_DELETED = 10,
        AGREEMENT_RECALLED = 11,
        AGREEMENT_REJECTED = 12,
        AGREEMENT_EXPIRED = 13,
        AGREEMENT_WORKFLOW_COMPLETED = 14
    }

    public static class CopilotMessageType
    {
        public static string Message = "Message";
        public static string AdaptiveCard = "AdaptiveCard";
        public static string WebComponent = "WebComponent";
    }

    public static class OfficeDocumentType
    {
        public static string NONE = "NONE";
        public static string WORD = "WORD";
        public static string EXCEL = "EXCEL";
    }

    public enum NotificationBannerType : int
    {
        Danger = 1, //Approvals Critical Notification
        Warning = 2, //LOB Critical Notification
        Info = 3, //Informational/Future events
    }

    public static class QueryComparisons
    {
        //
        // Summary:
        //     Represents the Equal operator.
        public static string Equal = "eq";

        //
        // Summary:
        //     Represents the Not Equal operator.
        public static string NotEqual = "ne";

        //
        // Summary:
        //     Represents the Greater Than operator.
        public static string GreaterThan = "gt";

        //
        // Summary:
        //     Represents the Greater Than or Equal operator.
        public static string GreaterThanOrEqual = "ge";

        //
        // Summary:
        //     Represents the Less Than operator.
        public static string LessThan = "lt";

        //
        // Summary:
        //     Represents the Less Than or Equal operator.
        public static string LessThanOrEqual = "le";
    }

    public static class TableOperators
    {
        public static string And = "and";
    }

    public enum AuditOperationType
    {
        Read = 1,
        Update,
        Create,
        Delete
    }

    public enum AuditOperationResult
    {
        Success = 1,
        Failure
    }

    public enum UserPreferenceType
    {
        FeaturePreferenceJson,
        QuickTourFeatureList,
        ReadNotificationsList,
        None
    }

    public enum PageType
    {
        Summary,
        History,
        Details
    }

    public enum CopilotUserContextType
    {
        DASHBOARD,
        SUMMARY,
        DETAILS,
        ATTACHMENT,
        ACTION,
        HISTORY,
        HELP,
        ERROR,
        SEARCH,
        ANALYSIS,
        DEFAULT,
        EXTERNAL,
        APPROVALS // Combined context for Details, Attachment, Summary, Default
    }

    public enum CopilotErrorType
    {
        None = 0,
        OutOfSync = 1,
        SafeLimit = 2
    }

    public enum ChatToolFunctionNames
    {
        OnErrorOccurred,
        ExplainAndAskPermission,
        GetAIAssistedSearchResults,
        GetRequestDetails,
    }

    public enum ActionConsentType
    {
        No = 0,
        Yes = 1
    }

    public enum SearchResultReturnType
    {
        DocumentNumbers = 0, // Returns only document numbers
        FullDetails = 1 // Returns full details of the search results
    }
}