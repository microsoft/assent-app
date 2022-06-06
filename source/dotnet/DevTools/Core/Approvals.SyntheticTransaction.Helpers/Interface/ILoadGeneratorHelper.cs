// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface
{
    using System.Threading.Tasks;

    public interface ILoadGeneratorHelper
    {
        Task<object> GenerateLoad(string tenant, string approver, int load, int batchsize, string samplePayload);
    }
}