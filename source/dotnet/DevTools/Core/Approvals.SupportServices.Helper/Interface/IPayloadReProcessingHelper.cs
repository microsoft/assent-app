// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    public interface IPayloadReProcessingHelper
    {
        List<dynamic> GetPayloadHistory(string tenantID, string documentCollection, string fromDate, string toDate, string collection);
        bool ReProcessMessage(JObject jObject);
    }
}
