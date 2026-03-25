// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// The UserFeedback class
/// </summary>
public class UserFeedback
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the feature name
    /// </summary>
    public string FeatureName { get; set; }

    /// <summary>
    /// Gets or sets the feedback inputs
    /// </summary>
    public List<FeedbackInput> Inputs { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the feedback is from a delegated user
    /// </summary>
    public bool IsDelegatedUser { get; set; }
    
    /// <summary>
    /// Gets or sets the client device
    /// </summary>
    public string ClientDevice { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the correlation id (Xcv)
    /// </summary>
    public string Xcv { get; set; }
    
    /// <summary>
    /// Gets or sets the approval identifier for partitioning data
    /// </summary>
    public string ApprovalIdentifier { get; set; }
    
    /// <summary>
    /// Gets or sets the document number (optional)
    /// </summary>
    public string DocumentNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the fiscal year (optional)
    /// </summary>
    public string FiscalYear { get; set; }
    
    /// <summary>
    /// Gets or sets the custom storage parameters for optional partition key and collection name
    /// </summary>
    public CustomStorageParameters CustomStorageParameters { get; set; }
}

/// <summary>
/// The FeedbackInput class
/// </summary>
public class FeedbackInput
{
    /// <summary>
    /// Gets or sets the input type
    /// </summary>
    public string InputType { get; set; }
    
    /// <summary>
    /// Gets or sets the input value
    /// </summary>
    public string InputValue { get; set; }
}

/// <summary>
/// Custom storage parameters for feedback storage
/// </summary>
public class CustomStorageParameters
{
    /// <summary>
    /// Gets or sets the partition key path for custom storage (e.g., "/FeatureName", "/DocumentNumber")
    /// </summary>
    public string PartitionKeyPath { get; set; }
    
    /// <summary>
    /// Gets or sets the collection name for custom storage
    /// </summary>
    public string CollectionName { get; set; }
}