// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

public interface IErrorHandlerHelper
{
    /// <summary>
    /// The function that is invoked when an event is raised. Directs to the correct scenario
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="chatRequestEventArgs"></param>
    /// <returns></returns>
    Task<string> ErrorOrchestrator(object sender, ChatRequestEventArgs chatRequestEventArgs);

    /// <summary>
    /// The function that the assistant calls to update a request as out of sync
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="chatRequestEventArgs"></param>
    /// <returns></returns>
    Task<string> ErrorHandler_OutOfSync(object sender, ChatRequestEventArgs chatRequestEventArgs);
}