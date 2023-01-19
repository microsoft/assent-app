// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The Harness Environment Controller
/// </summary>
[ApiController]
[Route("api/v1/TestHarnessEnvironment")]
public class TestHarnessEnvironment : ControllerBase
{
    public TestHarnessEnvironment()
    {

    }

    /// <summary>
    /// Get environments
    /// </summary>
    /// <param name="env"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetEnvironment")]
    public IActionResult GetEnvironment(string env)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(env))
            {
                throw new ArgumentNullException("env");
            }
            return Ok(env);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}