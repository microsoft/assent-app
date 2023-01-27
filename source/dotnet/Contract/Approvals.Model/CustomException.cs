// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;

[Serializable]
public class CustomException : Exception
{
    public string _errorMessage;
    public int _errorNumber;

    public CustomException()
    { }

    public CustomException(string errorMessage)
    {
        _errorMessage = errorMessage;
    }

    public CustomException(string errorMessage, int errorNumber)
    {
        _errorMessage = errorMessage;
        _errorNumber = errorNumber;
    }
}