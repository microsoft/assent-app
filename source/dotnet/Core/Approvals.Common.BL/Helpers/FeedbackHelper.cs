// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Feedback Helper class
/// </summary>
public class FeedbackHelper : IFeedbackHelper
{
    /// <summary>
    /// The configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The log provider
    /// </summary>
    protected readonly ILogProvider _logProvider;

    /// <summary>
    /// The CosmosDb helper
    /// </summary>
    private readonly ICosmosDbHelper _cosmosDbHelper;

    /// <summary>
    /// Default database name
    /// </summary>
    private readonly string _defaultDatabaseName = "feedback";

    /// <summary>
    /// Default collection name
    /// </summary>
    private readonly string _defaultCollectionName = "general";

    /// <summary>
    /// Default partition key path - using FeatureName for better data distribution
    /// </summary>
    private readonly string _defaultPartitionKeyPath = "/FeatureName";

    /// <summary>
    /// Constructor of FeedbackHelper
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logProvider"></param>
    /// <param name="cosmosDbHelper"></param>
    public FeedbackHelper(
        IConfiguration config,
        ILogProvider logProvider,
        ICosmosDbHelper cosmosDbHelper)
    {
        _config = config;
        _logProvider = logProvider;
        _cosmosDbHelper = cosmosDbHelper;
        
        // Set default target for standard flow
        _cosmosDbHelper.SetTarget(_defaultDatabaseName, _defaultCollectionName, _defaultPartitionKeyPath);
    }

    /// <summary>
    /// Add user feedback (with optional custom storage parameters in the feedback object)
    /// </summary>
    /// <param name="feedback">The user feedback</param>
    /// <returns>Task</returns>
    public async Task AddFeedbackAsync(UserFeedback feedback)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.Xcv, feedback.Xcv },
            { LogDataKey.ClientDevice, feedback.ClientDevice },
            { LogDataKey.EventType, "UserFeedback" },
            { LogDataKey.CustomEventName, feedback.FeatureName }
        };

        // Add delegation status to log data
        logData.Add(LogDataKey.UserAlias, feedback.IsDelegatedUser ? "OnBehalfUser" : "SignedInUser");

        try
        {
            // Ensure ID is set
            if (feedback.Id == Guid.Empty)
            {
                feedback.Id = Guid.NewGuid();
            }
            
            // Set timestamp if not already set
            if (!feedback.Timestamp.HasValue)
            {
                feedback.Timestamp = DateTimeOffset.UtcNow;
            }
            
            // Store the feedback using CosmosDB
            await StoreFeedbackInCosmosDbAsync(feedback);
            
            _logProvider.LogInformation(TrackingEvent.FeedbackSubmissionSuccess, logData);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.ErrorMessage, ex.Message);
            _logProvider.LogError(TrackingEvent.FeedbackSubmissionFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Stores UserFeedback data in CosmosDB (with optional custom storage parameters in the feedback object)
    /// </summary>
    /// <param name="feedbackData">The feedback data.</param>
    /// <returns>Task</returns>
    private async Task StoreFeedbackInCosmosDbAsync(UserFeedback feedbackData)
    {
        // Ensure FeatureName is set for partitioning - use as default partition key value
        if (string.IsNullOrEmpty(feedbackData.FeatureName))
        {
            feedbackData.FeatureName = "unknown";
        }

        // Extract custom storage parameters before temporarily removing them from the data object
        var customStorageParameters = feedbackData.CustomStorageParameters;
        
        // Temporarily remove CustomStorageParameters to avoid storing configuration data
        feedbackData.CustomStorageParameters = null;
        
        try
        {
            // Handle custom parameters if provided, otherwise use defaults
            if (customStorageParameters != null && 
                (!string.IsNullOrEmpty(customStorageParameters.CollectionName) || 
                 !string.IsNullOrEmpty(customStorageParameters.PartitionKeyPath)))
            {
                // Use custom storage parameters
                string databaseName = _defaultDatabaseName;
                string collectionName = !string.IsNullOrEmpty(customStorageParameters.CollectionName) 
                    ? customStorageParameters.CollectionName 
                    : _defaultCollectionName;
                
                // Use custom partition key path if provided, otherwise use default
                string partitionKeyPath = !string.IsNullOrEmpty(customStorageParameters.PartitionKeyPath)
                    ? customStorageParameters.PartitionKeyPath
                    : _defaultPartitionKeyPath;
                
                // Use the optional parameters for custom storage scenarios
                await _cosmosDbHelper.InsertDocumentAsync(feedbackData, databaseName, collectionName, partitionKeyPath);
            }
            else
            {
                // Use default storage parameters with FeatureName as partition key
                // Use the default target set in constructor
                await _cosmosDbHelper.InsertDocumentAsync(feedbackData);
            }
        }
        finally
        {
            // Restore the original CustomStorageParameters value
            feedbackData.CustomStorageParameters = customStorageParameters;
        }
    }
}