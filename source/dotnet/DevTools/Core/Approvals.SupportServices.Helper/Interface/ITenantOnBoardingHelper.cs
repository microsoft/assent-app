// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;

    public interface ITenantOnBoardingHelper
    {
        Task<bool> TenantOnBoarding(JObject applicationDetail, IFormFileCollection files);

        Task<string> GetTenantList(string tenantType);
    }
}