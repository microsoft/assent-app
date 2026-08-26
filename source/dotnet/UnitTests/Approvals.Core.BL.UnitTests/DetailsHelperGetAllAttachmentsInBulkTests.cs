// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.UnitTests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Helpers;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

/// <summary>
/// Security regression test for the V11 bulk-attachment-download IDOR fix: a caller must not be able
/// to retrieve another user's attachments by supplying an arbitrary UserAlias unless server-side
/// delegation validation (IDelegationHelper.CheckUserAuthorization) actually grants access.
/// </summary>
[TestClass]
public class DetailsHelperGetAllAttachmentsInBulkTests
{
    private Mock<IDelegationHelper> mockDelegationHelper = null!;
    private Mock<IActionAuditLogHelper> mockActionAuditLogHelper = null!;
    private Mock<ILogProvider> mockLogProvider = null!;
    private Mock<IPerformanceLogger> mockPerformanceLogger = null!;
    private Mock<IApprovalTenantInfoHelper> mockApprovalTenantInfoHelper = null!;
    private Mock<IConfiguration> mockConfiguration = null!;
    private Mock<INameResolutionHelper> mockNameResolutionHelper = null!;
    private Mock<IApprovalDetailProvider> mockApprovalDetailProvider = null!;
    private Mock<IFlightingDataProvider> mockFlightingDataProvider = null!;
    private Mock<IEditableConfigurationHelper> mockEditableConfigurationHelper = null!;
    private Mock<ISummaryHelper> mockSummaryHelper = null!;
    private Mock<IApprovalHistoryProvider> mockApprovalHistoryProvider = null!;
    private Mock<ITenantFactory> mockTenantFactory = null!;
    private Mock<IImageRetriever> mockImageRetriever = null!;
    private Mock<IBlobStorageHelper> mockBlobStorageHelper = null!;
    private Mock<IHttpHelper> mockHttpHelper = null!;
    private Mock<IReadDetailsHelper> mockReadDetailsHelper = null!;
    private DetailsHelper detailsHelper = null!;

    [TestInitialize]
    public void Initialize()
    {
        mockDelegationHelper = new Mock<IDelegationHelper>();
        mockActionAuditLogHelper = new Mock<IActionAuditLogHelper>();
        mockLogProvider = new Mock<ILogProvider>();
        mockPerformanceLogger = new Mock<IPerformanceLogger>();
        mockApprovalTenantInfoHelper = new Mock<IApprovalTenantInfoHelper>();
        mockConfiguration = new Mock<IConfiguration>();
        mockNameResolutionHelper = new Mock<INameResolutionHelper>();
        mockApprovalDetailProvider = new Mock<IApprovalDetailProvider>();
        mockFlightingDataProvider = new Mock<IFlightingDataProvider>();
        mockEditableConfigurationHelper = new Mock<IEditableConfigurationHelper>();
        mockSummaryHelper = new Mock<ISummaryHelper>();
        mockApprovalHistoryProvider = new Mock<IApprovalHistoryProvider>();
        mockTenantFactory = new Mock<ITenantFactory>();
        mockImageRetriever = new Mock<IImageRetriever>();
        mockBlobStorageHelper = new Mock<IBlobStorageHelper>();
        mockHttpHelper = new Mock<IHttpHelper>();
        mockReadDetailsHelper = new Mock<IReadDetailsHelper>();

        mockApprovalTenantInfoHelper
            .Setup(x => x.GetTenantInfo(It.IsAny<int>()))
            .Returns(new ApprovalTenantInfo { AppName = "TestApp", BusinessProcessName = "{0}.{1}" });

        detailsHelper = new DetailsHelper(
            mockDelegationHelper.Object,
            mockActionAuditLogHelper.Object,
            mockLogProvider.Object,
            mockPerformanceLogger.Object,
            mockApprovalTenantInfoHelper.Object,
            mockConfiguration.Object,
            mockNameResolutionHelper.Object,
            mockApprovalDetailProvider.Object,
            mockFlightingDataProvider.Object,
            mockEditableConfigurationHelper.Object,
            mockSummaryHelper.Object,
            mockApprovalHistoryProvider.Object,
            mockTenantFactory.Object,
            mockImageRetriever.Object,
            mockBlobStorageHelper.Object,
            mockHttpHelper.Object,
            mockReadDetailsHelper.Object);
    }

    /// <summary>
    /// A caller (attacker) requesting another user's (victim) attachments must be rejected with
    /// UnauthorizedAccessException when server-side delegation validation denies the grant, and the
    /// request must never reach the tenant adapter.
    /// </summary>
    [TestMethod]
    public async Task GetAllAttachmentsInBulk_UnauthorizedDelegate_ThrowsAndNeverCallsTenant()
    {
        mockDelegationHelper
            .Setup(x => x.CheckUserAuthorization(It.IsAny<User>(), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedAccessException("User doesn't have permission to see the report."));

        var attackerUser = new User { MailNickname = "attacker", UserPrincipalName = "attacker@microsoft.com", Id = Guid.NewGuid().ToString() };
        var approvalRequests = new List<ApprovalRequest>
        {
            new ApprovalRequest { ApprovalIdentifier = new ApprovalIdentifier { DisplayDocumentNumber = "DOC1" } }
        };

        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() =>
            detailsHelper.GetAllAttachmentsInBulk(
                1, string.Empty, string.Empty, JsonConvert.SerializeObject(approvalRequests),
                "victimAlias", attackerUser, "Web", string.Empty, Guid.NewGuid().ToString(), "@microsoft.com"));

        mockTenantFactory.Verify(
            x => x.GetTenant(It.IsAny<ApprovalTenantInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
