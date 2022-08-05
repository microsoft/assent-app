// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The Local File Cache class
    /// </summary>
    public class LocalFileCache : ILocalFileCache
    {
        /// <summary>
        /// Cached files
        /// </summary>
        private readonly Dictionary<string, byte[]> _cachedFiles;

        /// <summary>
        /// The hosting environment
        /// </summary>
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Constructor of LocalFileCache
        /// </summary>
        /// <param name="hostEnvironment"></param>
        public LocalFileCache(IHostingEnvironment hostEnvironment)
        {
            _hostingEnvironment = hostEnvironment;
            _cachedFiles = new Dictionary<string, byte[]>();
        }

        #region Implemented Methods
        /// <summary>
        /// Get file.
        /// </summary>
        /// <param name="pathLocal"></param>
        /// <returns></returns>
        public byte[] GetFile(string pathLocal)
        {
            if (_cachedFiles.ContainsKey(pathLocal))
            {
                return _cachedFiles[pathLocal];
            }
            else
            {
                if (CacheFile(pathLocal))
                {
                    return GetFile(pathLocal);
                }
                else
                    return null;
            }
        }

        #endregion Implemented Methods

        #region LocalFileCache Methods
        /// <summary>
        /// Cache file.
        /// </summary>
        /// <param name="pathLocal"></param>
        /// <returns></returns>
        public bool CacheFile(string pathLocal)
        {
            try
            {
                var filestream = File.Open(_hostingEnvironment.ContentRootPath + pathLocal, FileMode.Open);
                var file = new byte[(int)filestream.Length];
                filestream.Read(file, 0, (int)filestream.Length);
                filestream.Close();
                if (file != null)
                {
                    if (!_cachedFiles.ContainsKey(pathLocal))
                        _cachedFiles.Add(pathLocal, file);
                    else
                        _cachedFiles[pathLocal] = file;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion LocalFileCache Methods

    }
}
