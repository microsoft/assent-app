// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public interface ISubscribeFeaturesHelper
{
    Task<List<string>> ManageFeatureSubscription(JToken featureDetail);

    bool IsFeatureEnabledForUser(string alias, int featureID);
}