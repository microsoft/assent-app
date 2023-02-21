// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;

/// <summary>
/// The User Image Retrieval class
/// </summary>
public class UserImageRetrieval : IImageRetriever
{
    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The local file cache helper
    /// </summary>
    private readonly ILocalFileCache _localFileCache;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserImageRetrieval"/> class.
    /// </summary>
    /// <param name="blobStorageHelper">The blob storage helper.</param>
    /// <param name="logProvider">The log provider.</param>
    /// <param name="localFileCache">The local file cache.</param>
    /// <param name="nameResolutionHelper">The name resolution helper.</param>
    public UserImageRetrieval(IBlobStorageHelper blobStorageHelper,
        ILogProvider logProvider,
        ILocalFileCache localFileCache,
        INameResolutionHelper nameResolutionHelper)
    {
        _blobStorageHelper = blobStorageHelper;
        _logProvider = logProvider;
        _localFileCache = localFileCache;
        _nameResolutionHelper = nameResolutionHelper;
    }

    #region Implemented Methods

    /// <summary>
    /// Get User Image Async.
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <returns></returns>
    public async Task<byte[]> GetUserImageAsync(string alias, string sessionId, string clientDevice)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();

        if (!string.IsNullOrEmpty(sessionId))
        {
            Tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, Tcv },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.UserRoleName, alias },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        byte[] photo = null;
        try
        {
            var employee = await _nameResolutionHelper.GetUser(alias);
            if (employee != null)
            {
                if (await _blobStorageHelper.DoesExist("userimages", alias))
                {
                    photo = await _blobStorageHelper.DownloadByteArray("userimages", alias);
                }
                else
                {
                    photo = await _nameResolutionHelper.GetUserImage(alias);
                    if (photo != null)
                    {
                        await _blobStorageHelper.UploadByteArray(photo, "userimages", alias);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiUserImageFail, ex, logData);
        }
        finally
        {
            if (photo == null)
            {
                photo = _localFileCache.GetFile(@"~/Content/images/blankUserImage.jpg");
            }
        }
        return photo;
    }

    #endregion Implemented Methods
}