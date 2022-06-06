// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface
{
    public interface ILocalFileCache
    {
        /// <summary>
        /// Get file
        /// </summary>
        /// <param name="pathLocal"></param>
        /// <returns></returns>
        byte[] GetFile(string pathLocal);
    }
}
