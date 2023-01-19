// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiverService.Utils;

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

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "ReportId",
            In = ParameterLocation.Header,
            Schema = new OpenApiSchema() { Type = "String" },
            Required = false // set to false if this is optional
        });
    }
}