// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.CFS.Approvals.Contracts;
using OpenAI.Chat;

namespace Microsoft.CFS.Approvals.Model;

public class ChatTools
{
    /// <summary>
    /// The chat tool for handling errors.
    /// </summary>
    public static readonly ChatTool ErrorHandlerTool = ChatTool.CreateFunctionTool(
        functionName: nameof(ChatToolFunctionNames.OnErrorOccurred),
        functionDescription: "Handles errors by providing self-serve functionality to the users.");

    /// <summary>
    /// The chat tool that explains errors and determines if next steps are needed.
    /// </summary>
    public static readonly ChatTool ExplainAndAskPermissionTool = ChatTool.CreateFunctionTool(
        functionName: nameof(ChatToolFunctionNames.ExplainAndAskPermission),
        functionDescription: "Called when the user needs troubleshooting help");

    /// <summary>
    /// The chat tool for search functionality.
    /// </summary>
    public static readonly ChatTool SearchTool = ChatTool.CreateFunctionTool(
        functionName: nameof(ChatToolFunctionNames.GetAIAssistedSearchResults),
        functionDescription: "Searches pending approvals using natural language and returns summary-level results. Use for exploratory queries, listing multiple requests, filtering by criteria (e.g. high value, recent), or when the user wants to find or browse requests. Do NOT use for getting details of a specific known document number.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "userPrompt": {
                    "type": "string",
                    "description": "The user's search query"
                }
            },
            "required": [ "userPrompt" ]
        }
        """)
    );

    /// <summary>
    /// The chat tool for getting request details
    /// </summary>
    public static readonly ChatTool GetRequestDetailsTool = ChatTool.CreateFunctionTool(
        functionName: nameof(ChatToolFunctionNames.GetRequestDetails),
        functionDescription: "Retrieves full details and generates a rich adaptive card for a specific approval request. ALWAYS call this tool when the user mentions a document number or asks for details, even if prior details exist in history. TenantID is optional; supply only if explicitly known.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "documentNumber": {
                    "type": "string",
                    "description": "The DocumentNumber of the request, needed to query the request"
                },
                "tenantID": {
                    "type": "string",
                    "description": "The tenant id of a request (optional, only include if explicitly known)"
                },
                "shouldReturnAdaptiveCard": {
                    "type": "boolean",
                    "description": "Indicates whether an adaptive card should be returned"
                }
            },
            "required": [ "documentNumber" ]
        }
        """)
    );
}