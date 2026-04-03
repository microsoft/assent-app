// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    /// <summary>
    /// Helper that orchestrates AI-assisted search: derives structured filters via LLM then fetches matching approval requests.
    /// </summary>
    public class AIAssistedSearchHelper : IAIAssistedSearchHelper
    {
        #region Variables

        private readonly ILogProvider _logProvider;
        private readonly IPerformanceLogger _performanceLogger;
        private readonly IConfiguration _config;
        private readonly IIntelligenceHelper _intelligenceHelper;
        private readonly ISearchHelper _searchHelper;
        private readonly IApprovalTenantInfoHelper _tenantInfoHelper;
        private readonly IDelegationHelper _delegationHelper;

        #endregion Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAssistedSearchHelper"/> class.
        /// </summary>
        public AIAssistedSearchHelper(
            ILogProvider logProvider,
            IPerformanceLogger performanceLogger,
            IConfiguration config,
            IIntelligenceHelper intelligenceHelper,
            ISearchHelper searchHelper,
            IApprovalTenantInfoHelper tenantInfoHelper,
            IDelegationHelper delegationHelper)
        {
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;
            _config = config;
            _intelligenceHelper = intelligenceHelper;
            _searchHelper = searchHelper;
            _tenantInfoHelper = tenantInfoHelper;
            _delegationHelper = delegationHelper;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Executes AI-assisted search: obtains filters from the model then retrieves results via search helper.
        /// </summary>
        /// <typeparam name="T">Result element type (document numbers or summary objects).</typeparam>
        /// <param name="signedInUser">Signed in user.</param>
        /// <param name="onBehalfUser">On behalf user.</param>
        /// <param name="query">Raw search query.</param>
        /// <param name="returnType">Desired return type.</param>
        /// <param name="domain">User domain.</param>
        /// <param name="host">Client host identifier.</param>
        /// <param name="oauth2UserToken">OAuth2 user token.</param>
        /// <returns>List of results of type <typeparamref name="T"/>.</returns>
        public async Task<List<T>> GetAIAssistedSearchResults<T>(User signedInUser, User onBehalfUser, string query, SearchResultReturnType returnType, string domain, string host, string oauth2UserToken)
        {
            #region Logging initiation event

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.InputPrompt, query },
                { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName }
            };
            _logProvider.LogInformation(TrackingEvent.GetAIAssistedSearchResultsInitiated, logData);

            #endregion Logging initiation event

            try
            {
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, nameof(AIAssistedSearchHelper), nameof(GetAIAssistedSearchResults)), logData))
                {
                    await _delegationHelper.CheckUserAuthorization(
                        signedInUser,
                        onBehalfUser,
                        oauth2UserToken,
                        host,
                        string.Empty,
                        string.Empty,
                        string.Empty);
                    var (messages, options, modelName) = await BuildMessagesAndOptionsAsync(query);
                    var searchFilters = await GenerateSearchFiltersAsync(messages, options, modelName);
                    var searchResults = await GetResultsAsync<T>(searchFilters, returnType, domain, signedInUser, onBehalfUser, host, oauth2UserToken);

                    #region Logging success event

                    _logProvider.LogInformation(TrackingEvent.GetAIAssistedSearchResultsSuccess, logData);

                    #endregion Logging success event

                    return searchResults;
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.GetAIAssistedSearchResultsFailed, ex, logData);
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Builds system/user messages and completion options for deriving search filters.
        /// </summary>
        private async Task<(List<ChatMessage> messages, ChatCompletionOptions options, string modelName)> BuildMessagesAndOptionsAsync(string userPrompt)
        {
            string modelName = _config[ConfigurationKey.DeepSearchModelName.ToString()];
            string filtersSchema = _config[ConfigurationKey.SearchFiltersSchema.ToString()];

            var systemMessage = await BuildDeepSearchPromptAsync();
            var messages = _intelligenceHelper.BuildMessages(systemMessage, userPrompt);

            var options = _intelligenceHelper.CreateOptionsFromConfig(ConfigurationKey.AzureOpenAICompletionsOptions.ToString());
            options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("searchFilters", BinaryData.FromString(filtersSchema));

            return (messages, options, modelName);
        }

        /// <summary>
        /// Invokes the model to obtain structured filters and attempts deserialization.
        /// </summary>
        private async Task<Filters> GenerateSearchFiltersAsync(List<ChatMessage> messages, ChatCompletionOptions options, string modelName)
        {
            var completion = await _intelligenceHelper.CompleteAsync(modelName, messages, options);
            var raw = completion?.Content?.Count > 0 ? completion.Content[0].Text : null;
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try { return JsonSerializer.Deserialize<Filters>(raw); }
            catch (Exception ex) //TODO: Change event name/ should there be an exception caught here?
            {
                _logProvider.LogWarning(TrackingEvent.DeepSearchRequestFailed, new Dictionary<LogDataKey, object> { { LogDataKey.ResponseContent, "FiltersDeserializeFailed:" + ex.Message } });
                return null;
            }
        }

        /// <summary>
        /// Retrieves search results based on derived filters and requested return type.
        /// </summary>
        private async Task<List<T>> GetResultsAsync<T>(Filters searchFilters, SearchResultReturnType returnType, string domain, User signedInUser, User onBehalfUser, string host, string oauth2UserToken)
        {
            if (returnType == SearchResultReturnType.DocumentNumbers)
            {
                return await _searchHelper.GetSearchResultsDocumentNumbersAsync(searchFilters, domain, signedInUser, onBehalfUser, host, oauth2UserToken) as List<T>;
            }
            return await _searchHelper.GetSearchResultSummaryObjectsAsync(searchFilters, domain, signedInUser, onBehalfUser, host, oauth2UserToken) as List<T>;
        }

        /// <summary>
        /// Constructs the deep search system prompt including tenant names and configured rules.
        /// </summary>
        private async Task<string> BuildDeepSearchPromptAsync()
        {
            //TODO: Should change this method to take in the fields it needs instead of loading everything here.
            List<string> tenantNames = await _tenantInfoHelper.GetNames();
            return _config[ConfigurationKey.DeepSearchScope.ToString()] + "\n\n"
                + _config[ConfigurationKey.DeepSearchOperatorsAndLogic.ToString()] + "\n\n"
                + _config[ConfigurationKey.DeepSearchIntentInterpretation.ToString()] + "\n\n"
                + _config[ConfigurationKey.DeepSearchDataRules.ToString()] + "\n\n"
                + _config[ConfigurationKey.DeepSearchExamples.ToString()] + "\n\n"
                + "The current Date Time (UTC) is " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\n\n"
                + "Use the following list of known app name keywords to guide matching: " + string.Join(", ", tenantNames);
        }

        #endregion Private Methods
    }
}