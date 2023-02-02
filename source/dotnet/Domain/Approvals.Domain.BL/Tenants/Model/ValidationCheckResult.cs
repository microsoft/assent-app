using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Model;

public class ValidationCheckResult
{
    /// <summary>
    /// Gets or sets whether the validatin was successful or fail.
    /// </summary>
    public bool ActionResult { get; set; }

    /// <summary>
    /// Gets or sets the error messages for the validation.
    /// </summary>
    public List<string> ErrorMessages { get; set; }
}
