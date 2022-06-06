// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface
{
    using System.Threading.Tasks;

    public interface IImageRetriever
    {
        /// <summary>
        /// Gets the user image asynchronous.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="clientDevice">The client device.</param>
        /// <returns></returns>
        Task<byte[]> GetUserImageAsync(string alias, string sessionId, string clientDevice);
    }
}
