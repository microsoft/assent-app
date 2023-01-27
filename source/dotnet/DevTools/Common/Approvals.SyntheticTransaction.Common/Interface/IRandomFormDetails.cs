// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Interface;
using System.Collections.Generic;

public interface IRandomFormDetails
{
    Dictionary<string, object> CreateFormData(string placeholder);
    int RandomNumber(int min = int.MinValue, int max = int.MaxValue);
}