// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

public interface IKeyVaultHelper
{
    Dictionary<string, AppSettings> GetKeyVault();
}
