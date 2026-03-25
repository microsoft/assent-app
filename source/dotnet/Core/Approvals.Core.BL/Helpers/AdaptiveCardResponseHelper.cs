using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    public class AdaptiveCardResponseHelper : IAdaptiveCardResponseHelper
    {
        #region Private Variables

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The adaptive details helper for fetching templates
        /// </summary>
        private readonly IAdaptiveDetailsHelper _adaptiveDetailsHelper;

        /// <summary>
        /// The approval tenant info helper for tenant configuration
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// The tenant factory for tenant-specific implementations
        /// </summary>
        private readonly ITenantFactory _tenantFactory;

        /// <summary>
        /// The log provider for logging
        /// </summary>
        private readonly ILogProvider _logProvider;

        #endregion Private Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveCardResponseHelper"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="adaptiveDetailsHelper">Adaptive details helper.</param>
        /// <param name="approvalTenantInfoHelper">Approval tenant info helper.</param>
        /// <param name="tenantFactory">Tenant factory.</param>
        /// <param name="logProvider">Log provider.</param>
        public AdaptiveCardResponseHelper(
            IConfiguration config,
            IAdaptiveDetailsHelper adaptiveDetailsHelper,
            IApprovalTenantInfoHelper approvalTenantInfoHelper,
            ITenantFactory tenantFactory,
            ILogProvider logProvider)
        {
            _config = config;
            _adaptiveDetailsHelper = adaptiveDetailsHelper;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _tenantFactory = tenantFactory;
            _logProvider = logProvider;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Creates a basic adaptive card with a single text block message.
        /// </summary>
        /// <param name="message">The message text to display.</param>
        /// <returns>Serialized adaptive card JSON string.</returns>
        public string CreateTextCard(string message)
        {
            AdaptiveTextBlock textBlock = CreateTextBlock(message);
            List<AdaptiveElement> elements = [textBlock];
            return ConstructAdaptiveCard(elements);
        }

        /// <summary>
        /// Constructs a take action card with a header, body text, and a list of actions
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="bodyText"></param>
        /// <param name="askRequest"></param>
        /// <param name="chatRequestContextArgs"></param>
        /// <returns></returns>
        public string CreateTakeActionCard(string headerText, string bodyText, AskRequest askRequest, ChatRequestEventArgs chatRequestContextArgs)
        {
            AdaptiveTextBlock headerTextBlock = CreateTextBlock(headerText, true, AdaptiveTextWeight.Default, AdaptiveTextSize.Default, AdaptiveTextBlockStyle.Heading);
            AdaptiveTextBlock textBlock = CreateTextBlock(bodyText);
            var actions = new List<(string Title, string Style, AdaptiveCardSubmissionData Data)>
                {
                    ("Yes", "Positive", CreateAdaptiveCardSubmissionData(
                        askRequest,
                        chatRequestContextArgs,
                        nameof(CopilotUserContextType.ERROR),
                        ActionConsentType.Yes,
                        $"{_config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]}/chatrequest",
                        $"{_config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]}/user_impersonation"
                        )
                    ),
                    ("No", "Default", CreateAdaptiveCardSubmissionData(
                        askRequest,
                        chatRequestContextArgs,
                        nameof(CopilotUserContextType.ERROR),
                        ActionConsentType.No,
                        $"{_config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]}/chatrequest",
                        $"{_config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]}/user_impersonation"
                        )
                    )
                };
            AdaptiveActionSet actionSet = CreateActionSet(actions);
            List<AdaptiveElement> elements = [headerTextBlock, textBlock, actionSet];
            // Create an Adaptive Card
            return ConstructAdaptiveCard(elements);
        }

        /// <summary>
        /// Creates an adaptive card using a pre-fetched template for parallel fetch scenarios.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="documentNumber">Document number of the approval request.</param>
        /// <param name="details">The request details data as JObject.</param>
        /// <param name="prefetchedTemplate">The pre-fetched adaptive card template.</param>
        /// <param name="userAlias">User alias.</param>
        /// <param name="loggedInAlias">Logged-in alias.</param>
        /// <param name="oauth2UserToken">OAuth 2.0 user token.</param>
        /// <param name="objectId">User's object ID.</param>
        /// <param name="domain">User's domain.</param>
        /// <param name="approverDisplayName">Display name of the approver (optional).</param>
        /// <param name="tcv">Transaction correlation vector for tracking (optional).</param>
        /// <returns>A sanitized Adaptive Card JObject suitable for Copilot/M365.</returns>
        public JObject CreateApprovalAssistantRequestCard(
            int tenantId,
            string documentNumber,
            JObject details,
            JObject prefetchedTemplate,
            string userAlias,
            string loggedInAlias,
            string oauth2UserToken,
            string objectId,
            string domain,
            string approverDisplayName = "",
            string tcv = "")
        {
            string resolvedTcv = string.IsNullOrEmpty(tcv) ? Guid.NewGuid().ToString() : tcv;

            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.DocumentNumber, documentNumber },
                { LogDataKey.UserAlias, userAlias },
                { LogDataKey.ClientDevice, Constants.FinanceAssistantClient },
                { LogDataKey.Tcv, resolvedTcv }
            };

            _logProvider.LogInformation(TrackingEvent.ApprovalAssistantCardGenerationInitiated, logData);

            #endregion Logging

            try
            {
                ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
                JObject adaptiveCard = GenerateAdaptiveCard(tenantInfo, userAlias, oauth2UserToken, objectId, domain, details, prefetchedTemplate, tenantId, documentNumber);

                string adaptiveCardJson = ReplaceApproverPlaceholders(
                    adaptiveCard.ToString(),
                    details,
                    userAlias,
                    loggedInAlias,
                    approverDisplayName,
                    resolvedTcv);

                JObject result = JObject.Parse(adaptiveCardJson);
                InjectScopeIntoSubmitActions(result);

                #region Logging

                logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
                logData[LogDataKey.TenantName] = tenantInfo.AppName;
                _logProvider.LogInformation(TrackingEvent.ApprovalAssistantCardGenerationSuccess, logData);

                #endregion Logging

                return result;
            }
            catch (Exception ex)
            {
                logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
                _logProvider.LogError(TrackingEvent.ApprovalAssistantCardGenerationFailure, ex, logData);
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Generates the adaptive card by populating the template with details data.
        /// </summary>
        private JObject GenerateAdaptiveCard(
            ApprovalTenantInfo tenantInfo,
            string userAlias,
            string oauth2UserToken,
            string objectId,
            string domain,
            JObject details,
            JObject fullTemplate,
            int tenantId,
            string documentNumber)
        {
            ITenant tenantAdaptor = _tenantFactory.GetTenant(
                tenantInfo,
                userAlias,
                Constants.TeamsClient,
                oauth2UserToken,
                objectId,
                domain);

            var cardLogData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.TenantName, tenantInfo.AppName },
                { LogDataKey.DocumentNumber, documentNumber }
            };

            return tenantAdaptor.GenerateAndModifyAdaptiveCard(
                template: fullTemplate.ToString(),
                responseJObject: details,
                logData: cardLogData);
        }

        /// <summary>
        /// Replaces approver-specific placeholders in the adaptive card JSON.
        /// These placeholders are not handled at template-fetch time and are required for Action.Submit data.
        /// </summary>
        private static string ReplaceApproverPlaceholders(
            string adaptiveCardJson,
            JObject details,
            string userAlias,
            string loggedInAlias,
            string approverDisplayName,
            string tcv)
        {
            string resolvedApproverName = ResolveApproverName(details, loggedInAlias, approverDisplayName);
            string resolvedApproverAlias = ResolveApproverAlias(details, userAlias);

            return adaptiveCardJson
                .Replace(Constants.ApproverNamePlaceholder, resolvedApproverName)
                .Replace(Constants.ApproverAliasPlaceholder, resolvedApproverAlias)
                .Replace(Constants.ActionDatePlaceholder, DateTime.UtcNow.ToString("o"))
                .Replace(Constants.TcvPlaceholder, tcv);
        }

        /// <summary>
        /// Resolves the approver name from details or fallback values.
        /// </summary>
        private static string ResolveApproverName(JObject details, string loggedInAlias, string approverDisplayName)
        {
            string approverName = details?.SelectToken("Approver.Name")?.ToString();
            if (!string.IsNullOrEmpty(approverName))
            {
                return approverName;
            }

            return string.IsNullOrEmpty(approverDisplayName) ? loggedInAlias : approverDisplayName;
        }

        /// <summary>
        /// Resolves the approver alias from details or fallback values.
        /// </summary>
        private static string ResolveApproverAlias(JObject details, string userAlias)
        {
            string approverAlias = details?.SelectToken("Approver.Alias")?.ToString();
            return string.IsNullOrEmpty(approverAlias) ? userAlias : approverAlias;
        }

        /// <summary>
        /// Injects the "scope" field into all Action.Submit data objects so FA can acquire a token to call our API.
        /// </summary>
        private void InjectScopeIntoSubmitActions(JObject card)
        {
            string scope = $"{_config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]}/user_impersonation";
            foreach (var token in card.SelectTokens("$..actions[?(@.type=='Action.Submit')].data").ToList())
            {
                if (token is JObject data && data["scope"] == null)
                {
                    data["scope"] = scope;
                }
            }
        }

        /// <summary>
        /// The function that constructs an adaptive card based on adaptive elements. Attaches the elements in the order they are given
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private static string ConstructAdaptiveCard(List<AdaptiveElement> elements)
        {
            // Create a container to hold the elements
            var container = new AdaptiveContainer
            {
                Items = elements
            };

            // Create an Adaptive Card
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 6))
            {
                Body = [container]
            };

            // Serialize the card to JSON
            string cardJson = card.ToJson();
            return cardJson;
        }

        /// <summary>
        /// The function that creates an action set based on the provided actions
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        private static AdaptiveActionSet CreateActionSet(List<(string title, string style, AdaptiveCardSubmissionData data)> actions)
        {
            // Create an action set with the provided actions
            AdaptiveActionSet actionSet = new()
            {
                Actions = []
            };

            foreach (var (title, style, data) in actions)
            {
                actionSet.Actions.Add(new AdaptiveSubmitAction
                {
                    Title = title,
                    Data = data,
                    Style = style
                });
            }

            return actionSet;
        }

        /// <summary>
        /// The function that creates a text block based on the provided parameters
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wrap"></param>
        /// <param name="weight"></param>
        /// <param name="size"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private static AdaptiveTextBlock CreateTextBlock(string text, bool wrap = true, AdaptiveTextWeight weight = AdaptiveTextWeight.Default, AdaptiveTextSize size = AdaptiveTextSize.Default, AdaptiveTextBlockStyle style = AdaptiveTextBlockStyle.Paragraph)
        {
            // Create a text block with the provided parameters
            return new AdaptiveTextBlock
            {
                Text = text,
                Wrap = wrap,
                Weight = weight,
                Size = size,
                Style = style
            };
        }

        /// <summary>
        /// Create Adaptive Card Submission Data
        /// </summary>
        /// <param name="askRequest"></param>
        /// <param name="chatRequestContextArgs"></param>
        /// <param name="userContext"></param>
        /// <param name="userConsent"></param>
        /// <param name="url"></param>
        /// <param name="scope"></param>
        private AdaptiveCardSubmissionData CreateAdaptiveCardSubmissionData(
            AskRequest askRequest, 
            ChatRequestEventArgs chatRequestContextArgs, 
            string userContext, 
            ActionConsentType userConsent, 
            string url, 
            string scope)
        {
            return new AdaptiveCardSubmissionData
            {
                Url = url,
                Scope = scope,
                AskRequest = new AskRequest()
                {
                    UserId = askRequest.UserId,
                    UserName = askRequest.UserName,
                    UserEmail = askRequest.UserEmail,
                    Input = askRequest.Input,
                    ChatId = askRequest.ChatId,
                    History = askRequest.History,
                    AdditionalDetails = BuildAdditionalDetails(userContext, chatRequestContextArgs, userConsent),
                },
            };
        }

        /// <summary>
        /// Build additional details
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="chatRequestContextArgs"></param>
        /// <param name="userConsent"></param>
        private static Dictionary<string, string> BuildAdditionalDetails(
        string userContext,
        ChatRequestEventArgs chatRequestContextArgs,
        ActionConsentType userConsent)
        {
            var additionalDetails = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(userContext))
                additionalDetails["userContext"] = userContext;
            if (chatRequestContextArgs.CopilotErrorType != CopilotErrorType.None)
                additionalDetails["copilotErrorType"] = chatRequestContextArgs.CopilotErrorType.ToString();
            if (chatRequestContextArgs.TenantId != 0)
                additionalDetails["tenantId"] = chatRequestContextArgs.TenantId.ToString();
            if (!string.IsNullOrEmpty(chatRequestContextArgs.DocumentNumber))
                additionalDetails["documentNumber"] = chatRequestContextArgs.DocumentNumber;
            if (!string.IsNullOrEmpty(chatRequestContextArgs.DetailsData))
                additionalDetails["detailsData"] = chatRequestContextArgs.DetailsData;
            if (userConsent != ActionConsentType.No)
                additionalDetails["userConsent"] = userConsent.ToString();
            if (!string.IsNullOrEmpty(chatRequestContextArgs.OnBehalfUserAlias))
                additionalDetails["onBehalfUserAlias"] = chatRequestContextArgs.OnBehalfUserAlias;
            if (!string.IsNullOrEmpty(chatRequestContextArgs.OnBehalfUserId))
                additionalDetails["onBehalfUserId"] = chatRequestContextArgs.OnBehalfUserId;

            return additionalDetails;
        }

        #endregion Private Methods
    }
}