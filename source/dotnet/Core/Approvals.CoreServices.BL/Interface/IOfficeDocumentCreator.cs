// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface;

public interface IOfficeDocumentCreator
{
    /// <summary>
    /// Get Document URL.
    /// </summary>
    /// <param name="officeDocumentContent"></param>
    /// <param name="displayDocumentNumber"></param>
    /// <param name="attachmentName"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    string GetDocumentURL(byte[] officeDocumentContent,
        string displayDocumentNumber,
        string attachmentName,
        string loggedInAlias,
        string sessionId);
}