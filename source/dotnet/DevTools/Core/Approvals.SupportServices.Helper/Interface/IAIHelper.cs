// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    public interface IAIHelper
    {
        JObject GetAIData(string customQueryParam, List<JToken> aiScopes);
    }
}
