// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Utils
{
    using System.Collections.Generic;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Operation filter to add the requirement of the custom header
    /// </summary>
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        /// <summary>
        /// Updated Swagger UI to add custom header
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

        }
    }
}