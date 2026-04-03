// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CFS.Approvals.Contracts;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

public interface IARConverterFactory
{
    IARConverter GetARConverter(ConfigurationKey arConverterClassName = ConfigurationKey.ARConverterClass);
}
