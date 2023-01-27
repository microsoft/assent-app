// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.SupportService.API.Filters;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.SupportServices.Helper.ModelBinder;
using Newtonsoft.Json.Linq;

/// <summary>
/// Mark Request Out of Sync Controller
/// </summary>
[Route("api/v1/MarkRequestOutOfSync/{env}")]
[ApiController]
[TypeFilter(typeof(AuthorizationFilter))]
public class MarkRequestOutOfSyncController : ControllerBase
{
    /// <summary>
    /// Mark request out of sync helper
    /// </summary>
    private readonly IMarkRequestOutOfSyncHelper _markRequestOutOfSyncHelper;

    /// <summary>
    /// Constructor of MarkRequestOutOfSyncController
    /// </summary>
    /// <param name="markRequestOutOfSyncHelper"></param>
    public MarkRequestOutOfSyncController(IMarkRequestOutOfSyncHelper markRequestOutOfSyncHelper)
    {
        _markRequestOutOfSyncHelper = markRequestOutOfSyncHelper;
    }

    /// <summary>
    /// Mark multiple requests out of sync
    /// </summary>
    /// <param name="requestBody"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Post([ModelBinder(BinderType = typeof(JsonModelBinder))] JObject requestBody)
    {
        string documentNumber = string.Empty;
        Dictionary<string, string> result = new Dictionary<string, string>();
        try
        {
            var uploadedFile = Request.Form.Files.GetFiles("uploadedFile").FirstOrDefault();
            if (uploadedFile != null)
            {
                using (var reader = new StreamReader(uploadedFile.OpenReadStream()))
                {
                    documentNumber = reader.ReadToEnd();
                }
            }
            else
            {
                documentNumber = requestBody["documentNumber"]?.ToString();
            }
            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                return BadRequest("Please provide document number(s).");
            }
            if (string.IsNullOrWhiteSpace(requestBody["tenantID"]?.ToString()))
            {
                return BadRequest("Please select tenant to mark request out of sync.");
            }
            char[] delmChars = { ' ', ',', '.', ';' };
            List<string> documents = documentNumber.Trim().Split(delmChars).ToList();
            string approver = (!string.IsNullOrWhiteSpace(requestBody["approver"]?.ToString()) ? requestBody["approver"].ToString() : string.Empty);
            int tenantID = Convert.ToInt32(requestBody["tenantID"]?.ToString());
            result = _markRequestOutOfSyncHelper.MarkRequestsOutOfSync(documents, approver, tenantID);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}