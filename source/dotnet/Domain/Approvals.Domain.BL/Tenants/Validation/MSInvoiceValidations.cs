// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Validations;

using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

public class MSInvoiceValidations : ValidationBase
{
    public MSInvoiceValidations(IConfiguration config, INameResolutionHelper nameResolutionHelper) : base(config, nameResolutionHelper)
    {
    }

    /// <summary>
    /// Checks if the input alias of the user is from a microsoft domain or not
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>True if the alias is from a non-microsoft domain, else False</returns>
    public override bool IsExternalUser(string alias)
    {
        // TODO:: Handle all domains for Microsoft
        return alias.Contains("@") && !alias.EndsWith(_config[ConfigurationKey.DomainName.ToString()]);
    }
}