// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Extensions;

/// <summary>
/// Class ApprovalTenantInfo.
/// </summary>
/// <seealso cref="Microsoft.WindowsAzure.Storage.Table.TableEntity" />
public class ApprovalTenantInfo : BaseTableEntity
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    /// <value>The tenant identifier.</value>
    public int TenantId // Since we map Rowkey to Tenant Id (String containing values 1 or 2 etc)
    {
        get
        {
            return iTenantId;
        }
    }

    /// <summary>
    /// Gets the i tenant identifier.
    /// </summary>
    /// <value>The i tenant identifier.</value>
    private int iTenantId // Created just to handle parse exception
    {
        get
        {
            int value = 0;
            int.TryParse(base.RowKey, out value);
            return value;
        }
    }

    /// <summary>
    /// Gets the application identifier.
    /// </summary>
    /// <value>The application identifier.</value>
    public string AppId
    { get { return base.PartitionKey; } } // We map PartitionKey to Application Id (GUID)

    /// <summary>
    /// Gets or sets the document type identifier.
    /// </summary>
    /// <value>The document type identifier.</value>
    public string DocTypeId { get; set; }

    /// <summary>
    /// Gets or sets the registered client names
    /// </summary>
    public string RegisteredClients { get; set; }

    /// <summary>
    /// Gets or sets the Actionable NotificationTemplate Keys
    /// </summary>
    public string ActionableNotificationTemplateKeys { get; set; }

    /// <summary>
    /// Gets or sets the tenant base URL.
    /// </summary>
    /// <value>The tenant base URL.</value>
    public string TenantBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the summary URL.
    /// </summary>
    /// <value>The summary URL.</value>
    public string SummaryURL { get; set; }

    /// <summary>
    /// Gets or sets the notify WP8.
    /// </summary>
    /// <value>The notify WP8.</value>
    public bool NotifyWp8 { get; set; }

    /// <summary>
    /// Gets or sets the notify android.
    /// </summary>
    /// <value>The notify android.</value>
    public bool NotifyAndroid { get; set; }

    /// <summary>
    /// Gets or sets the notify ios.
    /// </summary>
    /// <value>The notify ios.</value>
    public bool NotifyIOS { get; set; }

    /// <summary>
    /// Gets or sets the notify win8.
    /// </summary>
    /// <value>The notify win8.</value>
    public bool NotifyWin8 { get; set; }

    /// <summary>
    /// Gets or sets the notify win10.
    /// </summary>
    /// <value>The notify win10.</value>
    public bool NotifyWin10 { get; set; }

    /// <summary>
    /// Gets or sets the notify email.
    /// </summary>
    /// <value>The notify email.</value>
    public bool NotifyEmail { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to notify teams with pending request
    /// </summary>
    /// <value>0</value> Disable for tenant even if feature flighted
    /// <value>1</value> Check for feature flighting for the user
    /// <value>2</value> Enable for all users
    public int NotifyTeams { get; set; }

    /// <summary>
    /// Gets or sets the Notify MSTeams client with watchdog reminders flag
    /// </summary>
    public bool NotifyTeamsWithWatchdogReminder { get; set; }

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    /// <value>The name of the application.</value>
    public string AppName { get; set; }

    /// <summary>
    /// Gets or sets the tenant detail URL.
    /// </summary>
    /// <value>The tenant detail URL.</value>
    public string TenantDetailUrl { get; set; }

    /// <summary>
    /// Gets or sets the tenant operation details.
    /// </summary>
    /// <value>The tenant operation details.</value>
    public string TenantOperationDetails { get; set; }

    /// <summary>
    /// Gets or sets the subscription.
    /// </summary>
    /// <value>The subscription.</value>
    public string Subscription { get; set; }

    /// <summary>
    /// Gets or sets the subscription filter.
    /// </summary>
    /// <value>The subscription filter.</value>
    public string SubscriptionFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [tenant enabled].
    /// </summary>
    /// <value><c>true</c> if [tenant enabled]; otherwise, <c>false</c>.</value>
    public bool TenantEnabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum tenant threads.
    /// </summary>
    /// <value>The maximum tenant threads.</value>
    public int MaxTenantThreads { get; set; }

    /// <summary>
    /// Gets or sets the tenant action details.
    /// </summary>
    /// <value>The tenant action details.</value>
    public string TenantActionDetails { get; set; }

    /// <summary>
    /// Gets or sets the attachment properties for the tenant that gives the container name and metadata for the feature.
    /// </summary>
    public string AttachmentProperties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether upload attachment feature is enabled for a tenant.
    /// </summary>
    /// <value><c>true</c> if upload attachments feature is enabled for tenant; otherwise, <c>false</c>.</value>
    public bool IsUploadAttachmentsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is day zero activity running.
    /// </summary>
    /// <value><c>true</c> if this instance is day zero activity running; otherwise, <c>false</c>.</value>
    public bool IsDayZeroActivityRunning { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [process attached summary].
    /// </summary>
    /// <value><c>true</c> if [process attached summary]; otherwise, <c>false</c>.</value>
    public bool ProcessAttachedSummary { get; set; }

    /// <summary>
    /// Gets or sets the process secondary action.
    /// </summary>
    /// <value>The process secondary action.</value>
    public int ProcessSecondaryAction { get; set; }

    /// <summary>
    /// Gets or sets the maximum tenant threads for secondary.
    /// </summary>
    /// <value>The maximum tenant threads for secondary.</value>
    public int MaxTenantThreadsForSecondary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is history clickable.
    /// </summary>
    /// <value><c>true</c> if this instance is history clickable; otherwise, <c>false</c>.</value>
    public bool IsHistoryClickable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [notify email with approval functionality].
    /// </summary>
    /// <value><c>true</c> if [notify email with approval functionality]; otherwise, <c>false</c>.</value>
    public bool NotifyEmailWithApprovalFunctionality { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [notify email with approval functionality in outlook for watchdog reminder emails].
    /// </summary>
    /// <value><c>true</c> if [notify email with approval functionality]; otherwise, <c>false</c>.</value>
    public bool NotifyWatchDogEmailWithApprovalFunctionality { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is race condition handled.
    /// </summary>
    /// <value><c>true</c> if this instance is race condition handled; otherwise, <c>false</c>.</value>
    public bool IsRaceConditionHandled { get; set; }

    /// <summary>
    /// Gets or sets the race condition sleep time in second.
    /// </summary>
    /// <value>The race condition sleep time in second.</value>
    public int RaceConditionSleepTimeInSecond { get; set; }

    /// <summary>
    /// Gets or sets the value of Ignore Current Approver Check flag
    /// </summary>
    /// <value>true or false</value>
    public bool IgnoreCurrentApproverCheck { get; set; }

    /// <summary>
    /// Gets the action details.
    /// </summary>
    /// <value>The action details.</value>
    public TenantActionDetails ActionDetails
    {
        get
        {
            if (!string.IsNullOrEmpty(TenantActionDetails))
                return TenantActionDetails.FromJson<TenantActionDetails>();
            return null;
        }
    }

    /// <summary>
    /// List of string which contains the names of the registered clients.
    /// Else return an empty list
    /// </summary>
    public IList<string> RegisteredClientsList
    {
        get
        {
            if (!string.IsNullOrEmpty(RegisteredClients))
                return RegisteredClients.FromJson<List<string>>().ConvertAll(t => t.ToUpper());
            return new List<string>();
        }
    }

    /// <summary>
    /// List of string which contains the actions which are enable for  Email With ApprovalFunctionality in outlook.
    /// Else return an empty list
    /// </summary>
    public IList<string> ActionableNotificationTemplateKeysList
    {
        get
        {
            if (!string.IsNullOrEmpty(ActionableNotificationTemplateKeys))
                return ActionableNotificationTemplateKeys.FromJson<List<string>>();
            return new List<string>();
        }
    }

    /// <summary>
    /// Array index will specify the section been queried by user
    /// Default 0 which would be reserved for summary and followed by details and
    /// last item in array would be for ACTION
    /// </summary>
    public TenantOperationDetails DetailOperations
    {
        get
        {
            if (!string.IsNullOrEmpty(TenantOperationDetails))
                return TenantOperationDetails.FromJson<TenantOperationDetails>();
            return null;
        }
    }

    /// <summary>
    /// Gets or sets the history logging.
    /// </summary>
    /// <value>The history logging.</value>
    public bool HistoryLogging { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApprovalTenantInfo"/> class.
    /// </summary>
    public ApprovalTenantInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApprovalTenantInfo"/> class.
    /// </summary>
    /// <param name="strPartitionKey">The string partition key.</param>
    /// <param name="strRowKey">The string row key.</param>
    public ApprovalTenantInfo(string strPartitionKey, string strRowKey)
        : base(strPartitionKey, strRowKey)
    {
    }

    /// <summary>
    /// Gets or sets the document number prefix.
    /// </summary>
    /// <value>The document number prefix.</value>
    public string DocumentNumberPrefix { get; set; }

    /// <summary>
    /// Gets or sets the name of the template.
    /// </summary>
    /// <value>The name of the template.</value>
    public string TemplateName { get; set; }

    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    /// <value>The name of the tool.</value>
    public string ToolName { get; set; }

    /// <summary>
    /// Gets or sets the name of the class.
    /// </summary>
    /// <value>The name of the class.</value>
    public string ClassName { get; set; }

    /// <summary>
    /// Gets or sets the use document number for row key.
    /// </summary>
    /// <value>The use document number for row key.</value>
    public bool UseDocumentNumberForRowKey { get; set; }

    /// <summary>
    /// Gets or sets the tenant message processing enabled.
    /// </summary>
    /// <value>The tenant message processing enabled.</value>
    public bool TenantMessageProcessingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the service parameter.
    /// </summary>
    /// <value>The service parameter.</value>
    public string ServiceParameter { get; set; }

    /// <summary>
    /// Gets or sets the name of the validation class.
    /// </summary>
    /// <value>The name of the validation class.</value>
    public string ValidationClassName { get; set; }

    /// <summary>
    /// Gets or sets the tenant detail retry count.
    /// </summary>
    /// <value>The tenant detail retry count.</value>
    public int TenantDetailRetryCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [enable user delegation].
    /// </summary>
    /// <value><c>true</c> if [enable user delegation]; otherwise, <c>false</c>.</value>
    public bool EnableUserDelegation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [enable external user delegation].
    /// </summary>
    /// <value><c>true</c> if [enable external user delegation]; otherwise, <c>false</c>.</value>
    public bool EnableExternalUserDelegation { get; set; }

    /// <summary>
    /// Gets or sets the type of the tenant.
    /// </summary>
    /// <value>The type of the tenant.</value>
    public string TenantType { get; set; }

    /// <summary>
    /// Gets or sets the name of the business process.
    /// </summary>
    /// <value>The name of the business process.</value>
    public string BusinessProcessName { get; set; }

    /// <summary>
    /// Gets or sets the bulk action concurrent call.
    /// </summary>
    /// <value>The bulk action concurrent call.</value>
    public int BulkActionConcurrentCall { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is background processing enabled upfront.
    /// </summary>
    /// <value><c>true</c> if this instance is background processing enabled upfront; otherwise, <c>false</c>.</value>
    public bool IsBackgroundProcessingEnabledUpfront { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the details fields are editable for a tenant
    /// </summary>
    /// <value><c>true</c> if the details fields are editable
    public bool IsDetailsEditable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant is enabled for controls and complicance or not
    /// If this is set as false, reading/viewing details before taking action is not enforced.
    /// </summary>
    /// <value><c>false</c> if control and compliance check is not required
    public bool IsControlsAndComplianceRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether pictorial line items are enabled.
    /// </summary>
    /// <value><c>true</c> if pictorial line items are enabled; otherwise, <c>false</c>.</value>
    public bool IsPictorialLineItemsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the filter categories for Line Items pictorial view.
    /// </summary>
    /// <value>The filter categories for Line Items pictorial view.</value>
    public string LineItemFilterCategories { get; set; }

    /// <summary>
    /// Gets or sets a value indicating which type of action submission does the tenant support
    /// <value>0</value> if single request submission
    /// <value>1</value> if pseudo bulk request submission
    /// <value>2</value> if bulk requests submission
    /// </summary>
    public int ActionSubmissionType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether tenant is subscribed for digest emails
    /// </summary>
    /// <value><c>true</c> if sending digest emails is enabled</value>
    public bool IsDigestEmailEnabled { get; set; }

    /// <summary>
    /// Background approval retry interval
    /// </summary>
    public int BackgroundApprovalRetryInterval { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether tenant is enabled for pull-model.
    /// </summary>
    /// <value><c>true</c> if this instance is enabled for pull-model; otherwise, <c>false</c>.</value>
    public bool IsPullModelEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether tenant is enabled for fetching action details.
    /// </summary>
    /// <value><c>true</c> if this instance is enabled for ExternalTenantActionDetails; otherwise, <c>false</c>.</value>
    public bool IsExternalTenantActionDetails { get; set; }

    /// <summary>
    /// Summary endpoint URL for pull-model tenant.
    /// </summary>
    public string TenantSummaryUrl { get; set; }

    /// <summary>
    /// The compliance message which needs to be displayed on the UI before any action is taken by the tenant
    /// </summary>
    public string ComplianceAcknowledgementText { get; set; }

    /// <summary>
    /// The compliance message which is displayed on the Tabular Summary Page/ Details Page
    /// </summary>
    public string AdditionalNotes { get; set; }

    /// <summary>
    /// The template for showing the details of the request context in the Action confirmation page
    /// Following are the possible values. Pelase have the below place holders as per the allocated number
    /// {0} unitValue
    /// {1} unitOfMeasure
    /// {2} submitterName
    /// {3} submittedDate
    /// {4} documentNumberPrefix
    /// {5} documentNumber
    /// {6} appName
    /// {7} title
    /// </summary>
    public string ActionConfirmationRequestDetailsTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value of ServiceTreeId for specific tenant
    /// </summary>
    public string ServiceTreeId { get; set; }

    /// <summary>
    /// Gets or sets a value of DataModelMapping for specific tenant
    /// </summary>
    public string DataModelMapping { get; set; }

    /// <summary>
    /// Gets or sets a value of DataMapping for specific tenant
    /// </summary>
    public string SummaryDataMapping { get; set; }

    /// <summary>
    /// Gets or sets a value of ButtonDisabledReason for specific tenant
    /// </summary>
    public string ButtonDisabledReason { get; set; }

    /// <summary>
    /// Gets or sets a value of IsSameApproverMultipleLevelSupported for specific tenant
    /// </summary>
    public bool IsSameApproverMultipleLevelSupported { get; set; }

    /// <summary>
    /// Gets or sets a value of IsAllCurrentApproversDisplayInHierarchy for specific tenant
    /// </summary>
    public bool IsAllCurrentApproversDisplayInHierarchy { get; set; }

    /// <summary>
    /// Gets or sets a value of IsOldHierarchyEnabled for specific tenant
    /// </summary>
    public bool IsOldHierarchyEnabled { get; set; }

    /// <summary>
    /// Gets or sets the ActionableEmailFolderName for each Tenant.
    /// </summary>
    /// <value>The ActionableEmailFolderName is the folder name in blob storage which contains the configuration files for Outlook Actionable emails.</value>
    public string ActionableEmailFolderName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify email with mobile friendly action card.
    /// </summary>
    /// <value>0</value> Disable for tenant even if feature flighted
    /// <value>1</value> Check for feature flighting for the user
    /// <value>2</value> Enable for all users
    public int NotifyEmailWithMobileFriendlyActionCard { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable the modern adaptive templating UI for a tenant
    /// </summary>
    /// <value>0</value> Disable for tenant even if feature flighted
    /// <value>1</value> Check for feature flighting for the user
    /// <value>2</value> Enable for all users
    public int EnableModernAdaptiveUI { get; set; }

    private string adaptiveCardVersion;

    /// <summary>
    /// Gets or sets value indicating the current latest version number of Adaptive Card
    /// </summary>
    public string AdaptiveCardVersion
    {
        get
        {
            if (string.IsNullOrEmpty(adaptiveCardVersion))
                return "v1";
            return adaptiveCardVersion;
        }
    }

    public bool AppendAdditionaDataToUserActionString { get; set; }

    /// <summary>
    /// Gets or sets Tenant Image Information for image to be retrieved from blob storage
    /// </summary>
    public string TenantImage { get; set; }

    /// <summary>
    /// Gets Tenant Image as Class
    /// </summary>
    public TenantImageInfo TenantImageDetails { get; set; }

    /// <summary>
    /// Gets or sets the Action Additional properties
    /// </summary>
    public string ActionAdditionalProperties { get; set; }

    /// <summary>
    /// List of string which contains the values of the Action Additional Properties.
    /// Else return an empty list
    /// </summary>
    public IList<string> ActionAdditionalPropertiesList
    {
        get
        {
            if (!string.IsNullOrEmpty(ActionAdditionalProperties))
                return ActionAdditionalProperties.FromJson<List<string>>();
            return new List<string>();
        }
    }

    /// <summary>
    /// supporting field for property details component settings
    /// </summary>
    public string DetailsComponentSettings { get; set; }

    /// <summary>
    /// Gets or sets details component settings
    /// </summary>
    public DetailsComponentInfo DetailsComponentInfo
    {
        get
        {
            if (DetailsComponentSettings == null)
            {
                return new DetailsComponentInfo();
            }
            else
            {
                return DetailsComponentSettings.FromJson<DetailsComponentInfo>();
            }
        }
        set
        {
            DetailsComponentSettings = value.ToJson();
        }
    }
}

#region Operation Details

/// <summary>
/// Class TenantOperationDetails.
/// </summary>
public class TenantOperationDetails
{
    /// <summary>
    /// Gets or sets the detail ops list.
    /// </summary>
    /// <value>The detail ops list.</value>
    public List<TenOpsDetails> DetailOpsList { get; set; }
}

/// <summary>
/// Used to store tenant enpointdata which needs to be appended to baseurl to formulate service url
/// e.g.: For Approval service for gettting summary information from LOB
/// baseurl: https://[lobappbaseurl]/
/// endpoint : Services/ApprovalDataService/GetApprovalSummary
/// SupportedPagination: False
/// </summary>
public class TenOpsDetails
{
    /// <summary>
    /// SUM: For summary endpoint
    /// DETL: For detail url
    /// </summary>
    public string operationtype { get; set; }

    /// <summary>
    /// Gets or sets the endpointdata.
    /// </summary>
    /// <value>The endpointdata.</value>
    public string endpointdata { get; set; }

    /// <summary>
    /// Gets or sets the supports pagination.
    /// </summary>
    /// <value>The supports pagination.</value>
    public bool SupportsPagination { get; set; }

    /// <summary>
    /// Gets or sets the client.
    /// </summary>
    /// <value>The client.</value>
    public bool _client { get; set; }

    /// <summary>
    /// Gets or sets the is cached.
    /// </summary>
    /// <value>The is cached.</value>
    public bool IsCached { get; set; }

    /// <summary>
    /// Gets or sets the type of the serializer.
    /// </summary>
    /// <value>The type of the serializer.</value>
    public int SerializerType { get; set; }

    /// <summary>
    /// Gets or sets value whether Tenant has Legacy response or not
    /// </summary>
    public bool IsLegacyResponse { get; set; }
}

#endregion Operation Details

#region Action Details

/// <summary>
/// Class TenantActionDetails.
/// </summary>
public class TenantActionDetails
{
    /// <summary>
    /// Gets or sets the primary.
    /// </summary>
    /// <value>The primary.</value>
    public List<TenantAction> Primary { get; set; }

    /// <summary>
    /// Gets or sets the secondary.
    /// </summary>
    /// <value>The secondary.</value>
    public List<TenantAction> Secondary { get; set; }
}

/// <summary>
/// Class TenantAction.
/// </summary>
public class TenantAction
{
    /// <summary>
    /// This property sets the default value of IsJustificationApplicable property to true
    /// </summary>
    private bool isJustificationApplicable = true;

    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    /// <value>The condition.</value>
    public string Condition { get; set; }

    /// <exclude />
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the justifications.
    /// </summary>
    /// <value>The justifications.</value>
    public List<TenantActionJustification> Justifications { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is comment mandatory.
    /// </summary>
    /// <value><c>true</c> if this instance is comment mandatory; otherwise, <c>false</c>.</value>
    public bool IsCommentMandatory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is justification mandatory.
    /// </summary>
    /// <value><c>true</c> if this instance is justification mandatory; otherwise, <c>false</c>.</value>
    public bool IsJustificationApplicable
    {
        get
        {
            return isJustificationApplicable;
        }
        set
        {
            isJustificationApplicable = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is interim state required.
    /// </summary>
    /// <value><c>true</c> if this instance is interim state required; otherwise, <c>false</c>.</value>
    public bool IsInterimStateRequired { get; set; }

    /// <summary>
    /// Gets or sets the length of the comment.
    /// </summary>
    /// <value>The length of the comment.</value>
    public int CommentLength { get; set; }

    /// <summary>
    /// Gets or sets the target page.
    /// </summary>
    /// <value>The target page.</value>
    public List<TenantActionTargetPage> TargetPage { get; set; }

    /// <summary>
    /// Gets or sets the placements.
    /// </summary>
    /// <value>The placements.</value>
    public List<TenantActionPlacement> Placements { get; set; }

    /// <summary>
    /// Gets or sets additional Information
    /// </summary>
    public List<TenantActionAdditionalInformation> AdditionalInformation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is bulk action.
    /// </summary>
    /// <value><c>true</c> if this instance is bulk action; otherwise, <c>false</c>.</value>
    public bool IsBulkAction { get; set; }

    /// <summary>
    /// Gets or sets a value of Action Confirmation Message.
    /// </summary>
    /// <value>the action confirmation message</value>
    public string ActionConfirmationMessage { get; set; }

    /// <summary>
    /// Gets or sets a value of Comments
    /// </summary>
    /// <value>Configuration for Comments</value>
    public List<Comment> Comments { get; set; }
}

/// <summary>
/// Class TenantActionJustification.
/// </summary>
public class TenantActionJustification
{
    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    /// <value>The condition.</value>
    public string Condition { get; set; }

    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    /// <value>The code.</value>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    /// <value>The key.</value>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the description of the justification item.
    /// </summary>
    /// <value>The description.</value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TenantActionJustification"/> is default.
    /// </summary>
    /// <value><c>true</c> if default; otherwise, <c>false</c>.</value>
    public bool _default { get; set; }
}

/// <summary>
/// Class TenantActionTargetPage.
/// </summary>
public class TenantActionTargetPage
{
    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    /// <value>The condition.</value>
    public string Condition { get; set; }

    /// <summary>
    /// Gets or sets the type of the page.
    /// </summary>
    /// <value>The type of the page.</value>
    public string PageType { get; set; }

    /// <summary>
    /// Gets or sets the delay time.
    /// </summary>
    /// <value>The delay time.</value>
    public int DelayTime { get; set; }
}

/// <summary>
/// Class TenantActionPlacement.
/// </summary>
public class TenantActionPlacement
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    /// <value>The code.</value>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    /// <value>The condition.</value>
    public string Condition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TenantActionPlacement"/> is default.
    /// </summary>
    /// <value><c>true</c> if default; otherwise, <c>false</c>.</value>
    public bool _default { get; set; }

    /// <summary>
    /// Gets or sets html ControlType
    /// </summary>
    /// <value>Html control type </value>
    public string ControlType { get; set; }

    /// <summary>
    /// Gets or sets the LabelText
    /// </summary>
    /// <value>LabelText</value>
    public string LabelText { get; set; }
}

/// <summary>
/// TenantActionAdditionalInformation class
/// </summary>
public class TenantActionAdditionalInformation
{
    /// <summary>
    /// Gets or sets Action code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets Action text
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets control type
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// List of values if type is Select or Choices
    /// </summary>
    public List<Value> Values { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to get value from Summary Object.
    /// </summary>
    public bool IsValueFromSummaryObject { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether property is Mandatory.
    /// </summary>
    public bool IsMandatory { get; set; }
}

/// <summary>
/// Value class
/// </summary>
public class Value
{
    /// <summary>
    /// Gets or sets Action text
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets Action code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets condition for which to display this option
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// Gets or sets the flag to determine if the value is default
    /// </summary>
    public bool _default { get; set; }
}

/// <summary>
/// Class Comment
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets a value indicating comment configuration is for which part of object using key mapping
    /// </summary>
    public string MappingKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether comment is mandatory or not.
    /// </summary>
    /// <value><c>true</c> if comment is mandatory; otherwise, <c>false</c>.</value>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    /// <value>The condition.</value>
    public string MandatoryCondition { get; set; }

    /// <summary>
    /// Gets or sets the length of the comment.
    /// </summary>
    /// <value>The length of the comment.</value>
    public int Length { get; set; }
}

#endregion Action Details

public class TenantImageInfo
{
    /// <summary>
    /// Gets or Sets File Name
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or Sets File Type
    /// </summary>
    public string FileType { get; set; }

    /// <summary>
    /// Gets or Sets File URL
    /// </summary>
    public string FileURL { get; set; }

    /// <summary>
    /// Gets or Sets File Base 64
    /// </summary>
    public string FileBase64 { get; set; }
}

public class DetailsComponentInfo
{
    /// <summary>
    /// Gets or sets component type
    /// </summary>
    public DetailsComponentType DetailsComponentType { get; set; }

    /// <summary>
    /// Gets or sets CDN url
    /// </summary>
    public string CdnUrl { get; set; }
}