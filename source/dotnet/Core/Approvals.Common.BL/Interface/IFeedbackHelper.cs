// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Interface for feedback helper
/// </summary>
public interface IFeedbackHelper
{
    /// <summary>
    /// Add user feedback (with optional custom storage parameters in the feedback object)
    /// </summary>
    /// <param name="feedback">The user feedback</param>
    /// <returns>Task</returns>
    Task AddFeedbackAsync(UserFeedback feedback);
}