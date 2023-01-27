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

        AADInstance,
        AADTenantId,
        AcceptsTerms,
        ActionAlreadyTakenFromApprovalsMessage,
        ActionAlreadyTakenMessage,
        ActionAuditLogAzureTableName,
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
        ARConverterClass,
        ArxQueueWaitTime,
        AttachmentSizeLimit, // AttachmentSizeLimit beyond which, it will send normal email without attachment.
        AzureSearchServiceName,
        AzureSearchServiceQueryApiKey,
        AzureSearchTransactionHistoryIndexName,
        BulkActionConcurrentCallMessage,
        ComponentName,
        CosmosDbAuthKey,
        CosmosDbCollectionActionAuditLog,
        CosmosDbCollectionAuditAgent,
        CosmosDbEndPoint,
        CosmosDbNameActionAuditLog,
        CosmosDbNameAuditAgent,
        CosmosDbPartitionKeyPathAuditAgent,
        CosmosDbPartitionKeyPath,
        DaysForReminderMails, // No of days for which we need to send reminder mail
        DetailControllerExceptionMessage,
        DeviceDeepLinkUrl,
        DocDBNameAuditAgent,
        DocDBCollectionAuditAgent,
        DomainName,
        EditableConfigurationTableName,
        EnableValidation,
        EnvironmentName,
        GraphAPIAuthString,
        GraphAPIClientId,
        GraphAPIClientSecret,
        HistoryPageSize,
        InvalidRequestException,
        IsAzureSearchEnabled,
        MainTopicFailCountThreshold,
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
        NotificationBCCEmailAddress,
        NotificationBroadcastUri, // Notification Framework related configuration keys.
        NotificationFrameworkAuthKey,
        NotificationFrameworkClientId,
        NotificationFrameworkMaxRetries, // MaxRetries to Notification Framework.
        NotificationFrameworkResourceUrl, // ResourceUrl to make call to Notification Framework.
        ReceiptAcknowledgmentMessage,
        ReconciliationAwaitTime,
        RelayServiceDownMessage,
        RetryPolicyRetryCount,
        SaveChangesOptionsContinueOnError,
        ServiceBusConnectionString,
        ServiceBusIssuerName,
        ServiceBusIssuerSecret,
        ServiceBusNamespace,
        ServiceComponentId,
        ServiceLineName,
        ServiceName,
        ServiceOfferingName,
        ServiceParameterAuthKey,
        ServiceParameterClientID,
        StorageAccountKey,
        StorageAccountName,
        SubscriptionNameNotification,
        SupportEmailId,
        SyntheticTransactionsApproverAliasList,
        SyntheticTransactionsLoadBatchDelay,
        TenantAPICallTimeoutValueInMins, // TenantAPICallTimeoutValueInMins
        TenantDownTimeMessagesAzureTableName,
        TopicNameMain,
        TopicNameNotification,
        TopicNameRetry,
        UnAuthorizedException,
        UrlPlaceholderTenants, // TenantIDs for which summary and detail URL contains placeholders with actual property names.
        UserMessageForComplianceAndAction,
        UserPreferenceAzureTableName,
        ValidateAliasUsingPayloadValidator,
        WatchDogBatchSize,
        WatchDogMaxFailureCount,

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
        /// Teams Tenant to generate AAD Token
        /// </summary>
        TeamsTenant,

        #endregion Common
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
        AAD = 1,
        CorpSTS = 2,
        AADOnBehalf = 3,
        UserOnBehalf = 4
    }

    public enum DataCallType
    {
        Summary = 0,
        Details = 1,
        All = 2
    }

    public enum FlightingFeatureName : int
    {
        ManageOutOfSync = 14,
        ActionableEmailWithMobileFriendlyAdaptiveCard = 26,
        ModernAdaptiveDetailsUI = 27,
        MSTeamNotification = 28
    }

    public enum FlightingFeatureStatus : int
    {
        Disabled = 1, // The feature is disabled
        EnabledForAll = 2, // Not a flighting feature; feature in production
        InFlighting = 3 // The feature is a flighting feature
    }

    public enum Tenant
    {
        Invoice = 1
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
}