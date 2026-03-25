// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.CFS.Approvals.Model;

// Delegate for error event handler
public delegate Task<string> ErrorHandler(object sender, ChatRequestEventArgs e);

public class Events
{
    // Event for notifying errors
    public static event ErrorHandler ErrorOccurred;

    /// <summary>
    /// The event that the assistant calls when an error occurs
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task<string> OnErrorOccurred(ChatRequestEventArgs e)
    {
        return await ErrorOccurred?.Invoke(this, e);
    }
}