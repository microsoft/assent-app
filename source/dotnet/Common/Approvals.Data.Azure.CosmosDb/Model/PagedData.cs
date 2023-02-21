// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Model;

using System.Collections.Generic;

/// <summary>
/// The PagedData class
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedData<T>
{
    public PagedData()
    {
        Result = new List<T>();
    }

    public List<T> Result { get; set; }

    public double TotalCount { get; set; }

    public string ContinuationToken { get; set; }
}