// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers;

using System;
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
        string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "PreviewDocuments");

        DirectoryInfo dirInfo = new DirectoryInfo(filePath);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        string safeDisplayDocumentNumber = SanitizePathSegment(displayDocumentNumber, nameof(displayDocumentNumber));
        string safeAttachmentName = SanitizePathSegment(attachmentName, nameof(attachmentName));
        string safeLoggedInAlias = SanitizePathSegment(loggedInAlias, nameof(loggedInAlias));

        string fileName = safeDisplayDocumentNumber + "_" + safeLoggedInAlias + "_" + safeAttachmentName;
        string candidatePath = Path.Combine(filePath, fileName);

        string canonicalBasePath = Path.GetFullPath(filePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string canonicalCandidatePath = Path.GetFullPath(candidatePath);

        if (!canonicalCandidatePath.StartsWith(canonicalBasePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid path: the target file path is outside the preview directory.");
        }

        if (!File.Exists(canonicalCandidatePath))
        {
            File.WriteAllBytes(canonicalCandidatePath, officeDocumentContent);
        }

        return fileName;
    }

    private static string SanitizePathSegment(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", parameterName);
        }

        if (value.IndexOf("..", StringComparison.Ordinal) >= 0 ||
            value.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
            value.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
        {
            throw new ArgumentException("Path traversal characters are not allowed.", parameterName);
        }

        string fileNameOnly = Path.GetFileName(value);

        if (!string.Equals(fileNameOnly, value, StringComparison.Ordinal))
        {
            throw new ArgumentException("Only file name segments are allowed.", parameterName);
        }

        return fileNameOnly;
    }
}