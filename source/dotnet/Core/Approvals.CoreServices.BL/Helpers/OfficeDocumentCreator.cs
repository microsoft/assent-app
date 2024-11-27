// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers;

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;

/// <summary>
/// Office Document Creator class
/// </summary>
public class OfficeDocumentCreator : IOfficeDocumentCreator
{
    /// <summary>
    /// The hosting environment
    /// </summary>
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    /// Constructor of OfficeDocumentCreator
    /// </summary>
    /// <param name="hostEnvironment"></param>
    public OfficeDocumentCreator(IHostingEnvironment hostEnvironment)
    {
        _hostingEnvironment = hostEnvironment;
    }

    /// <summary>
    /// Get document URL.
    /// </summary>
    /// <param name="officeDocumentContent"></param>
    /// <param name="displayDocumentNumber"></param>
    /// <param name="attachmentName"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public string GetDocumentURL(byte[] officeDocumentContent, string displayDocumentNumber, string attachmentName, string loggedInAlias, string sessionId)
    {
        string filePath = _hostingEnvironment.WebRootPath + @"\PreviewDocuments";
        DirectoryInfo dirInfo = new DirectoryInfo(filePath);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }
        string filename = displayDocumentNumber + "_" + loggedInAlias + "_" + attachmentName;
        
        // CodeQL [SM00395] False Positive: Path is not controlled by user inputs
        if (!File.Exists(filePath + @"\" + filename))
        {
            // CodeQL [SM00395] False Positive: Path is not controlled by user inputs
            File.WriteAllBytes(filePath + @"\" + filename, officeDocumentContent);
        }
        return filename;
    }
}
