// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IAuditFactory
{
    /// <summary>
    /// This method will create the instance of AuditLogger
    /// </summary>
    IAuditLogger GetAuditLogger();
}
