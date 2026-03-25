// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;

public class QuickTourHelper : IQuickTourHelper
{
    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// The user preference helper
    /// </summary>
    private readonly IUserPreferenceHelper _userPreferenceHelper;

    /// <summary>
    /// The flighting data provider
    /// </summary>
    private readonly IFlightingDataProvider _flightingDataProvider;

    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logProvider"></param>
    /// <param name="userPreferenceHelper"></param>
    /// <param name="flightingDataProvider"></param>
    /// <param name="blobStorageHelper"></param>
    public QuickTourHelper(
        ILogProvider logProvider,
        IUserPreferenceHelper userPreferenceHelper,
        IFlightingDataProvider flightingDataProvider,
        IBlobStorageHelper blobStorageHelper)
    {
        _logProvider = logProvider;
        _userPreferenceHelper = userPreferenceHelper;
        _flightingDataProvider = flightingDataProvider;
        _blobStorageHelper = blobStorageHelper;
    }

    /// <summary>
    /// Get all quick tour features with its status (Is viewed or not)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="loggedInUpn"></param>
    /// <param name="alias"></param>
    /// <param name="clientDevice"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    public async Task<List<QuickTourFeatureWithStatus>> GetAllQuickTourFeatures(string sessionId, string loggedInUpn, string alias, string clientDevice, string domain)
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
            { LogDataKey.UserRoleName, loggedInUpn },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        #region Download Blob images for slides

        var templateList = new Dictionary<string, string>();
        await GetAllIconsFromBlob(templateList);

        #endregion Download Blob images for slides

        try
        {
            var listOfAllFlightingFeatures = _flightingDataProvider.GetAllFlightingFeature();

            var quickTourFeatureListWithStatus = new List<QuickTourFeatureWithStatus>();
            var userPreferenceForClient = _userPreferenceHelper.GetUserPreferences(loggedInUpn, clientDevice);

            List<string> quickTourFeatureList = userPreferenceForClient?.QuickTourFeatureList != null ? userPreferenceForClient?.QuickTourFeatureList.FromJson<List<string>>() : null;
            if (listOfAllFlightingFeatures.Any())
            {
                foreach (var flightingFeature in listOfAllFlightingFeatures)
                {
                    if (!string.IsNullOrEmpty(flightingFeature.QuickTourSlidesJson) || flightingFeature.TeachingCoach==true)
                    {
                        var quickTourObj = flightingFeature.QuickTourSlidesJson?.ToJObject();
                        if (flightingFeature.TeachingCoach == false)
                        {
                            
                            if (quickTourObj != null)
                            {
                                var slidesArr = quickTourObj["Slides"].ToJson().ToJArray();

                                foreach (var item in slidesArr)
                                {
                                    string imageBase64 = string.Empty;
                                    item["image"] = templateList.TryGetValue(item["image"].ToString(), out imageBase64) ? imageBase64 : item["image"];
                                }
                                quickTourObj["Slides"] = slidesArr;
                            }
                        }
                        string summaryImage = string.Empty;
                        quickTourFeatureListWithStatus.Add(new QuickTourFeatureWithStatus
                        {
                            Id = flightingFeature.Id,
                            Name = flightingFeature.FeatureName,
                            IsEnabled = _flightingDataProvider.IsFeatureEnabledForUser(alias, flightingFeature.Id, domain),
                            IsViewed = quickTourFeatureList != null && quickTourFeatureList.Any() && quickTourFeatureList.Contains(flightingFeature.RowKey),
                            Summary = quickTourObj?["Summary"]?.ToString() ?? string.Empty,
                            SummaryImage = (quickTourObj?["SummaryImage"] != null && templateList.TryGetValue(quickTourObj["SummaryImage"].ToString(), out summaryImage)) ? summaryImage : (quickTourObj?["SummaryImage"]?.ToString() ?? string.Empty),
                            Slides = quickTourObj != null ? quickTourObj["Slides"].ToJson().ToJArray() : null,
                        });
                    }
                }
            }

            // Log Success
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogInformation(TrackingEvent.WebApiGetAllQuickTourFeaturesWithStatusSuccess, logData);

            return quickTourFeatureListWithStatus;
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiGetAllQuickTourFeaturesWithStatusFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Gets the blob template by file name.
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>returns a slide base64 images </returns>
    private async Task GetAllIconsFromBlob(IDictionary<string, string> templateList)
    {
        try
        {
            var listBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.QuickTourSlides, "", null, null);

            foreach (var item in listBlobs)
            {
                var blobItemName = item.Name;
                var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
                var slide = await _blobStorageHelper.DownloadByteArray(Constants.QuickTourSlides, blobItemName);
                var base64String = Convert.ToBase64String(slide, 0, slide.Length);
                templateList[blobItemName] = string.Format("{0},{1}", "data:image/jpeg;base64", base64String);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.QuickTourSlidesFileName, ex);
            throw;
        }
    }
}