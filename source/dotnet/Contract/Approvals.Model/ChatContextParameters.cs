// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using OpenAI.Chat;

/// <summary>
/// The Chat ContextParameters object
/// </summary>
public class ChatContextParameters
{
    public ChatCompletionOptions ChatCompletionOptions { get; set; }
    public string SystemMessage { get; set; }
    public string UserMessage { get; set; }
    public string ModelDeploymentName { get; set; }

}
