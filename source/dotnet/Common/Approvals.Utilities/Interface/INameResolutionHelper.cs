// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    public interface INameResolutionHelper
    {
        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        Task<Graph.User> GetUser(string alias);

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        Task<string> GetUserName(string alias);

        /// <summary>
        /// Get User Image.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        Task<byte[]> GetUserImage(string alias);

        /// <summary>
        /// Check if User is valid
        /// </summary>
        /// <param name="Alias"></param>
        /// <returns></returns>
        Task<bool> IsValidUser(string Alias);
    }
}