// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using NJsonSchema;

public interface ISchemaGenerator
{
    JsonSchema Generate(string json);
}
