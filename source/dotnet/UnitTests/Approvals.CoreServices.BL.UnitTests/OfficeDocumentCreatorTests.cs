// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.UnitTests;

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CFS.Approvals.CoreServices.BL.Helpers;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class OfficeDocumentCreatorTests
{
    private string tempRootPath = string.Empty;
    private OfficeDocumentCreator officeDocumentCreator = null!;

    [TestInitialize]
    public void Initialize()
    {
        tempRootPath = Path.Combine(Path.GetTempPath(), "OfficeDocumentCreatorTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRootPath);

        Mock<IHostingEnvironment> hostingEnvironmentMock = new();
        hostingEnvironmentMock.SetupGet(env => env.ApplicationName).Returns("Approvals.CoreServices.BL.UnitTests");
        hostingEnvironmentMock.SetupGet(env => env.EnvironmentName).Returns("UnitTest");
        hostingEnvironmentMock.SetupGet(env => env.ContentRootPath).Returns(tempRootPath);
        hostingEnvironmentMock.SetupGet(env => env.WebRootPath).Returns(tempRootPath);
        hostingEnvironmentMock.SetupGet(env => env.ContentRootFileProvider).Returns(new NullFileProvider());
        hostingEnvironmentMock.SetupGet(env => env.WebRootFileProvider).Returns(new NullFileProvider());

        officeDocumentCreator = new OfficeDocumentCreator(hostingEnvironmentMock.Object);
    }

    [TestMethod]
    public void GetDocumentURL_ShouldCreateFileInPreviewDocumentsAndReturnExpectedFileName()
    {
        byte[] officeDocumentContent = [1, 2, 3, 4, 5];
        string displayDocumentNumber = "DOC123";
        string attachmentName = "invoice.docx";
        string loggedInAlias = "jdoe";
        string sessionId = "session-1";

        string result = officeDocumentCreator.GetDocumentURL(
            officeDocumentContent,
            displayDocumentNumber,
            attachmentName,
            loggedInAlias,
            sessionId);

        string expectedFileName = "DOC123_jdoe_invoice.docx";
        string expectedPath = Path.Combine(tempRootPath, "PreviewDocuments", expectedFileName);

        Assert.AreEqual(expectedFileName, result);
        Assert.IsTrue(File.Exists(expectedPath));
        CollectionAssert.AreEqual(officeDocumentContent, File.ReadAllBytes(expectedPath));
    }

    [TestMethod]
    public void GetDocumentURL_ShouldThrowArgumentException_WhenAttachmentNameContainsPathTraversal()
    {
        byte[] officeDocumentContent = [1, 2, 3];
        string displayDocumentNumber = "DOC123";
        string attachmentName = "../invoice.docx";
        string loggedInAlias = "jdoe";
        string sessionId = "session-1";

        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
            officeDocumentCreator.GetDocumentURL(
                officeDocumentContent,
                displayDocumentNumber,
                attachmentName,
                loggedInAlias,
                sessionId));

        Assert.AreEqual("attachmentName", exception.ParamName);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(tempRootPath))
        {
            Directory.Delete(tempRootPath, true);
        }
    }
}
